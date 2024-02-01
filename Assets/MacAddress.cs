using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacAddress : MonoBehaviour
{
    public GameObject Lock;

    // Start is called before the first frame update
    void Start()
    {
        string currentDeviceId = SystemInfo.deviceUniqueIdentifier;

        Debug.Log("currentDeviceId" + currentDeviceId);

        if(currentDeviceId == "aeOdebf51b68cbe61fcf7d8e97c3309a3ceadc29")
        {
            Lock.SetActive(false);
        }

        else
        {
            Lock.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
