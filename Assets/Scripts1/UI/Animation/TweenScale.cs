using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TweenScale : MonoBehaviour
{
    [SerializeField] Vector3 from = Vector3.one, to = Vector3.one;
    [SerializeField] float duration = 1;
    bool playing = false;
    float time;
    // Start is called before the first frame update
    void Start()
    {
        StartAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        if(playing){
            time += Time.deltaTime;
            if(time > duration)
                playing = false;
            else{
                transform.localScale = Vector3.Lerp(from, to, time / duration);
            }
        }
    }

    void StartAnimation(){
        playing = true;
        time = 0;
    }

}
