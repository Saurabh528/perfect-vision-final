using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuscleResultViewer : MonoBehaviour
{
    [SerializeField] RectTransform[] _points;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool ShowPoints(MuscleBalanceResultData data, Transform[] basePts){//return false if some of deviations does not exist
        bool allPtExist = true;
        for( int i = 0; i < basePts.Length; i++){
            if(data._dicDeviation.ContainsKey(basePts[i].name)){
                _points[i].gameObject.SetActive(true);
                _points[i].anchoredPosition = new Vector3(data._dicDeviation[basePts[i].name].x, data._dicDeviation[basePts[i].name].y, 0);
            }
            else{
                allPtExist = false;
                _points[i].gameObject.SetActive(false);
            }
        }
        return allPtExist;
    }
}
