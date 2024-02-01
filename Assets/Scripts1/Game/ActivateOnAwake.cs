using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnAwake : MonoBehaviour
{
    [SerializeField] GameObject[] m_objects;

	void Awake()
	{
		foreach(GameObject obj in m_objects)
			obj.SetActive(true);
	}
}
