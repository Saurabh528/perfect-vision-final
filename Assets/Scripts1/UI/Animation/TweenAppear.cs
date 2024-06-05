using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenAppear : MonoBehaviour
{
    enum TweenState{
        Hidden,
        Shown,
        Appearing,
        Disappearing
    };

    TweenState _tweenState = TweenState.Hidden;
    [SerializeField] float _duration = 1;
    [SerializeField] bool _fade;//alpha changes from 0 to 1.
    [SerializeField] bool _scale;
    [SerializeField] Vector3 _scaleFrom = Vector3.one, _scaleTo = Vector3.one;
    float _lifeTime;
    Dictionary<MaskableGraphic, Color> _originColors = new Dictionary<MaskableGraphic, Color>();

    void Awake(){
        if(_fade){
            MaskableGraphic[] graphics = GetComponentsInChildren<MaskableGraphic>();
            foreach(MaskableGraphic graph in graphics){
                _originColors[graph] = graph.color;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_lifeTime != 0){
            _lifeTime -= Time.deltaTime;
            if(_lifeTime < 0){
                _lifeTime = 0;
                if(_tweenState == TweenState.Appearing)
                    SetStateTo();
                else if(_tweenState == TweenState.Disappearing){
                    SetStateFrom();
                    gameObject.SetActive(false);
                }
            }
            else
                Tween();
        }
    }

    public void Tween(){
        if( _tweenState == TweenState.Appearing){
            if(_scale)
                transform.localScale = Vector3.Lerp(_scaleTo, _scaleFrom, _lifeTime / _duration);
            if(_fade){
                foreach(KeyValuePair<MaskableGraphic, Color> pair in _originColors)
                    pair.Key.color = new Color(pair.Value.r, pair.Value.g, pair.Value.b, pair.Value.a * (_duration - _lifeTime) / _duration);
            }
        }
        else if( _tweenState == TweenState.Disappearing){
            if(_scale)
                transform.localScale = Vector3.Lerp(_scaleFrom, _scaleTo, _lifeTime / _duration);
            if(_fade){
                foreach(KeyValuePair<MaskableGraphic, Color> pair in _originColors)
                    pair.Key.color = new Color(pair.Value.r, pair.Value.g, pair.Value.b, pair.Value.a * _lifeTime / _duration);
            }
        }
    }

    public void Appear(){
        if(_tweenState == TweenState.Shown || _tweenState == TweenState.Appearing)
            return;
        gameObject.SetActive(true);
        _lifeTime = _duration;
        SetStateFrom();
        _tweenState = TweenState.Appearing;
    }

    public void Disappear(){
        if(_tweenState == TweenState.Hidden || _tweenState == TweenState.Disappearing)
            return;
        _lifeTime = _duration;
        SetStateTo();
        _tweenState = TweenState.Disappearing;
    }

    void SetStateFrom(){
        if(_scale)
            transform.localScale = _scaleFrom;
        if(_fade){
            foreach(KeyValuePair<MaskableGraphic, Color> pair in _originColors)
                pair.Key.color = new Color(pair.Value.r, pair.Value.g, pair.Value.b, 0);
        }
        _tweenState = TweenState.Hidden;
    }

    void SetStateTo(){
        if(_scale)
            transform.localScale = _scaleTo;
        if(_fade){
            foreach(KeyValuePair<MaskableGraphic, Color> pair in _originColors)
                pair.Key.color = new Color(pair.Value.r, pair.Value.g, pair.Value.b, pair.Value.a);
        }
        _tweenState = TweenState.Shown;
    }
}
