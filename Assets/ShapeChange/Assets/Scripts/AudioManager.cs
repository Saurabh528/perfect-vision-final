using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    //------------------------CREDITS----------------------------
    //Background music by Eric Matyas: http://www.soundimage.org
    //Sound effects: https://www.noiseforfun.com
    //-----------------------------------------------------------

    public AudioSource backgroundMusic, tokenSound, scoreSound, deathSound, colorChangeSound, skinSwitchSound, notEnoughTokenSound;

    [HideInInspector]
    public bool soundIsOn = true;       //GameManager script might modify this value

    //Functions are called when it is necessary

    public void StopBackgroundMusic()
    {
        backgroundMusic.Stop();
    }

    public void PlayBackgroundMusic()
    {
        if (soundIsOn)
            backgroundMusic.Play();
    }

    public void TokenSound()
    {
        if(soundIsOn)
            tokenSound.Play();
    }

    public void ScoreSound()
    {
        if (soundIsOn)
            scoreSound.Play();
    }

    public void DeathSound()
    {
        if (soundIsOn)
            deathSound.Play();
    }

    public void ColorChangeSound()
    {
        if (soundIsOn)
            colorChangeSound.Play();
    }

    public void NotEnoughTokenSound()
    {
        if (soundIsOn)
            notEnoughTokenSound.Play();
    }

    public void SkinSwitchSound()
    {
        if (soundIsOn)
            skinSwitchSound.Play();
    }
}
