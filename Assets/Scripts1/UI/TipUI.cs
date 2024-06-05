using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipUI : MonoBehaviour
{
    [SerializeField] Text _text;
    float _lifetime;

    // Update is called once per frame
    void Update()
    {
        if(_lifetime > 0){
            _lifetime -= Time.deltaTime;
            if(_lifetime < 0){
                _lifetime = 0;
                gameObject.SetActive(false);
            }
        }
    }

    public void Show(string text, float duration = 0){
        _text.text = text;
        _lifetime = duration;
        gameObject.SetActive(true);
    }


}
