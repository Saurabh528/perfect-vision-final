using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundTask : MonoBehaviour
{
    Texture2D destinationTexture;
    string rgbText;
    public string[] rgbAllowedScenes;
    bool showRGB = false;
    public const string KeyName_ShowRGB = "ShowRGBUnderCursor";
    [SerializeField] GUIStyle rgbTextStyle;
    [SerializeField] TextMeshProUGUI textDebugOutput;
    public static BackgroundTask Instance;
    public static bool MODE_CALORIMETERENABLED = true;
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;  // Maximum time interval between clicks for it to be considered a double-click

    void Awake(){
        if(!Instance){
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Directory.CreateDirectory(UtilityFunc.GetCalorimeterDataDir());
        destinationTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        // Add the onPostRender callback
        //Camera.onPostRender += OnPostRenderCallback;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnGUI()
    {
        if(!showRGB)
            return;
        Event e = Event.current;

        // Check if the left mouse button was clicked
        if (e.isMouse && e.type == EventType.MouseDown && e.button == 0)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickTime)
            {
                // Double-click detected
                Debug.Log("Double-click detected!");
                OnDoubleClick();
            }

            lastClickTime = Time.time;
        }
    }

    void OnDoubleClick()
    {
       // Get the mouse position in screen space
        Vector3 mousePos = Input.mousePosition;

        // Ensure the mouse position is within the screen bounds
        if (mousePos.x < 0 || mousePos.x >= Screen.width || mousePos.y < 0 || mousePos.y >= Screen.height){
            rgbText = "";
            return;
        }

        // Capture the entire screen after all cameras have rendered
        StartCoroutine(CaptureScreen(mousePos));
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        CheckRGBEnable(arg0.name);
    }

    /* void OnDestroy(){
        Camera.onPostRender -= OnPostRenderCallback;
    } */

    /* void OnGUI(){
        if(string.IsNullOrEmpty(rgbText) || !showRGB)
            return;
        var mousePosition = Input.mousePosition;

        float x = mousePosition.x;
        float y = Screen.height - mousePosition.y;
        float width = 0;
        float height = 0;
        var rect = new Rect(x, y, width, height);

        GUI.Label(rect, rgbText, rgbTextStyle);
    } */



    private System.Collections.IEnumerator CaptureScreen(Vector3 mousePos)
    {
        // Wait until end of frame to ensure all cameras have rendered
        yield return new WaitForEndOfFrame();

        // Create a new texture with the size of the screen
        

        // Read the pixel at the mouse position
        destinationTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        destinationTexture.Apply();

        // Get the color of the pixel
        Color color = destinationTexture.GetPixel((int)mousePos.x, (int)mousePos.y);

        // Output the RGB values to the console (or use them as needed)
        //Debug.Log("R: " + color.r + " G: " + color.g + " B: " + color.b);
        /*int ri = (int)(color.r * 255);
        int gi = (int)(color.g * 255);
        int bi = (int)(color.b * 255);
        rgbText = $"({ri}, {gi}, {bi})"; */
        string filename = $"{DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")}.png";
        File.WriteAllBytes($"{UtilityFunc.GetCalorimeterDataDir()}/{filename}", destinationTexture.EncodeToPNG());
        CalorimeterRecord record = new CalorimeterRecord(UtilityFunc.Color2Int(color),
            UtilityFunc.Color2Int(ColorCalibration.RedColor),
            UtilityFunc.Color2Int(ColorCalibration.CyanColor),
            UtilityFunc.Color2Int(ColorCalibration.BackColor));
        CalorimeterData.Instance.AddRecord(filename, record);
        CalorimeterData.Instance.Save(CalorimeterData.csvFileName);
    }

    public void CheckRGBEnable(string sceneName){
        showRGB = MODE_CALORIMETERENABLED && PlayerPrefs.GetInt(KeyName_ShowRGB, 0) == 1 && rgbAllowedScenes.Contains(sceneName);
    }

    public static void DebugString(string str){
        if(!Instance)
            return;
        Instance.textDebugOutput.text += str + "\n";
    }

    public static void LogValue(string name, System.Object value){
        if(!Instance)
            return;
        Instance.textDebugOutput.text += $"\n{name}: {value.ToString()}";
    }

}
