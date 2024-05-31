using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DiagnosticController : MonoBehaviour
{
    public void OnBtnClose(){
        if(ResultExist())
            PopupUI.ShowQuestionBox("Would you like to save the diagnostic result?", OnClickSaveButton, ExitScene);
        else
            ExitScene();
    }

    void OnEnable(){
		ExitUI.EnableShutdownButton(false);
	}

	void OnDestroy(){
		ExitUI.EnableShutdownButton(true);
	}

    public abstract void AddResults();

    public void OnClickSaveButton(){
        AddResults();
        SaveResult();
        ExitScene();
    }

    public virtual bool ResultExist(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        if(pr == null)
            return false;
        return true;
    }

    public void SaveResult(){
        
        PatientDataMgr.SavePatientData();
    }

    void ExitScene(){
        ChangeScene.LoadScene("Diagnostic");
    }
}
