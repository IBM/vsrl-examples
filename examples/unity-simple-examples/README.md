# Interface with Unity3D 
This folder contains a series of basic examples to connect 
the [Stable Baselines](https://github.com/hill-a/stable-baselines/tree/v2.10.0) framework for reinforcement learning 
with Unity environments via [gym-unity](https://github.com/Unity-Technologies/ml-agents/tree/0.15.1/gym-unity).

The examples will use the environments provided in the [EnvBuild](../EnvBuild/) folder.

## Loading a Unity3D Environment as a Gym environment
The gym-unity wrapper allows users to interact with Unity3D environments via standard OpenAI Gym functions.
To load a Unity environment as a Gym environment, the basic steps to follow are:

```python
...
# Loading Unity Environment 
env = UnityEnv(env_name,...)
action = env.action_space.sample()
# Interact with the Unity environment as a `gym` environment
observation, reward, done, info = env.step(action)
...
```
See more [here](https://github.com/Unity-Technologies/ml-agents/tree/0.15.1/gym-unity).

## Train a RL agent in a Unity3D Environment
`train_agent.py` shows how to train your first RL agent using Stable Baselines.

To train an agent in a safe environment, use the [`../EnvBuild/SafeDroneDelivery`](../EnvBuild/SafeDroneDelivery.md) 
environment. As certain safety constraints are embedded into the environment, the agent can only perform verified 
safe actions in SafeDroneDelivery.

An example using this environment is shown below:
```python
from gym_unity.envs import UnityEnv
from stable_baselines.common.policies import MlpPolicy
from stable_baselines import PPO2

file_name = 'SafeDroneDelivery'
env_name = "../EnvBuild/" + file_name


env = UnityEnv(env_name,
               worker_id=1000,
               use_visual=False,
               uint8_visual=False,
               allow_multiple_visual_obs=False,
               no_graphics=False)

# Create the agent
model = PPO2(MlpPolicy, env, verbose=0, learning_rate=1.0e-4)
model.learn(total_timesteps=500)

env.close()
```


## Train a safe agent in a Unity3D Environment
`train_safe_agent.py` shows the training of a simple agent using `SafePolicy`, which checks if each chosen 
action is safe. If it is not, a safe action is found by randomly sampling the space around. 
With this policy, the agent can train in any environment and ensure safety.

To train using `PPO2` from Stable Baselines with `SafePolicy` in a Unity environment:
```python
from gym_unity.envs import UnityEnv
from stable_baselines import PPO2
from train_safe_agent import SafePolicy 

file_name = 'DroneDelivery'
env_name = "../EnvBuild/" + file_name
env = UnityEnv(env_name, worker_id=10, use_visual=False, no_graphics=False)

model = PPO2(SafePolicy, env, verbose=1)
model.learn(total_timesteps=1000)

obs = env.reset()
for i in range(1000):
    action, _states = model.predict(obs)
    obs, rewards, done, info = env.step(action)
    env.render()

env.close()
```