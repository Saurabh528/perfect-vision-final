using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.NetworkInformation;
using System.IO;
public class DataManagement : MonoBehaviour
{
    public static string PlayerName; //assigned every time the log in button is pressed
   // private string theDate; //calculated at teh begenning of this script
    public static double distance = 0; //start recording distance for each game
    public static bool recordDistance = false; // controled by the current game player movement script ; called from Standard.cs ; stopped from 
    public static Vector3 previousPosition;
    public static int currentLevel = 1; //updated everytime a new game scene is launched
    public static int currentScore = 0; //updated when game ends
    public static float playTime; //updated everytime a new game is launched ;Set from dropDownGameSelect
    public static string deviceID; //updated at start of this script
    public static bool saveInfo = false; //called from game scripts
    public static string gameName; //Set from dropDownGameSelect
    private static string path;


    public static string theDate;
    // Use this for initialization
    void Start()
    {
        theDate = System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");
        path = Application.persistentDataPath;
        gameName = "";
        DontDestroyOnLoad(this.gameObject);
        //theDate = System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");
        deviceID = ShowNetworkInterfaces(); //get MACID
       // previousPosition = tinker_u_sound.final_position;
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (recordDistance == true)
        {
            PathToCSV();
            
        }
        if (saveInfo == true) //game script has finished, write information to file
        {
           
            saveInfo = !saveInfo;
            WriteToCSV();
           // distance = 0;

        }
        //if(gameName == "Balloon")
        // //  currentScore = PlayerScoreDestroy.score;
        //else
        //currentScore = ScoreScript.score;
       
    }

    public String ShowNetworkInterfaces()
    {
        string mac = null;

        IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface adapter in nics)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();

            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + "-");
                }
            }
            // info += mac + "\n";

            // info += "\n";
            
        }

        return mac;

    }

    public void PathToCSV()
    {
        save_csv1("Path" + PlayerName + "_" + gameName + theDate , Time.timeSinceLevelLoad + "," );
    }

    public void WriteToCSV()
    {
        save_csv("Master" + PlayerName + "_", theDate + "," + PlayerName + "," + gameName + "," + (distance / 100f) + "," + playTime + "," + currentLevel + "," + currentScore);// + "," + deviceID);
    }

    public void save_csv(String file_name, String data)
    {
        string path = Application.persistentDataPath;

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path + "/" + file_name + ".csv", true))
        {



            file.WriteLine(data);
            FileInfo fInfo = new FileInfo(@path + "/" + file_name + ".csv");
            //fInfo.IsReadOnly = true;

        }



    }

    public void save_csv1(String file_name, String data)
    {
        string path = Application.persistentDataPath;

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path + "/" + file_name + ".csv", true))
        {


            file.WriteLine(data);
            FileInfo fInfo = new FileInfo(@path + "/" + file_name + ".csv");
            //fInfo.IsReadOnly = true;

        }



    }
    /* public static void save_txt(string file_name, List<String> data)
     {

         using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path + "/" + file_name + ".txt", false))
         {
             for (int i = 0; i < data.Count; i++)
                 file.WriteLine(data[i]);
         }

     }

     public static void save_txt_upload(string file_name, List<String> data)
     {
         using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path + "/" + file_name + ".txt", false))
         {
             for (int i = 0; i < data.Count; i++)


                 file.WriteLine(data[i] + "\t" + "ID= " + (10000 + i));
         }

     }

     public static List<string> load_name_txt(string file_name)
     {
         List<string> a = new List<string>();
         //Debug.Log("Opening yo");
         path = Application.persistentDataPath;

         if (System.IO.File.Exists(@path + "/" + file_name + ".txt"))
         {
             a = new List<string>(System.IO.File.ReadAllLines(@path + "/" + file_name + ".txt"));

         }
         return a;
     }

     public static void save_txt(string file_name, String data)
     {
         //string path = Application.persistentDataPath;
         using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path + "/" + file_name + ".txt", true))
         {



             file.WriteLine(data);

         }
     }*/
}
