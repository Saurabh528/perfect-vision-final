using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControlBarUI : MonoBehaviour
{
    TextMeshProUGUI textCompanyName;
    // Start is called before the first frame update
    void Start()
    {
        textCompanyName = GetComponent<TextMeshProUGUI>();
        if(textCompanyName)
            textCompanyName.text = GameConst.COMPANYNAME;
    }

}
