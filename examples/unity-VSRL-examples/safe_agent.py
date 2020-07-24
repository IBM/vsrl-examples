#
# Copyright (C) 2020 IBM. All Rights Reserved.
#
# See LICENSE.txt file in the root directory
# of this source tree for licensing information.
#

import csv
import os
import time

import gym
import matplotlib.pyplot as plt
import tensorflow as tf
from gym_unity.envs import UnityEnv
from stable_baselines import PPO2
from stable_baselines.bench import Monitor
from stable_baselines.common.policies import ActorCriticPolicy, mlp_extractor
from stable_baselines.common.tf_layers import linear
from stable_baselines.common.vec_env import DummyVecEnv

import sys
sys.path.append(".")
from safe_environment import DroneEnv

T = 0.1
num_dogs = 3
dog_start_idx = 4
tree_start_idx = dog_start_idx + num_dogs * 3
num_trees = 3

# Safe policy of three layers of size 256 each
class SafePolicy(ActorCriticPolicy):

    constraint_func = None
    constrained_sample = None

    def __init__(self, sess, ob_space, ac_space, n_env, n_steps, n_batch, act_fun=tf.nn.tanh, reuse=False, **kwargs):
        super().__init__(sess, ob_space, ac_space, n_env, n_steps, n_batch, reuse=reuse)
        layers = [256, 256, 256]
        net_arch = [dict(vf=layers, pi=layers)]

        with tf.variable_scope("model", reuse=reuse):
            pi_latent, vf_latent = mlp_extractor(tf.layers.flatten(self.processed_obs), net_arch, act_fun)

            self._value_fn = linear(vf_latent, 'vf', 1)

            self._proba_distribution, self._policy, self.q_value = \
                self.pdtype.proba_distribution_from_latent(pi_latent, vf_latent, init_scale=0.01)

        self._setup_init()

    def step(self, obs, state=None, mask=None, deterministic=False):

        action, value, neglogp = self.sess.run([self.action, self.value_flat, self.neglogp],
                                                   {self.obs_ph: obs})
        if PlotYN:
            plt.clf()
            for i in range(dog_start_idx, dog_start_idx + 3 * num_dogs, 3):
                ox = obs[0][i+1]
                oy = obs[0][i + 2]
                plt.plot(ox, oy, 's',color='k',label='Dog/Kid')
            for i in range(tree_start_idx, tree_start_idx + 3 * num_trees, 3):
                ox = obs[0][i+1]
                oy = obs[0][i + 2]
                plt.plot(ox, oy, 'o',color='r',label='Tree')

        if PlotYN:
            plt.plot(obs[0][1],obs[0][2],'x',label='DronePosition')
            plt.plot(obs[0][1]+action[0][0]*T, obs[0][2]+action[0][1]*T,'sy',label='Suggested Action')

        if (not self.constraint_func(obs[0], action[0])) and UseVSRL:
            action = self.constrained_sample(obs[0])

        if PlotYN:
            plt.plot([obs[0][1]+action[0][0]*T],[obs[0][2]+action[0][1]*T],'og',MarkerSize=15,MarkerFaceColor='None',label='Chosen Safe Action')
            plt.plot([obs[0][1],obs[0][1]+action[0][0]*T],[obs[0][2],obs[0][2]+action[0][1]*T],'g')
            plt.xlim([-2,2])
            plt.ylim([-2,0])
            plt.legend(bbox_to_anchor=(0.5, 1.1), loc='upper center', ncol=3)
            plt.draw()
            plt.pause(0.001)
        return action, value, self.initial_state, neglogp

    def proba_step(self, obs, state=None, mask=None):
        return self.sess.run(self.policy_proba, {self.obs_ph: obs})

    def value(self, obs, state=None, mask=None):
        return self.sess.run(self.value_flat, {self.obs_ph: obs})


def DistFromObstacles(obs):
    d=[]
    for i in range(dog_start_idx, dog_start_idx + 3 * num_dogs, 3):
        d.append(obs[i+3])

    for i in range(tree_start_idx, tree_start_idx + 3 * num_trees, 3):
        d.append(obs[i+3])
    return d


class SafeMonitor(Monitor):
    def __init__(self,
                 env: gym.Env,
                 filename: str):
        super().__init__(env=env, filename=filename)
        self.EpisodeCollisions=0
        self.TotalCollisions=[]

        self.logger = csv.DictWriter(self.file_handler,
                                         fieldnames=('r', 'l', 't','Collisions') )
        self.logger.writeheader()

    def step(self, action):
        if self.needs_reset:
            raise RuntimeError("Tried to step environment that needs reset")
        observation, reward, done, info = self.env.step(action)

        self.rewards.append(reward)

        if min(DistFromObstacles(observation)) < .3:
            self.EpisodeCollisions+=1

        if done:
            self.needs_reset = True
            ep_rew = sum(self.rewards)
            eplen = len(self.rewards)
            ep_info = {"r": round(ep_rew, 6), "l": eplen, "t": round(time.time() - self.t_start, 6)}
            ep_info[ "Collisions"] = self.EpisodeCollisions
            for key in self.info_keywords:
                ep_info[key] = info[key]
            self.episode_rewards.append(ep_rew)
            self.episode_lengths.append(eplen)
            self.episode_times.append(time.time() - self.t_start)
            ep_info.update(self.current_reset_info)
            if self.logger:
                self.logger.writerow(ep_info)
                self.file_handler.flush()
            info['episode'] = ep_info

            self.TotalCollisions.append(self.EpisodeCollisions)
            self.EpisodeCollisions=0

        self.total_steps += 1
        return observation, reward, done, info

if __name__ == '__main__':

    UseVSRL = True#False

    log_dir = "tmp_log/"
    os.makedirs(log_dir, exist_ok=True)
    if UseVSRL:
        log_dir+='VSRL_'
        tb_name='VSRL'
    else:
        log_dir+='StandardRL_'
        tb_name='StandardRL'

    file_name = 'DroneDelivery'
    env_name = "../EnvBuild/" + file_name

    env = UnityEnv(env_name, worker_id=111, use_visual=False)
    tmp_env = DroneEnv(env_name, worker_id=0)
    SafePolicy.constraint_func = tmp_env.constraint_func
    SafePolicy.constrained_sample = tmp_env.constrained_sample
    tmp_env.close()
    env = SafeMonitor(env, log_dir)
    env = DummyVecEnv([lambda: env])

    PlotYN = False
    model = PPO2(SafePolicy, env, verbose=0, tensorboard_log="./ppo_sb_tensorboard/", learning_rate = 5.0e-4)
    # PlotYN = True
    # model = PPO2(SafePolicy, env, verbose=0, learning_rate = 5.0e-4)
    # model.learn(total_timesteps=1000000, tb_log_name="VSRL", callback=TensorboardCallback())

    model.learn(total_timesteps=1000000, tb_log_name=tb_name)
    model.save("unity_"+tb_name+"_model.pkl")

    env.close()
