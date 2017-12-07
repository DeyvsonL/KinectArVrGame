using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCanvasPosition : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    var plane = GameObject.Find("BackgroundPlane");
	    transform.parent = plane.transform;
		transform.localPosition = new Vector3(0, 1500, -0.15f);
        transform.localScale = new Vector3(0.002f ,0.002f, 1);
	}
	
}
