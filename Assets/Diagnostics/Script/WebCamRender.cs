using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class WebCamRender : MonoBehaviour
{
    WebCamTexture tex;
    [SerializeField] RawImage display;
    [SerializeField] AspectRatioFitter aspectFitter;

    bool isCaptuable;
    string recordFileName;
    Texture2D recordedImage;
    byte[] recordedBytes;

    public bool OpenCamera(){
        CloseCamera();

        WebCamDevice[] devices = WebCamTexture.devices;
        if(devices.Length == 0)
            return false;
        WebCamDevice device = devices[GlobalSettingUI.GetCurrentCameraIndex()];
        tex = new WebCamTexture(device.name);
        tex.Play();

        // Update aspect ratio
        float aspectRatio = (float)tex.width / (float)tex.height;
        aspectFitter.aspectRatio = aspectRatio;
        display.texture = tex;
        display.enabled = true;
        isCaptuable = true;
        return true;
    }

    void OnDestroy(){
        CloseCamera();
    }

    public void CloseCamera(){
        if(tex != null){
            if(display != null){
                display.texture = null;
                display.enabled = false;
            }
            tex.Stop();
            tex = null;
        }
    }

    public bool IsOpen(){
        return tex != null;
    }


    public void CaptureAndSaveImage(string filePath, bool closeAfterGrab = false, bool pauseAfterGrab = false)
    {
        StartCoroutine(CaptureAndSave(filePath, closeAfterGrab, pauseAfterGrab));
    }

    private IEnumerator CaptureAndSave(string filePath, bool closeAfterGrab, bool pauseAfterGrab)
    {
        isCaptuable = false;
        yield return new WaitForEndOfFrame();

        // Create a new Texture2D with the same dimensions as the WebCamTexture
        Texture2D capturedImage = new Texture2D(tex.width, tex.height);
        
        // Read pixels from the WebCamTexture
        capturedImage.SetPixels(tex.GetPixels());
        capturedImage.Apply();

        // Encode texture into PNG
        byte[] bytes = capturedImage.EncodeToPNG();

        // Write to a file
        File.WriteAllBytes(filePath, bytes);

        // Optionally, clean up the Texture2D if it's no longer needed
        Destroy(capturedImage);
        if(closeAfterGrab)
            CloseCamera();
        else if(pauseAfterGrab)
            tex.Pause();
        else
            isCaptuable = true;
    }

    public void Resume(){
        if(!IsOpen())
            return;
        tex.Play();
    }

    public void Pause(){
        if(!IsOpen())
            return;
        tex.Pause();
    }

    public bool IsCaptuable(){
        return isCaptuable;
    }

    public RawImage GetDisplayImage(){
        return display;
    }

    public void StartRecord(string filepath){
        if(!IsOpen() || !string.IsNullOrEmpty(recordFileName))
            return;
        recordFileName = filepath;
        if(File.Exists(recordFileName))
            File.Delete(recordFileName);
        recordedImage = new Texture2D(tex.width, tex.height);
        StartCoroutine(Routine_Record());
    }

    public void StopRecord(){
        recordFileName = "";
        Destroy(recordedImage);
    }

    IEnumerator Routine_Record(){
        while(tex != null && !string.IsNullOrEmpty(recordFileName)){
            if(!tex.isPlaying)
                ;//yield return new WaitForSeconds(0.01f);
            /* else if(recordedBytes == null){
                // Read pixels from the WebCamTexture
                recordedImage.SetPixels(tex.GetPixels());
                recordedImage.Apply();

                // Encode texture into PNG
                recordedBytes = recordedImage.EncodeToPNG();
                yield return new WaitForSeconds(0.01f);
            } */
            else if(!File.Exists(recordFileName)){
                if(recordedBytes != null){
                    // Write to a file
                    File.WriteAllBytes(recordFileName, recordedBytes);
                    recordedBytes = null;
                }
                else
                    BufferImage();
            }
            else if(File.Exists(recordFileName)){
                BufferImage();
            }
            yield return new WaitForSeconds(0.01f);
                
        }
    }

    void BufferImage(){
         // Read pixels from the WebCamTexture
        recordedImage.SetPixels(tex.GetPixels());
        recordedImage.Apply();

        // Encode texture into PNG
        recordedBytes = recordedImage.EncodeToPNG();
    }
}
