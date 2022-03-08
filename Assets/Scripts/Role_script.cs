using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Role_script : NetworkBehaviour
{
    private TextMeshProUGUI roleText;

    public NetworkVariable<int> currentNetworkRole = new NetworkVariable<int>(0);
    private role currentRole = role.Innocent;

    public enum role
    {
        Innocent = 0,
        Skeptic = 1,
        Betrayer = 2
    }

    private void Start()
    {
        if(IsClient && IsOwner){
            roleText = GameObject.Find("RoleText").GetComponent<TextMeshProUGUI>();
            roleText.text = RoleParseForText();
        }
    }

    private void OnEnable()
    {
        currentNetworkRole.OnValueChanged += UpdateRole;
    }

    private void OnDisable() 
    {
        currentNetworkRole.OnValueChanged -= UpdateRole;
    }

    void UpdateRole(int oldValue, int newValue)
    {
        if (IsOwner)
        {
            currentRole = (role)newValue;
            roleText.text = RoleParseForText();
        }
    }

    string RoleParseForText()
    {
        switch (currentRole)
        {
            case role.Innocent:
                return "Innocent";

            case role.Skeptic:
                return "Skeptic";

            case role.Betrayer:
                return "Betrayer";

            default:
                return "Error, No Role";
        }
    }
}
