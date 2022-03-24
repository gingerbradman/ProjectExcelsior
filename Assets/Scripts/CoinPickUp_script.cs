using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CoinPickUp_script : NetworkBehaviour
{
    public int value;

    private void OnTriggerEnter(Collider other) 
    {
        if(!IsServer){ return ; }

        if(other.tag == "Environment"){ return; }

        Debug.Log("Gold Collision");

        NetworkObject otherNetworkObject = other.GetComponent<NetworkObject>();
        PlayerBase_script otherPlayerBase;

        if(otherNetworkObject == null){ return; }

        if(other.tag == "ActivePlayer")
        {
            otherPlayerBase = other.GetComponent<PlayerBase_script>();

            otherPlayerBase.AddGold(value);

            this.GetComponent<NetworkObject>().Despawn();

        }
    }

}
