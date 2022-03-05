using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Provides backing logic for any UI before MainMenu stage. Mostly we just load main menu
/// </summary>
public class StartupUI_script : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("Scene_Menu");
    }
}


