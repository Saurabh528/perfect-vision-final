using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour {

	public float lifetime;

	public void Start () {

		Debug.Log("self");
		Destroy (gameObject, lifetime);
	}


}

