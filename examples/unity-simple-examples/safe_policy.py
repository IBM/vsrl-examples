import csv
import random
import time
import gym
import numpy as np
import matplotlib.pyplot as plt
import tensorflow as tf
from stable_baselines.common.policies import ActorCriticPolicy, mlp_extractor
from stable_baselines.common.tf_layers import linear
from stable_baselines.bench import Monitor


delta_time = 0.1
num_animated = 3
animated_start_index = 4
max_tries = 100
tree_start_index = animated_start_index + num_animated * 3
num_trees = 3
SAFE_SEP = 0.3
SAFE_SEP_DOG = 0.4
INDEX_DRONE_DISTANCE_TARGET = 0
INDEX_DRONE_POS_X = 1
INDEX_DRONE_POS_Y = 2
VEL_X = 0
VEL_Y = 0


class SafePolicy(ActorCriticPolicy):
    """
    Safe Policy object that implements actor critic with a choice of Safe Actions, using a feed forward neural network.

    Parameters:
        tf_session (TensorFlow session): The current TensorFlow session
        ob_space (Gym Space): The observation space of the environment
        ac_space (Gym Space): The action space of the environment
        num_env (int): The number of environments to run
        num_steps (int): The number of steps to run for each environment
        num_batch (int): The number of batch to run (n_envs * n_steps)
        activation_func(tf.func):  the activation function to use in the neural network.
        reuse (bool): If the policy is reusable or not
        kwargs (dict): Extra keyword arguments for the nature CNN feature extraction
    """

    def __init__(self,
                 tf_session,
                 ob_space,
                 ac_space,
                 num_env,
                 num_steps,
                 num_batch,
                 activation_func=tf.nn.tanh,
                 reuse=False, **kwargs):
        super(SafePolicy, self).__init__(tf_session, ob_space, ac_space, num_env, num_steps, num_batch, reuse=reuse)
        layers = [256, 256, 256]
        net_arch = [dict(vf=layers, pi=layers)]

        with tf.variable_scope("model", reuse=reuse):
            pi_latent, vf_latent = mlp_extractor(tf.layers.flatten(self.processed_obs), net_arch, activation_func)

            self._value_fn = linear(vf_latent, 'vf', 1)

            self._proba_distribution, self._policy, self.q_value = \
                self.pdtype.proba_distribution_from_latent(pi_latent, vf_latent, init_scale=0.01)

        self._setup_init()

    def __plot_positions(self, obs):
        """
        Plot positions of dogs/kid and trees
        Parameters:
            obs ([float]): Observation vector
        """
        drone_info = obs[0]

        plt.clf()
        plt.plot(drone_info[INDEX_DRONE_POS_X], drone_info[INDEX_DRONE_POS_Y], 'x')
        for i in range(animated_start_index, animated_start_index + 3 * num_animated, 3):
            ox = drone_info[i + 1]
            oy = drone_info[i + 2]
            plt.plot(ox, oy, 's', color='k')
        for i in range(tree_start_index, tree_start_index + 3 * num_trees, 3):
            ox = drone_info[i + 1]
            oy = drone_info[i + 2]
            plt.plot(ox, oy, 'o', color='r')

    def step(self, obs, state=None, mask=None, deterministic=False):
        """
        Perform RL step
        Parameters:
            obs ([float]): Observation vector
            state ([float]): The initial state of the policy. For feedforward policies, None.
            mask ([float]): Mask
            deterministic (bool): Whether or not to return deterministic actions.

        Returns:
            Any: action
            Any: value
            Any: initial_state
            Any: neglogp
        """
        action, value, neglogp = self.sess.run([self.action, self.value_flat, self.neglogp],
                                               {self.obs_ph: obs})

        self.__plot_positions(obs)

        drone_info = obs[0]

        # if the action is safe, use it. otherwise, check another possible action
        if not is_action_safe(drone_info, action[VEL_X])[0]:
            safe_actions = []
            for i in range(max_tries):
                alternative_action = [10 * (random.random() - 0.5), 10 * (random.random() - 0.5)]
                if is_action_safe(drone_info, alternative_action)[0]:
                    safe_actions.append(alternative_action)
                    break

            if safe_actions:
                action = random.choices(safe_actions)
            else:
                print("No Safe Action found")
                action = np.array([[0, 0]])

        return action, value, self.initial_state, neglogp

    def proba_step(self, obs, state=None, mask=None):
        """
        Implements: Returns the action probability for a single step
        Parameters:
            obs ([float]): The current observation of the environment
            state ([float]): The last states, None for feedforward policies
            mask([float]): The last masks, None for feedforward policies
        Returns:
            [float]: Action probability
        """
        return self.sess.run(self.policy_proba, {self.obs_ph: obs})

    def value(self, obs, state=None, mask=None):
        """
        Implements: Returns the value for a single step
        Parameters:
            obs ([float]): The current observation of the environment
            state ([float]): The last states, None for feedforward policies
            mask([float]): The last masks, None for feedforward policies
        Returns:
            [float]: Associated value of the action
        """
        return self.sess.run(self.value_flat, {self.obs_ph: obs})


def dist_from_obstacles(obs):
    """
    Calculate the distance to the obstacles

    Parameters:
        obs ([float]): Observation vector

    Returns:
        [float]:  Distance to the obstacles

    """

    d = []
    for i in range(animated_start_index, animated_start_index + 3 * num_animated, 3):
        d.append(obs[i + 3])

    for i in range(tree_start_index, tree_start_index + 3 * num_trees, 3):
        d.append(obs[i + 3])
    return d


def is_action_safe(obs, new_action):
    """
    Check if one action is safe

    Parameters:
        obs ([float]): Observation vector
        new_action (float, float): Action vector (speed)

    Returns:
        bool:  Is action safe
        float: distance to the nearest obstacle

    """

    x, y = obs[INDEX_DRONE_POS_X], obs[INDEX_DRONE_POS_Y]
    new_x, new_y = x + new_action[0] * delta_time, y + new_action[1] * delta_time
    distance = 1000000
    is_safe = True
    for i in range(animated_start_index, animated_start_index + 3 * num_animated, 3):
        obstacle_x = obs[i + 1]
        obstacle_y = obs[i + 2]
        distance = np.min([distance, np.sqrt((new_x - obstacle_x) ** 2 + (new_y - obstacle_y) ** 2)])
        if distance < SAFE_SEP_DOG:
            is_safe = False
    for i in range(tree_start_index, tree_start_index + 3 * num_trees, 3):
        obstacle_x = obs[i + 1]
        obstacle_y = obs[i + 2]
        distance = np.min([distance, np.sqrt((new_x - obstacle_x) ** 2 + (new_y - obstacle_y) ** 2)])
        if distance < SAFE_SEP:
            is_safe = False
    return is_safe, distance


class SafeMonitor(Monitor):
    """
    A monitor wrapper for Gym environments to save collisions events

    Parameters:
        env (gym.Env): The environment
        filename (Optional[str]): the location to save a log file, can be None for no log
    """

    def __init__(self,
                 env: gym.Env,
                 filename: str):
        super(SafeMonitor, self).__init__(env=env, filename=filename)
        self.EpisodeCollisions = 0
        self.TotalCollisions = []
        self.logger = csv.DictWriter(self.file_handler,
                                     fieldnames=('r', 'l', 't', 'Collisions'))
        self.logger.writeheader()

    def step(self, action):
        """
        Get information for the next RL step

        Parameters:
            action (float, float): Action vector (speed)

        Returns:
            [float]: Observation vector
            float: reward value
            observation, reward, done, info

        """
        if self.needs_reset:
            raise RuntimeError("Tried to step environment that needs reset")
        observation, reward, done, info = self.env.step(action)
        self.rewards.append(reward)
        obs = dist_from_obstacles(observation)

        if np.min(obs) < .3:
            self.EpisodeCollisions += 1
        if done:
            self.needs_reset = True
            ep_rew = sum(self.rewards)
            eplen = len(self.rewards)
            ep_info = {"r": round(ep_rew, 6), "l": eplen, "t": round(time.time() - self.t_start, 6),
                       "Collisions": self.EpisodeCollisions}

            self.episode_rewards.append(ep_rew)
            self.episode_lengths.append(eplen)
            self.episode_times.append(time.time() - self.t_start)
            ep_info.update(self.current_reset_info)
            if self.logger:
                self.logger.writerow(ep_info)
                self.file_handler.flush()
            info['episode'] = ep_info

            self.TotalCollisions.append(self.EpisodeCollisions)
            self.EpisodeCollisions = 0

        self.total_steps += 1
        return observation, reward, done, info