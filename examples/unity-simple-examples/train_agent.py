#
# Copyright (C) 2020 IBM. All Rights Reserved.
#
# See LICENSE.txt file in the root directory
# of this source tree for licensing information.
#

from gym_unity.envs import UnityEnv
from stable_baselines import PPO2
from stable_baselines.common.policies import MlpPolicy

# Linux: The env_config.json and the Unity binary must be on the same folder than this Python script.
# MacOS: Copy the Unity binary to the EnvBuild folder.
file_name = 'DroneDelivery'
env_name = "../EnvBuild/" + file_name

num_episodes = 500


class StableBasGym:
    @staticmethod
    def run():
        # LINUX: Disable the Unity window -> no_graphics=True
        env = UnityEnv(env_name,
                       worker_id=1000,
                       use_visual=False,
                       uint8_visual=False,
                       allow_multiple_visual_obs=False,
                       no_graphics=False)

        # Create the agent
        model = PPO2(MlpPolicy, env, verbose=0, learning_rate=1.0e-4)
        model.learn(total_timesteps=num_episodes)

        env.close()

        print("Successfully trained")


if __name__ == '__main__':
    StableBasGym().run()
