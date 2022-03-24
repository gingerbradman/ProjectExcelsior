using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DapperDino.UMT.Lobby.Networking;
public class GameManager_script : NetworkBehaviour
{

    [SerializeField]
    private bool inDebugMode = true;
    GameObject[] alivePlayers;

    TMP_Text gameStatusText;

    [SerializeField]
    int numberOfInnocent;
    [SerializeField]
    int numberOfSkeptic;
    [SerializeField]
    int numberOfBetrayer;

    UIManager_script uiManager;
    [SerializeField]
    private GameObject avatarPrefab;
    [SerializeField]
    private GameObject persistentPlayerPrefab;
    private ServerGameNetPortal serverGameNetPortal;

    private List<GameObject> playerAvatarList = new List<GameObject>();
    private List<GameObject> playerRoleList = new List<GameObject>();
    [SerializeField]
    private NetworkVariable<int> partyGold = new NetworkVariable<int>(0);

    void Awake()
    {
        if (IsServer)
        {

        }
    }

    private void OnEnable()
    {
        partyGold.OnValueChanged += UpdatePartyGold;
    }

    private void OnDisable() 
    {
        partyGold.OnValueChanged -= UpdatePartyGold;
    }


    // Start is called before the first frame update
    void Start()
    {
        alivePlayers = GameObject.FindGameObjectsWithTag("ActivePlayer");
        gameStatusText = GameObject.Find("Game Status Text").GetComponent<TMP_Text>();
        uiManager = this.GetComponent<UIManager_script>();
        serverGameNetPortal = GameObject.Find("NetPortals").GetComponent<ServerGameNetPortal>();

        if (inDebugMode)
        {
            uiManager.debugUICanvas.SetActive(true);
            return;
        }

        StartCoroutine(SpawnPlayersWaitCoroutine());
    }

    async void SpawnPlayers()
    {
        if(!IsServer){ return; }


        for (int i = 0; i < serverGameNetPortal.getClientData().Count; i++)
        {

            PlayerData? currentPlayer = serverGameNetPortal.GetPlayerData((ulong)i);

            if (currentPlayer == null)
            {
                return;
            }

            GameObject avatar = Instantiate(avatarPrefab, Vector3.zero, Quaternion.identity);
            avatar.GetComponent<NetworkObject>().SpawnAsPlayerObject(currentPlayer.Value.ClientId);
            playerRoleList.Add(avatar);
            playerAvatarList.Add(avatar);
        }

        AssignBetrayer();
        AssignSkeptic();
        playerRoleList.Clear(); //Clear list for next Spawn Players

    }

    void SpawnPersistentPlayers()
    {
        if(!IsServer){ return; }


        for (int i = 0; i < serverGameNetPortal.getClientData().Count; i++)
        {

            PlayerData? currentPlayer = serverGameNetPortal.GetPlayerData((ulong)i);

            if (currentPlayer == null)
            {
                return;
            }

            GameObject persistentPlayer = Instantiate(persistentPlayerPrefab, Vector3.zero, Quaternion.identity);
            persistentPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(currentPlayer.Value.ClientId);
        }
    }

    void RemoveAvatars()
    {
        if(!IsServer){ return; }

        for (int i = 0; i < serverGameNetPortal.getClientData().Count; i++)
        {

            PlayerData? currentPlayer = serverGameNetPortal.GetPlayerData((ulong)i);

            if (currentPlayer == null)
            {
                return;
            }

            Destroy(playerAvatarList[i].gameObject);
        }

        playerAvatarList.Clear();
    }

    IEnumerator SpawnPlayersWaitCoroutine()
    {
        yield return new WaitForSeconds(5);

        SpawnPlayers();

        StopCoroutine(SpawnPlayersWaitCoroutine());
    }

    IEnumerator NextRoundWaitCoroutine()
    {
        yield return new WaitForSeconds(10);

        if(IsServer){

            SpawnPersistentPlayers();
            RemoveAvatars();
            serverGameNetPortal.EndRound();

        }

        StopCoroutine(NextRoundWaitCoroutine());
    }

    void UpdatePartyGold(int oldValue, int newValue)
    {
        if(!IsServer) { return; }

        if(partyGold.Value >= 1000)
        {
            InnocentWinClientRpc();
        }

    }

    public void AddPartyGold(int x)
    {
        if(!IsServer){ return; }

        partyGold.Value += x;
    }


    public void AssessGame()
    {
        if (IsServer)
        {

            alivePlayers = GameObject.FindGameObjectsWithTag("ActivePlayer");

            numberOfInnocent = 0;
            numberOfSkeptic = 0;
            numberOfBetrayer = 0;
            for (int i = 0; i < alivePlayers.Length; i++)
            {
                if (alivePlayers[i].GetComponent<Role_script>().currentNetworkRole.Value == (int)Role_script.role.Innocent)
                {
                    numberOfInnocent++;
                }
                if (alivePlayers[i].GetComponent<Role_script>().currentNetworkRole.Value == (int)Role_script.role.Skeptic)
                {
                    numberOfInnocent++;
                    numberOfSkeptic++;
                }
                if (alivePlayers[i].GetComponent<Role_script>().currentNetworkRole.Value == (int)Role_script.role.Betrayer)
                {
                    numberOfBetrayer++;
                }
            }

            if (numberOfInnocent == 0 && numberOfBetrayer >= 1)
            {
                if (inDebugMode)
                {
                    return;
                }
                else
                {
                    BetrayersWinClientRpc();
                }
            }
            else if (numberOfBetrayer == 0 && numberOfInnocent >= 1)
            {
                if (inDebugMode)
                {
                    return;
                }
                else
                {
                    InnocentWinClientRpc();
                }
            }
        }
    }

    private void AssignBetrayer()
    {
        int clientToBecomeBetrayer = Random.Range( 0, playerRoleList.Count); //+1 to include the upper limit; Random Range exludes the maximum
        playerRoleList[clientToBecomeBetrayer].GetComponent<Role_script>().currentNetworkRole.Value = 2; //2 is the Enum value for the betrayer
        playerRoleList.Remove(playerRoleList[clientToBecomeBetrayer]);                                   //Remove player from role list, so they aren't reassinged
    }

    private void AssignSkeptic()
    {
        int clientToBecomeSkeptic = Random.Range( 0, playerRoleList.Count); //+1 to include the upper limit; Random Range exludes the maximum
        playerRoleList[clientToBecomeSkeptic].GetComponent<Role_script>().currentNetworkRole.Value = 1; //1 is the Enum value for the Skeptic
        playerRoleList.Remove(playerRoleList[clientToBecomeSkeptic]); 
    }

    [ClientRpc]
    private void InnocentWinClientRpc()
    {
        gameStatusText.text = "Innocent Win!";
        StartCoroutine(NextRoundWaitCoroutine());
    }

    [ClientRpc]
    private void BetrayersWinClientRpc()
    {
        gameStatusText.text = "Betrayers Win!";
        StartCoroutine(NextRoundWaitCoroutine());
    }

    [ClientRpc]
    private void ClearTextClientRpc()
    {
        gameStatusText.text = "";
    }
}
