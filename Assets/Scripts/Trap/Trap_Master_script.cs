using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Trap_Master_script : NetworkBehaviour
{

    private bool armed = false;

    public Set_Trap_Trigger_script setTrap;
    public Set_Off_Trap_Trigger_script setOffTrap;
    public Damaging_Trap_Trigger_script damageTrap;
    public Inspect_Trap_Trigger_script inspectTrap;

    private void Awake() 
    {
        setOffTrap.gameObject.SetActive(false);
        damageTrap.gameObject.SetActive(false);
    }

    public void ArmTrap()
    {
        armed = true;
        setOffTrap.gameObject.SetActive(true);
    }

    public void TriggerTrap()
    {
        damageTrap.gameObject.SetActive(true);
    }

    public void ResetTrap()
    {
        armed = false;
        setOffTrap.gameObject.SetActive(false);
        damageTrap.gameObject.SetActive(false);
    }


}
