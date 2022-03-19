using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using StarterAssets;
using TMPro;

public class PlayerBase_script : NetworkBehaviour
{
    private int maximumHealth = 100;
    [SerializeField]
    private NetworkVariable<int> health = new NetworkVariable<int>(100);
    private TextMeshProUGUI healthText;
    private Slider healthSlider;
    private GameManager_script gm;
    private GameObject menuGui;

    private ThirdPersonController thirdPersonController;

    // Start is called before the first frame update
    void Start()
    {
        if(IsClient && IsOwner){
            gm = GameObject.Find("GameManager").GetComponent<GameManager_script>();
            healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
            healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
            menuGui = GameObject.Find("MenuCanvas");
            menuGui.SetActive(false);
            thirdPersonController = GetComponent<ThirdPersonController>();
            healthSlider.value = health.Value;
            healthText.text = "Health: " + health.Value + "/" + maximumHealth;
        }
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(menuGui.activeInHierarchy)
            {
                menuGui.SetActive(false);
            }
            else
            {
                menuGui.SetActive(true);
            }
        }
    }

    private void OnEnable()
    {
        if(IsClient && IsOwner){
            gm = GameObject.Find("GameManager").GetComponent<GameManager_script>();
            healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
            healthText = GameObject.Find("HealthText").GetComponent<TextMeshProUGUI>();
            menuGui = GameObject.Find("MenuCanvas");
            menuGui.SetActive(false);
            thirdPersonController = GetComponent<ThirdPersonController>();
            healthSlider.value = health.Value;
            healthText.text = "Health: " + health.Value + "/" + maximumHealth;
        }
        
        health.OnValueChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        health.OnValueChanged -= UpdateHealth;
    }

    void UpdateHealth(int oldValue, int newValue)
    {
        if (IsOwner)
        {
            Debug.Log("Update Health Function");
            healthSlider.value = newValue;
            healthText.text = "Health: " + newValue + "/" + maximumHealth;

            if(health.Value <= 0)
            {
                Debug.Log("Death Clip");
                PlayerDeath();
            }
        }
    }

    public void TakeDamage(int x)
    {
        Debug.Log("TakeDamage Function");

        if(health.Value - x <= 0){
            health.Value = 0;
        }
        else 
        {
            health.Value -= x;
        }
    }

    public void PlayerDeath()
    {
        PlayerDeathServerRPC();

        thirdPersonController.Die();
    }

    [ServerRpc]
    private void PlayerDeathServerRPC ()
    {
        this.gameObject.tag = "Spectator";

        gm.AssessGame();
    }
}
