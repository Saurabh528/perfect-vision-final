//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CircleController : MonoBehaviour
//{
//    public GameObject circle1;
//    public GameObject circle2;
//    public float rotationSpeed = 50f;
//    public float initialDistance = 1f;
//    public float distanceIncrement = 0.1f;

//    private float currentDistance;
//    private float angle;

//    void Start()
//    {
//        currentDistance = initialDistance;
//        UpdateCirclePositions();
//    }

//    void Update()
//    {
//        RotateCircles();
//        HandleDistanceChange();
//    }

//    void RotateCircles()
//    {
//        angle += rotationSpeed * Time.deltaTime;
//        UpdateCirclePositions();
//    }

//    void UpdateCirclePositions()
//    {
//        float radians = angle * Mathf.Deg2Rad;
//        Vector3 circle1Position = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * currentDistance;
//        Vector3 circle2Position = new Vector3(Mathf.Cos(radians + Mathf.PI), Mathf.Sin(radians + Mathf.PI), 0) * currentDistance;

//        circle1.transform.position = transform.position + circle1Position;
//        circle2.transform.position = transform.position + circle2Position;
//    }

//    void HandleDistanceChange()
//    {
//        if (Input.GetKey(KeyCode.UpArrow))
//        {
//            currentDistance += distanceIncrement * Time.deltaTime;
//            UpdateCirclePositions();
//        }
//        else if (Input.GetKey(KeyCode.DownArrow))
//        {
//            currentDistance -= distanceIncrement * Time.deltaTime;
//            UpdateCirclePositions();
//        }
//    }
//}


using UnityEngine;
using UnityEngine.UI;

public class CircleController1 : MonoBehaviour
{
    public GameObject BigCircle;
    public GameObject SmallCircle;
    public Button AntiClockwiseButton;
    public Button DistanceButton;
    public float rotationSpeed = 30f;
    public float distanceSpeed = 1f;

    private bool isRotating = false;
    private bool isChangingDistance = false;

    void Start()
    {
        AntiClockwiseButton.onClick.AddListener(ToggleRotation);
        DistanceButton.onClick.AddListener(ToggleDistanceChange);
    }

    void Update()
    {
        if (isRotating)
        {
            SmallCircle.transform.RotateAround(BigCircle.transform.position, Vector3.forward, -rotationSpeed * Time.deltaTime);
        }

        if (isChangingDistance)
        {
            float step = distanceSpeed * Time.deltaTime;
            Vector3 direction = (SmallCircle.transform.position - BigCircle.transform.position).normalized;
            SmallCircle.transform.position = Vector3.MoveTowards(SmallCircle.transform.position, BigCircle.transform.position, -step);
        }
    }

    void ToggleRotation()
    {
        isRotating = !isRotating;
    }

    void ToggleDistanceChange()
    {
        isChangingDistance = !isChangingDistance;
    }
}
