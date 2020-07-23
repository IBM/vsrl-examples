# VSRL first steps

The key difference between VSRL and normal RL is that for VSRL the environment must have a function (which we name `constraint_func`) which takes in an action and the current state (as a high-level representation of only the safety-relevant features; these are the symbolic features) and returns whether that action is safe in that state.

For simplicity, we'll use the the high-level state of the environment directly as the observations here (as opposed to the observations being RGB images). Thus we can just use a small wrapper class which passes in the observations as the symbolic features (otherwise we need to train a model to extract the symbolic features from the images).

Here we provide a couple of simple examples of using VSRL.

## Usage

### Wrap an environment to enforce safety

```python
from vsrl.rl.envs import ACC
from vsrl.rl.envs.safety_wrappers import wrap_environment, wrap_symbolic_observation_env

# `wrap_environment` uses the environment's constraint_func to only allow safe actions;
# if the desired action isn't safe, a new one is found
# `wrap_symbolic_observation_env` passes the observations as the symbolic features

Env = wrap_symbolic_observation_env(wrap_environment(ACC))
env = Env(oracle_obs=True)

# env will always be safe now, no matter which actions you take

obs = env.reset()
while True:
    obs, reward, done, info = env.step(env.action_space.sample())
    assert not info["unsafe"]
    if done:
        break
```

### Load a trained agent

```python
from stable_baselines import PPO2
from vsrl.rl.envs import ACC
from vsrl.rl.envs.safety_wrappers import wrap_environment, wrap_symbolic_observation_env

Env = wrap_symbolic_observation_env(wrap_environment(ACC))
env = Env(oracle_obs=True)
model = PPO2.load("ppo_safe.pkl")

# test the trained agent
total_reward = 0
obs = env.reset()
while True:
    action, _ = model.predict(obs)
    obs, reward, done, info = env.step(action)
    total_reward += reward
    if done:
        break
print(total_reward)
```

### Training an RL agent

Because all of the modifications to ensure safety are happening inside of the environment, we can train an RL agent as if this were any other (OpenAI gym-compatible) environment.

```python
from stable_baselines import PPO2
from vsrl.rl.envs import ACC
from vsrl.rl.envs.safety_wrappers import wrap_environment, wrap_symbolic_observation_env

Env = wrap_symbolic_observation_env(wrap_environment(ACC))
env = Env(oracle_obs=True)

model = PPO2("MlpPolicy", env)
model.learn(total_timesteps=250_000)
model.save("ppo2.pkl")
```
