import os
from gym_unity.envs import UnityEnv
from stable_baselines import PPO2
from stable_baselines.common.vec_env import DummyVecEnv
from safe_policy import SafeMonitor, SafePolicy

log_dir = "tmp_log/"
os.makedirs(log_dir, exist_ok=True)

file_name = 'DroneDelivery'
env_name = "../EnvBuild/" + file_name
unity_env = UnityEnv(env_name, worker_id=10, use_visual=False, no_graphics=False)

# Wrap environment to keep track of collisions
safe_monitor = SafeMonitor(unity_env, log_dir)
environment = DummyVecEnv([lambda: safe_monitor])

model = PPO2(SafePolicy, environment, verbose=0, tensorboard_log="./ppo_sb_tensorboard/",
             learning_rate=5.0e-4)
model.learn(total_timesteps=1000000, tb_log_name='SafeRL')
model.save("unity_SafeRL_model.pkl")
environment.close()
