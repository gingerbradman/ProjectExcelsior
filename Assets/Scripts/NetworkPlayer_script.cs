using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using Unity.Netcode;

public class NetworkPlayer_script : NetworkBehaviour
{
    [SerializeField] private ThirdPersonShooterController_script thirdPersonShooterController_Script;
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;

    private void Awake() 
    {
        if(!IsOwner){ return; }

        playerInput.enabled = true;
        thirdPersonShooterController_Script.enabled = true;
        thirdPersonController.enabled = true;
        characterController.enabled = true;
    }
}
