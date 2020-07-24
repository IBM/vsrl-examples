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
In `safe_policy` are classes `SafePolicy` and `SafeMonitor`. `SafePolicy` checks if each chosen action is safe. If not, 
a safe action is found by randomly sampling the space around. With this policy, the agent can train in any environment 
and ensure safety. `SafeMonitor` is a monitor wrapper that tracks collisions that occur while training.  

Below is an example training a safe agent using `PPO2` from Stable Baselines with these classes:
```python
from gym_unity.envs import UnityEnv
from stable_baselines import PPO2
from safe_policy import SafePolicy 

log_dir = "tmp_log/"
os.makedirs(log_dir, exist_ok=True)

file_name = 'DroneDelivery'
env_name = "../EnvBuild/" + file_name
unity_env = UnityEnv(env_name, worker_id=10, use_visual=False)

# Wrap environment to keep track of collisions
safe_monitor = SafeMonitor(unity_env, log_dir)
environment = DummyVecEnv([lambda: safe_monitor])

model = PPO2(SafePolicy, environment, verbose=0, tensorboard_log="./ppo_sb_tensorboard/",
             learning_rate=5.0e-4)
model.learn(total_timesteps=1000000, tb_log_name='SafeRL')
model.save("unity_SafeRL_model.pkl")
environment.close()
```
