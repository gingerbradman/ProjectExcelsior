using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using StarterAssets;

public class ArrowProjectile_script : NetworkBehaviour
{

    private Rigidbody arrowRigidbody;
    [SerializeField]
    private int travelSpeed = 10;
    private int damage;
    public void SetDamage(int x){damage = x;}
    private float force;
    public void SetForce(float x){force = x;}

    private void Awake() 
    {
        arrowRigidbody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        arrowRigidbody.velocity = transform.forward * travelSpeed * force;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if(!IsServer)
        {
            return;
        }

        NetworkObject otherNetworkObject = other.GetComponent<NetworkObject>();

        if(otherNetworkObject == null)
        {
            this.gameObject.GetComponent<NetworkObject>().Despawn();
            return;
        }

        if(otherNetworkObject.OwnerClientId == this.OwnerClientId)
        {
            return;
        }

        if(otherNetworkObject.IsOwnedByServer)
        {
            NetworkObject serverPlayer = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
            serverPlayer.GetComponent<ThirdPersonController>().TakeDamage(damage);
            this.gameObject.GetComponent<NetworkObject>().Despawn();
            return;
        }

        ProjectileCollisionServerRpc(otherNetworkObject.OwnerClientId);
        this.gameObject.GetComponent<NetworkObject>().Despawn();


    }

    [ServerRpc]
    void ProjectileCollisionServerRpc(ulong other)
    {
        Debug.Log("Arrow Collision");
        NetworkObject otherNetworkObject = NetworkManager.Singleton.ConnectedClients[other].PlayerObject;
        
        if(otherNetworkObject.IsPlayerObject)
        {
            otherNetworkObject.GetComponent<ThirdPersonController>().TakeDamage(damage);            
        }
    }
}
