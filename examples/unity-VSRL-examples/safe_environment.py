#
# Copyright (C) 2020 IBM. All Rights Reserved.
#
# See LICENSE.txt file in the root directory
# of this source tree for licensing information.
#

import json
import logging

from gym_unity.envs import UnityEnv
from stable_baselines import PPO2
from vsrl.rl.envs.safety_wrappers import wrap_environment, wrap_symbolic_observation_env


class DroneEnv(UnityEnv):
    """
    To be compatible with the VSRL framework, an environment must have a method called
    `constraint_func` which returns whether an action is safe or not in the state
    specified by given symbolic (i.e. high-level; not images) features.

    The constraint should be based on the dynamics of the agent / hazards in the
    environment and should ideally be proven to keep the agent safe (using a software
    like KeYmaeraX).
    """

    DOG_OFFSET = 5
    NUM_DOGS = 3
    TREE_OFFSET = DOG_OFFSET + NUM_DOGS * 3
    MAX_LINEAR_STEP = 0.01  # The maximum amount of space that a linear obstacle will move in a given time step.
    MAX_CIRCULAR_STEP = 0.01  # the max amount of space that the circular obstacle will move in a time step.

    def __init__(
        self,
        *args,
        dynamics_types,
        T: float = 0.01,
        safe_sep: float = 0.0001,
        debug: bool = False,
        **kwargs,
    ):
        """
        :param safe_sep: the safe separation distance between ego and obstacles' centerpoints.
        """
        super().__init__(*args, **kwargs)
        self.dynamics_types = dynamics_types
        self.T = T
        self.safe_sep = safe_sep
        self.debug = debug

    def constraint_func(self, action, sym_feats):
        return self._safe_for_trees(sym_feats, action) and self._safe_for_obstacles(
            sym_feats, action
        )

    def _next_ego_location(self, x, y, action):
        """ The ego's next location (nb: this is not a great model -- can we get the exact dynamics?) """
        return x + action[0] * self.T, y + action[1] * self.T

    def _safe_for_trees(self, obs, action):
        """ checks if an action is safe with regards to trees """
        ego_x, ego_y = self._next_ego_location(obs[1], obs[2], action)
        for tree_x, tree_y in obs[self.TREE_OFFSET :].reshape(-1, 3)[:, :2]:
            if self._is_too_close([ego_x, ego_y], [tree_x, tree_y]):
                if self.debug:
                    logging.warn(
                        f"A tree is not safe: {tree_x, tree_y}, {obs[1], obs[2]}"
                    )
                return False
        return True

    def _safe_for_obstacles(self, obs, action):
        """
        Checks that the action is safe for all obstacles.

        :param obs the observation vector.
        :param action the action.
        """
        dog_xys = obs[self.DOG_OFFSET : self.TREE_OFFSET].reshape(-1, 3)[:, :2]
        for dynamics_type, (dog_x, dog_y) in zip(self.dynamics_types, dog_xys):
            if dynamics_type == "linear":
                if not self._safe_for_linear(obs[1], obs[2], dog_x, dog_y, action):
                    if self.debug:
                        logging.warn(
                            f"An obstacle is not safe: {dog_x, dog_y}, {obs[1], obs[2]}"
                        )
                    return False
            else:
                if not self._safe_for_circular(obs[1], obs[2], dog_x, dog_y, action):
                    if self.debug:
                        logging.warn(
                            f"An obstacle is not safe: {dog_x, dog_y}, {obs[1], obs[2]}"
                        )
                    return False
        return True

    def _safe_for_linear(self, egox, egoy, x, y, action):
        new_x, new_y = self._next_ego_location(egox, egoy, action)
        return (
            abs(new_x - x) > self.safe_sep
            or abs(new_y, y) > self.safe_sep + self.MAX_LINEAR_STEP
        )

    def _safe_for_circular(self, egox, egoy, x, y, action):
        new_x, new_y = self._next_ego_location(egox, egoy, action)
        return (
            abs(new_x - x) > self.safe_sep + self.MAX_CIRCULAR_STEP
            or abs(new_y - y) > self.safe_sep + self.MAX_CIRCULAR_STEP
        )

    def _is_too_close(self, xy1, xy2):
        """ Checks whether two <x,y> coordinates are colliding with one another. """
        x1, y1 = xy1
        x2, y2 = xy2
        return (x1 - x2) ** 2 + (y1 - y2) ** 2 < self.safe_sep ** 2


env_name = "../EnvBuild/DroneDelivery"
with open("../EnvBuild/env_config.json") as f:
    dynamics_types = json.load(f)["movement"]

SafeDroneEnv = wrap_symbolic_observation_env(wrap_environment(DroneEnv))
# LINUX: Disable the Unity window -> no_graphics=True
env = SafeDroneEnv(
    env_name,
    dynamics_types=dynamics_types,
    worker_id=0,
    use_visual=False,
    no_graphics=False,
)

model = PPO2("MlpPolicy", env, verbose=1)
model.learn(total_timesteps=1000)
env.close()
