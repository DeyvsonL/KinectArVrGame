using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{

    private int points;

    private int dribbles;

    [SerializeField]
    private Text pointsText;

    [SerializeField]
    private Text dribblesText;

    // Use this for initialization
    void Start () {
		
	}

    [ClientRpc]
    public void RpcAddPoints()
    {
        pointsText.text = string.Format("{0}\nDefesas", ++points);
    }

    [ClientRpc]
    public void RpcAddDribbles() {
        dribblesText.text = string.Format("{0}\nDribles", ++dribbles);
    }

}
