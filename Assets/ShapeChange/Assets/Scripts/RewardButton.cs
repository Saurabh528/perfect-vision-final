using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardButton : MonoBehaviour
{
    public int rewardCount = 5;

    void Start()
    {
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "+" + rewardCount.ToString();      //For example, if the rewardCount equals 5, then the text is: +5
    }

    public void AddReward()
    {
        FindObjectOfType<ScoreManager>().IncrementToken(rewardCount);       //Increments token by the given value
    }

    public void ShowRewardVideo()
    {
        //UNCOMMENT THE FOLLOWING LINES IF YOU ENABLED UNITY ADS AT UNITY SERVICES AND REOPENED THE PROJECT!
        //if (FindObjectOfType<AdManager>().unityAds)
        //    FindObjectOfType<AdManager>().ShowUnityRewardVideoAd();       //Shows Unity Reward Video ad
        //else
        FindObjectOfType<AdManager>().ShowAdmobRewardVideo();       //Shows Admob Reward Video ad
    }
}
