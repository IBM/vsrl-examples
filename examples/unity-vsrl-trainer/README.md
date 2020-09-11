# Verifiably Safe Reinforcement Learning Examples

This folder contains an environment to test and challenge reinforcement learning algorithms in safe environments. Verifiably Safe Reinforcement Learning [Verifiably Safe Reinforcement Learning] framework is used as reference but the environments can be tested with any RL algorithm.

This is the trainer that we are using in the unity samples.


## Requirements

- [Python 3.6 or 3.7](https://www.python.org/downloads/)
- (optional, only to create new environments) [Unity Hub](https://store.unity.com/download)

## Setup

To facilitate the installation process a script was provided to install/update dependencies. Open the this folder in a Terminal window and run:

```sh
./setup.sh
```

This process will create an environment with all the necessary dependencies. To activate the environment run:

```
source venv/bin/activate
```


## Setting up Unity
You will need to install ML Agents and Universal RP.

1. Add the vsrl-trainer project to Unity Hub.
2. Select the Unity version 2019.3.13f1 or any 2019.3.xxx version
3. Select your platform.
4. Open the project.
5. Navigate to Window > Package Manager.
6. Installing ML-Agents: Press the `+` button on the top left corner of the screen and select `Add package from disk`.
Select the file `package.json` in the folder `ml-agents/com.unity.ml-agents`.


## Train your own model

You need to have setted your unity project following the previous steps.

For start the training we recomend to you read the following [link](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Training-ML-Agents.md)

We included a [config file](../EnvBuild/UnityTrainingConfig.yaml) if you want to start to train using mlagents-learn.
