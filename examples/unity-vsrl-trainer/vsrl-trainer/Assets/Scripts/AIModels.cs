/*

Copyright (C) 2020 IBM. All Rights Reserved.

See LICENSE.txt file in the root directory
of this source tree for licensing information.

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barracuda;
using MLAgents;

public class AIModels : MonoBehaviour
{
    private static string PLAYER_VSRL = "VSRL-Agent";
    private static string PLAYER_RL = "RL-Agent";

    [HideInInspector] public const float HIGH_REWARD = 100f;
    [HideInInspector] public const float MID_REWARD = 10f;
    [HideInInspector] public const float LOW_REWARD = 1f;
    [HideInInspector] public const float HIGH_PENALTY = -1f;
    [HideInInspector] public const float MID_PENALTY = -0.5f;
    [HideInInspector] public const float LOW_PENALTY = -0.1f;
    [HideInInspector] public const float GOAL_DISTANCE_PENALTY = -0.01f;
    [HideInInspector] public const float STATIC_COLLISION_PENALTY = -0.1f;
    [HideInInspector] public const float FALL_PENALTY = -400f;

    public NNModel[] RL_Models;
    public NNModel[] VSRL_Models;

    private Dictionary <string, NNModel> RL_Models_dic = new Dictionary<string, NNModel>();
    private Dictionary <string, NNModel> VSRL_Models_dic = new Dictionary<string, NNModel>();

    public NNModel VSRL_Current_Model;
    public NNModel RL_Current_Model;



    private void Awake() {
        PopulateDicts();
    }

    private void PopulateDicts () {
        foreach (NNModel model in RL_Models) {
            RL_Models_dic.Add(model.name, model);
        }
        foreach (NNModel model in VSRL_Models) {
            VSRL_Models_dic.Add(model.name, model);
        }
    }

    private void SelectRLModel (string model_name) {
        if (RL_Models_dic.ContainsKey(model_name)) {
            RL_Current_Model = RL_Models_dic[model_name];
        } else {
            RL_Current_Model = RL_Models[0];
        }
    }

    private void SelectVSRLModel (string model_name) {
        if (VSRL_Models_dic.ContainsKey(model_name)) {
            VSRL_Current_Model = VSRL_Models_dic[model_name];
        } else {
            VSRL_Current_Model = VSRL_Models[0];
        }
    }

    public void SetModels (string model_name) {
        SelectRLModel(model_name);
        SelectVSRLModel(model_name);
    }

    public void SetupModelFor(Agent agent) {
        NNModel model;
        if (agent.gameObject.name == PLAYER_VSRL) {
            if (VSRL_Current_Model == null) VSRL_Current_Model = VSRL_Models[0];
            model = VSRL_Current_Model;
        } else {
            if (RL_Current_Model == null) RL_Current_Model = VSRL_Models[0];
            model = RL_Current_Model;
        }
        agent.SetModel(model.name, model);
    }
}
