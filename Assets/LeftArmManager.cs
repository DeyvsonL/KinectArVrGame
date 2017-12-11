using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LeftArmManager : NetworkBehaviour
{

    private GameObject _head;
    private GameObject _camera;
	// Use this for initialization
	void Start () {
		
	}

    public override void OnStartLocalPlayer()
    {
        _head = GameObject.Find("Head(Clone)");
        _camera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update ()
    {
        if (!isLocalPlayer) return;
        if (_head == null)
        {
            Debug.Log(null);
        }
        else
        {
            Debug.Log(_head.transform.position);
            _camera.transform.position = _head.transform.position;
        }
    }
}
