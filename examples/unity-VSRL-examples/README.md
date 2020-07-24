# VSRL and Unity

These examples show how to train a VSRL agent in a Unity3D environment.

## With a constrained agent

Though the VSRL method in the paper is framed as a modification to the environment, the same effect can be achieved by modifying the RL agent instead. Depending on how the agent and environment interact and how the symbolic features are extracted from the environment, one of these methods may be simpler than the other. `safe_agent.py` gives an example of using the safety constraint inside of the agent.

Note - for the theoretical results from the VSRL paper to hold, the agent should not see itself as taking a different action if the one it initially desired was unsafe. Safety is preserved either way, but the reward-learning may be affected if the agent sees itself as taking a safe replacement action instead. We are not aware of any results which indicate that this method of training is necessarily worse, however.

## With the VSRL framework

`safe_environment.py` defines a wrapper class for the drone environment which adds a new function, `constraint_func`. This takes an action and the current (symbolic) state of the environment as input and returns whether the action is safe. Once this function is added, we can use the wrappers from the `vsrl` library to automatically enforce safety in the environment. Because this is handled internally in the environment classes, we can train an RL agent on this environment just as we would with any other (OpenAI gym-compatible) environment.

```python
import json
from stable_baselines import PPO2
from vsrl.rl.envs.safety_wrappers import wrap_environment, wrap_symbolic_observation_env
from safe_environment import DroneEnv

env_name = "DroneDelivery"
# note: the safety constraints depend on knowing the dynamics type (circular or linear)
# for each object. That could be passed in directly or, as here, loaded from the
# environment config file.
with open("env_config.json") as f:
    dynamics_types = json.load(f)["movement"]

SafeDroneEnv = wrap_symbolic_observation_env(wrap_environment(DroneEnv))
env = SafeDroneEnv(
    env_name,
    dynamics_types=dynamics_types,
    worker_id=0,
    use_visual=False,
    no_graphics=True,
)

model = PPO2("MlpPolicy", env, verbose=1)
model.learn(total_timesteps=1000)
```
