using DapperDino.UMT.Lobby.Networking;
using TMPro;
using UnityEngine;

namespace DapperDino.UMT.Lobby
{
    public class MainMenuUI : MonoBehaviour
    {

        [SerializeField]
        private GameObject hostClientButtons;
        [SerializeField]
        private GameObject stopClientButton;

        private void Start()
        {

        }

        public void OnHostClicked()
        {
            GameNetPortal.Instance.StartHost();
        }

        public void OnClientClicked()
        {
            ClientGameNetPortal.Instance.StartClient();
            stopClientButton.SetActive(true);
            hostClientButtons.SetActive(false);
        }

        public void OnStopClientClicked()
        {
            ClientGameNetPortal.Instance.StopClient();
            hostClientButtons.SetActive(true);
            stopClientButton.SetActive(false);
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}

