using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextRewardController : MonoBehaviour
{
    public Text reward;
    public Text penalty;

    public void UpdateText(string _reward, string _penalty) {
        reward.text = "REWARD: " + _reward;
        penalty.text = "PENALTY: " + _penalty;
    }
}
