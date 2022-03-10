using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;
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

    void Awake()
    {
        if (IsServer)
        {

        }
    }


    // Start is called before the first frame update
    void Start()
    {
        alivePlayers = GameObject.FindGameObjectsWithTag("ActivePlayer");
        gameStatusText = GameObject.Find("Game Status Text").GetComponent<TMP_Text>();
        uiManager = this.GetComponent<UIManager_script>();

        if (inDebugMode)
        {
            uiManager.debugUICanvas.SetActive(true);
            return;
        }

        StartCoroutine(SpawnPlayersWaitCoroutine());
    }

    void SpawnPlayers()
    {
        if(!IsServer){ return; }

        ServerGameNetPortal gameNetPortal = GameObject.Find("NetPortals").GetComponent<ServerGameNetPortal>();

        for (int i = 0; i < gameNetPortal.getClientData().Count; i++)
        {

            PlayerData? currentPlayer = gameNetPortal.GetPlayerData((ulong)i);

            if (currentPlayer == null)
            {
                return;
            }

            GameObject avatar = Instantiate(avatarPrefab, Vector3.zero, Quaternion.identity);
            avatar.GetComponent<NetworkObject>().SpawnAsPlayerObject(currentPlayer.Value.ClientId);
            //avatar.GetComponent<NetworkObject>().ChangeOwnership(currentPlayer.Value.ClientId);
        }
    }

    IEnumerator SpawnPlayersWaitCoroutine()
    {
        yield return new WaitForSeconds(5);

        SpawnPlayers();

        StopCoroutine(SpawnPlayersWaitCoroutine());
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

    [ClientRpc]
    private void InnocentWinClientRpc()
    {
        gameStatusText.text = "Innocent Win!";
    }

    [ClientRpc]
    private void BetrayersWinClientRpc()
    {
        gameStatusText.text = "Betrayers Win!";
    }
}
