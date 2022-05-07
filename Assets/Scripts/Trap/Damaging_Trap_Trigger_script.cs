using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Damaging_Trap_Trigger_script : NetworkBehaviour
{
    private Trap_Master_script parentTrap;

    private void Start() 
    {
        parentTrap = GetComponentInParent<Trap_Master_script>();
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "ActivePlayer"){

            //Damage Player
        
        }
    }
}
