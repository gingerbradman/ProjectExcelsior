using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using StarterAssets;

public class ThirdPersonShooterController_script : NetworkBehaviour
{
    [SerializeField] private GameObject cameraMountPoint;
    [SerializeField] private GameObject aimCameraPrefab;
    [SerializeField] private GameObject mainCameraPrefab;
    [SerializeField] private GameObject playerCameraRoot;
    private GameObject mainVirtualCamera;
    private GameObject aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform spawnBulletPosition;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private StarterAssetsInputs Controls
    {
        get
        {
            if(starterAssetsInputs != null) { return starterAssetsInputs;}
            return starterAssetsInputs = new StarterAssetsInputs();
        }
    }
    private Animator animator;

    private void Awake() 
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner){ return; }
            
        mainVirtualCamera = Instantiate(mainCameraPrefab);
        aimVirtualCamera = Instantiate(aimCameraPrefab);

        mainVirtualCamera.gameObject.transform.parent = cameraMountPoint.transform;
        mainVirtualCamera.gameObject.transform.position = cameraMountPoint.transform.position;
        mainVirtualCamera.gameObject.transform.rotation = cameraMountPoint.transform.rotation;
        mainVirtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = playerCameraRoot.transform;

        aimVirtualCamera.gameObject.transform.parent = cameraMountPoint.transform;
        aimVirtualCamera.gameObject.transform.position = cameraMountPoint.transform.position;
        aimVirtualCamera.gameObject.transform.rotation = cameraMountPoint.transform.rotation;
        aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = playerCameraRoot.transform;
        

    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner){ return; }

        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            mouseWorldPosition = raycastHit.point;
        }

        if(starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            if (starterAssetsInputs.shoot)
            {
                Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
                Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                starterAssetsInputs.shoot = false;
            }
        } 
        else 
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetRotateOnMove(true);
            thirdPersonController.SetSensitivity(normalSensitivity);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }

        starterAssetsInputs.shoot = false;
    }
}
