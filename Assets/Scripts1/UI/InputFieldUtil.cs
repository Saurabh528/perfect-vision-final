using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldUtil : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    public Button btn_OK, btn_Cancel;
    int InputSelected;
    // Start is called before the first frame update
    void OnEnable()
    {
        InputSelected = 0;
        if(inputFields.Length > 0){
            SelectInputField();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(inputFields.Length > 1){
            if(Input.GetKeyDown(KeyCode.Tab)){
                GetCurrentInputField();
                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift)){
                    InputSelected--;
                    InputSelected += inputFields.Length;
                }
                else
                    InputSelected++;
                InputSelected = InputSelected % inputFields.Length;
                SelectInputField();
            }
        }

        if((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && btn_OK && btn_OK.onClick != null){
            GetCurrentInputField();
            if(InputSelected == -1 || inputFields[InputSelected] == null || inputFields[InputSelected].lineType == TMP_InputField.LineType.SingleLine)
                btn_OK.onClick.Invoke();
        }
           
        if(Input.GetKeyDown(KeyCode.Escape) && btn_Cancel && btn_Cancel.onClick != null)
            btn_Cancel.onClick.Invoke();
    }

    void SelectInputField(){
        if(InputSelected < 0 || InputSelected >= inputFields.Length)
            return;
        inputFields[InputSelected].Select();
    }

    void GetCurrentInputField(){
        for(int i = 0; i < inputFields.Length; i++){
            if(inputFields[i].isFocused){
                InputSelected = i;
                return;
            }
        }
        InputSelected = -1;
    }
}
