using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Set_Trap_Trigger_script : NetworkBehaviour
{
    private Trap_Master_script parentTrap;

    private void Start() 
    {
        parentTrap = GetComponentInParent<Trap_Master_script>();
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "ActivePlayer"){

            if(other.GetComponent<Role_script>() == null){return;}

            Role_script otherRole = other.GetComponent<Role_script>();

            if(otherRole.currentNetworkRole.Value == (int)Role_script.role.Betrayer)
            {
                // Allow the Betrayer to arm the trap.
            }
        
        }
    }

}
