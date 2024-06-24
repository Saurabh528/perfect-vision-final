using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using TMPro;
using System.Net;
public class CheckForUpdate : MonoBehaviour
{
    [SerializeField] TweenAppear tweenNewVersion;
    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string _version)
        {
            string[] versionStrings = _version.Split('.');
            if (versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(versionStrings[0]);
            minor = short.Parse(versionStrings[1]);
            subMinor = short.Parse(versionStrings[2]);
        }

        internal bool IsUpdatable(Version onlineVersion)
        {
            if (major < onlineVersion.major)
            {
                return true;
            }
            else
            {
                if (minor < onlineVersion.minor)
                {
                    return true;
                }
                else
                {
                    if (subMinor < onlineVersion.subMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
    
    [SerializeField] TextMeshProUGUI localVersionTxt, onlineVersionTxt;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckForOnlieVersion());
    }

    IEnumerator CheckForOnlieVersion(){
        string versionText = "1.0.0";
        // Load the text asset from the Resources folder
        TextAsset textAsset = Resources.Load<TextAsset>("Version");

        // Check if the text asset is not null
        if (textAsset != null)
            versionText = textAsset.text;
        Version localVersion = new Version(versionText);
        localVersionTxt.text = localVersion.ToString();
        try{
            WebClient webClient = new WebClient();
            Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1C_C61DPiFoyult1hiZgpTLLN0asxRXG0"));
            if (localVersion.IsUpdatable(onlineVersion))
            {
                onlineVersionTxt.text = onlineVersion.ToString();
                tweenNewVersion.Appear();
            }
        }
        catch (Exception ex){
            Debug.Log($"Couldn't get online version: {ex.ToString()}");
        }
        yield return null;
    }

    public void OnUpdateBtnClick(){
        string os = SystemInfo.operatingSystem;

        if (os.Contains("Windows"))
        {
            if(GameConst.COMPANYNAME.StartsWith("BinoPlay"))
                Application.OpenURL("https://drive.google.com/uc?export=download&id=1LnZyQ_ZGIAWNDXPlkUd2oh5TTDLmv1dh");
            else
                Application.OpenURL("https://drive.google.com/uc?export=download&id=1lCI-ZZstN6paqxr2A2LcvT1pioJ1YeNK");
        }
        else/*  if (os.Contains("Mac OS") || os.Contains("MacOS")) */
        {
            if(GameConst.COMPANYNAME.StartsWith("BinoPlay"))
                Application.OpenURL("https://drive.google.com/uc?export=download&id=19Por_Ht6a2d5-lgY5lCbTsNaB2dTp0uq");
            else
                Application.OpenURL("https://drive.google.com/uc?export=download&id=19Por_Ht6a2d5-lgY5lCbTsNaB2dTp0uq");
        }
        
        ExitUI.ForceExit();
    }


}
