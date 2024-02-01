using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballon_GameMusic : MonoBehaviour
{
    public AudioClip GameAudio;
    public AudioSource GameAudioSource;
    // Use this for initialization
    void Start()
    {
        GameAudioSource.clip = GameAudio;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
