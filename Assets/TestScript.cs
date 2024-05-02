using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class TestScript : MonoBehaviour
{
   void Start(){
        Dictionary<string, string> dic = new Dictionary<string, string>();
        string jsonStr = JsonConvert.SerializeObject(dic);
        dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
        int i = 0;
   }
   
}


