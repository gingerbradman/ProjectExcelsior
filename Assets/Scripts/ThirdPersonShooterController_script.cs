using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;
using StarterAssets;

public class ThirdPersonShooterController_script : NetworkBehaviour
{
    [SerializeField] private GameObject playerCameraRoot;
    [SerializeField]
    private GameObject followVirtualCamera;
    [SerializeField]
    private GameObject aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();
    [SerializeField] private NetworkObject pfArrowProjectile;
    [SerializeField] private Transform spawnArrowPosition;
    [SerializeField]
    private Slider powerSlider;
    public float fillTime = 1f;
    private bool chargingShot = false;
    private bool sliderDirection = false; //false for increasing, true for decreasing

    private Vector3 mouseWorldPosition;
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    private void Awake()
    {

    }

    void Start()
    {

        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
        powerSlider = GameObject.Find("ArrowPowerSlider").GetComponent<Slider>();


        if (IsLocalPlayer)
        {
            //enabled = true;

            followVirtualCamera = thirdPersonController.GetFollowCamera();
            aimVirtualCamera = thirdPersonController.GetAimCamera();

            followVirtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = playerCameraRoot.transform;

            aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = playerCameraRoot.transform;

        }
        else
        {
            //enabled = false;
        }
    }

    void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            mouseWorldPosition = raycastHit.point;
        }

        if (starterAssetsInputs.aim)
        {
            if (aimVirtualCamera != null)
            {
                aimVirtualCamera.gameObject.SetActive(true);
            }

            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            if (starterAssetsInputs.shoot)
            {
                //Vector3 aimDir = (mouseWorldPosition - spawnArrowPosition.position).normalized;
                //Instantiate(pfArrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                //starterAssetsInputs.shoot = false;

                chargingShot = true;
                powerSlider.value += Time.deltaTime * fillTime;

            }
            else if (chargingShot == true)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    Vector3 aimDir = (mouseWorldPosition - spawnArrowPosition.position).normalized;
                    NetworkObject clone;
                    clone = Instantiate(pfArrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                    clone.SpawnWithOwnership(OwnerClientId);
                    ArrowProjectile_script cloneArrowScript = clone.GetComponent<ArrowProjectile_script>();
                    cloneArrowScript.SetForce(powerSlider.value * 5);
                    cloneArrowScript.SetDamage((int)powerSlider.value * 10);
                    starterAssetsInputs.shoot = false;
                    chargingShot = false;
                    powerSlider.value = powerSlider.minValue;
                }
                else
                {
                    if (IsOwner)
                    {
                        Vector3 aimDir = (mouseWorldPosition - spawnArrowPosition.position).normalized;
                        FireArrowServerRpc(aimDir, powerSlider.value);
                        starterAssetsInputs.shoot = false;
                        chargingShot = false;
                        powerSlider.value = powerSlider.minValue;

                    }
                }

            }
        }
        else
        {
            if (aimVirtualCamera != null)
            {
                aimVirtualCamera.gameObject.SetActive(false);
            }

            thirdPersonController.SetRotateOnMove(true);
            thirdPersonController.SetSensitivity(normalSensitivity);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }

        //starterAssetsInputs.shoot = false;
    }

    [ServerRpc]
    void FireArrowServerRpc(Vector3 aimDirection, float power)
    {
        NetworkObject clone;
        clone = Instantiate(pfArrowProjectile, spawnArrowPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));
        clone.SpawnWithOwnership(OwnerClientId);
        ArrowProjectile_script cloneArrowScript = clone.GetComponent<ArrowProjectile_script>();
        cloneArrowScript.SetForce(power * 5);
        cloneArrowScript.SetDamage((int)power * 10);
        
    }
}
