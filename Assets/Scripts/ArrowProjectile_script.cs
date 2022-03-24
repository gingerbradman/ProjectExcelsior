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
        if(!IsServer) { return; }

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

        if(other.tag == "ActivePlayer"){
            NetworkObject otherPlayerObject = NetworkManager.Singleton.ConnectedClients[otherNetworkObject.OwnerClientId].PlayerObject;
            otherPlayerObject.GetComponent<PlayerBase_script>().TakeDamage(damage);
            this.gameObject.GetComponent<NetworkObject>().Despawn();
            return;
        }

    }

    /*

    [ServerRpc]
    void ProjectileCollisionServerRpc(ulong other)
    {
        Debug.Log("Arrow Collision");
        ProjectileCollisionClientRpc(other);
    }

    [ClientRpc]
    void ProjectileCollisionClientRpc(ulong other)
    {
        NetworkObject otherNetworkObject = NetworkManager.Singleton.ConnectedClients[other].PlayerObject;
        
        if(otherNetworkObject.IsPlayerObject)
        {
            otherNetworkObject.GetComponent<PlayerBase_script>().TakeDamage(damage);            
        }
    }
    */
}
