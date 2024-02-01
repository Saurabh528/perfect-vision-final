using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class therapistOrPatient : MonoBehaviour {
    public static int index; 
	// Use this for initialization
	void Start () {
        Debug.Log("STARTING WORKS");
        
       // saving_login_pw.therapist_offline_list.Clear();
        //  using (StreamReader sr = new StreamReader("E:/Intern/ArmAbleNew/Assets" + "/" + "Credentials_Data_Therapists.csv"))
        using (StreamReader sr = new StreamReader("E:/clone_armable1/clone_armable1wt/Assets" + "/" + "Credentials_Data_Therapists.csv"))
        {
            //string line;
            // Read and display lines from the file until the end of 
            // the file is reached.
          /*  while ((line = sr.ReadLine()) != null)
            {
               saving_login_pw.therapist_offline_list.Add(line.Split(',')[0]);
            }*/
        }
        
        //saving_login_pw.patient_offline_list.Clear();
       using (StreamReader sr = new StreamReader(Application.persistentDataPath + /*"E:/clone_armable1/clone_armable1wt/Assets"*/  "/" + "Credentials_Data_Patients.csv"))
        {
           // string line;
            
            // Read and display lines from the file until the end of 
            // the file is reached.
          /*  while ((line = sr.ReadLine()) != null)
           {
               saving_login_pw.patient_offline_list.Add(line.Split(',')[0]);
           }*/
        }
       Debug.Log("hey");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void onClick(int num)
    {
        Debug.Log("hey");
        //0 for patient 1 for therapist
       // index = num;
      // commented on 7th October
       // saving_login_pw.therapist_offline_list.Clear();
        
        using (StreamReader sr = new StreamReader(Application.persistentDataPath + /*"E:/clone_armable1/clone_armable1wt/Assets"*/  "/" + "Credentials_Data_Therapists.csv"))
        {
            //string line;
            // Read and display lines from the file until the end of 
            // the file is reached.
         /*  while ((line = sr.ReadLine()) != null)
            {
               saving_login_pw.therapist_offline_list.Add(line.Split(',')[0]);
            }*/
        }
        

       // saving_login_pw.patient_offline_list.Clear();
        using (StreamReader sr = new StreamReader(Application.persistentDataPath + /*"E:/clone_armable1/clone_armable1wt/Assets"*/   "/" + "Credentials_Data_Patients.csv"))
       {
           // string line;

            // Read and display lines from the file until the end of 
            // the file is reached.
          /*  while ((line = sr.ReadLine()) != null)
            {
                saving_login_pw.patient_offline_list.Add(line.Split(',')[0]);
            }*/
        }
        
        
    }
}
