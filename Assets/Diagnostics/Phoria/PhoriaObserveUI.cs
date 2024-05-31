using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoriaObserveUI : MonoBehaviour
{
    public Text _textHorVal, _textVerVal, _textCombineVal, _textDegreeVal, _textHorTag, _textVerTag, _textCombineTag;
    
    public bool ShowValues(float deltaXPix, float deltaYPix, EYESIDE eyeside ){//return true when combine value is less than threshold.
        float xval = 0;
        if(eyeside == EYESIDE.LEFT)
            xval = deltaXPix;
        else
            xval = -deltaXPix;
        float hor = PixToPhoriaValue(xval);
        _textHorVal.text = Mathf.Abs(hor).ToString("F2");
        _textHorTag.text = hor > 0? "exo": (hor < 0?"eso": "");
        float ver = PixToPhoriaValue(deltaYPix);
        _textVerVal.text = Mathf.Abs(ver).ToString("F2");
        _textVerTag.text = ver > 0? "hypo": (ver < 0?"hyper": "");
        float combine = Mathf.Sqrt(hor * hor + ver * ver);
        _textCombineVal.text = combine.ToString("F2");
        _textCombineTag.text = $"{_textHorTag.text} {_textVerTag.text}";
        float degree = Mathf.Atan2(deltaYPix, deltaXPix) * 180 / Mathf.PI;
        if(degree < 0)
            degree += 360;
        _textDegreeVal.text = degree.ToString("F2");
        return combine < 0.05f;
    }

    float PixToPhoriaValue(float pix){
        return (pix + 0.116f) / 17.407f;
    }
}
