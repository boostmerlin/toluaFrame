using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Camera.main.GetComponent<MapMove>().SetMapSize(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
