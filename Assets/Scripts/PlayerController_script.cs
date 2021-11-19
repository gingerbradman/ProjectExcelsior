using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;

public class PlayerController_script : NetworkBehaviour
{
    public StarterAssetsInputs starterAssetsInputs;
    public CharacterController controller;
    private Transform cam;

    public float speed = 6f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

#region Server

    private void CmdMove(Vector3 position)
    {
        controller.Move(position);
    }

#endregion

#region Client
    public void Start()
    {
        if(!IsLocalPlayer){ return; }
        enabled = true;
        cam = Camera.main.transform;
    }

    private void Update()
    {
        if(!IsLocalPlayer){ return; }

        Vector3 direction = new Vector3(starterAssetsInputs.move.x, 0f, starterAssetsInputs.move.y).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            CmdMove(moveDir.normalized * speed * Time.deltaTime);
        }
    }

#endregion
}
