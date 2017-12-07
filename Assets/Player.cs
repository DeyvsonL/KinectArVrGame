﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{

    private int points;

    [SerializeField]
    private Text pointsText;

	// Use this for initialization
	void Start () {
		
	}

    [ClientRpc]
    public void RpcSetPoints()
    {
        pointsText.text = string.Format("{0}\nDefesas", ++points);
    }

}
