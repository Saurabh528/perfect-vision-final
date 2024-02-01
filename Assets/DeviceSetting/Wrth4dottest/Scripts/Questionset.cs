using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;





[CreateAssetMenu(fileName = "Questions", menuName = "Worth4dotset")]

public class Questionset : ScriptableObject
{

    public Text questionText;
    public Button[] answerButtons;
  
    [Serializable]
    public class _question
    {
        public string question;
        public string[] options;
        public string[] Diagonis;
    }

    public List<_question> question = new List<_question>();

  

}
