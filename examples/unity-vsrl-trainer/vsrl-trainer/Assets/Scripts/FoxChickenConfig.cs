using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class FoxChickenConfig {
    public int numberOfDogs = 3;

    public List<string> type;
    public List<string> movement;
    public List<Vector3> animatedPositions = new List<Vector3>();
    public List<Vector3> treePositions = new List<Vector3>();
    public string reward = "high";
    public string penalty = "low";

    public int dogCollision; //environmental parameter
    public int deliverPackage; //environmental parameter
    public int treeCollision;  //environmental parameter
    public int trainingTrials = 5000000;
    public bool endAtCollision = false;

    public string GetConfigPath()
    {
        Debug.Log("Reading config file");
        string argName = "-envConfig";
        string[] arguments = Environment.GetCommandLineArgs();
        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] == argName && arguments.Length > i + 1)
            {
                return arguments[i + 1];
            }
        }
        // default
        string configPath;
        if (Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.LinuxPlayer ||
                    Application.platform == RuntimePlatform.WindowsPlayer) {
            configPath = Path.Combine("./", "env_config.json");
        } else if (Application.platform == RuntimePlatform.OSXEditor ||
                    Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.LinuxEditor) {
            configPath = Path.Combine("../EnvBuild", "env_config.json");
        } else {
            configPath = Path.Combine("./EnvBuild", "env_config.json");
        }
        return configPath;
    }

    public List<int> GetDogPrefabs()
    {
        List<int> prefabs = new List<int>();
        for (int i = 0; i < numberOfDogs; i++)
        {
            if (i < type.Count)
            {
                if (type[i].Equals("goose"))
                {
                    prefabs.Add(2);
                }else if (type[i].Equals("child"))
                {
                    prefabs.Add(1);
                }else if (type[i].Equals("dog"))
                {
                    prefabs.Add(0);
                }
                else
                {
                    prefabs.Add(0);
                }
            }
            else
            {
                prefabs.Add(0);
            }
        }

        return prefabs;
    }

    public List<int> GetDogMovements()
    {
        List<int> prefabs = new List<int>();
        for (int i=0; i<numberOfDogs; i++) {
            if (i < movement.Count) {
                var move = movement[i];
                if (move.Equals("linear")) {
                    prefabs.Add(1);
                } else if (move.Equals("circular")) {
                    prefabs.Add(0);
                } else if (move.Equals("chase")) {
                        prefabs.Add(2);
                } else {
                    prefabs.Add(0);
                }
            } else {
                prefabs.Add(0);
            }
        }
        return prefabs;
    }

    public float GetRewardValue()
    {
        if (reward.Equals("low"))
        {
            return AIModels.LOW_REWARD;
        }
        else if (reward.Equals("medium"))
        {
            return AIModels.MID_REWARD;
        } else
        {
            return AIModels.HIGH_REWARD;
        }
    }

    public float GetPenaltyValue()
    {
        if (penalty.Equals("low"))
        {
            return AIModels.LOW_PENALTY;
        }
        else if (penalty.Equals("medium"))
        {
            return AIModels.MID_PENALTY;
        } else
        {
            return AIModels.HIGH_PENALTY;
        }
    }

    public float GetStaticPenaltyValue()
    {
        return AIModels.STATIC_COLLISION_PENALTY;
    }

}
