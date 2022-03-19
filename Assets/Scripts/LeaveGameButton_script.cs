using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class LeaveGameButton_script : MonoBehaviour
{
    public void LeaveGame()
    {
        SceneManager.LoadScene(0);
        
        NetworkManager.Singleton.Shutdown();
    }
}
