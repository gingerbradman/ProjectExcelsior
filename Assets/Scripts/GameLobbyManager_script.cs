using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameLobbyManager_script : MonoBehaviour
{
	public static GameLobbyManager_script Instance { get; private set; } = null;

	private FacepunchTransport transport;
	public Lobby? CurrentLobby { get; private set; } = null;

	public List<Lobby> Lobbies { get; private set; } = new List<Lobby>(capacity: 100);

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);


		try
		{
			Steamworks.SteamClient.Init( 480, true);
		}
		catch
		{
			Debug.Log("Steam Client has not initialized!");
		}
	}

	private void Start()
    {


#if UNITY_EDITOR
		Debug.unityLogger.logEnabled = true;
#else
		Debug.unityLogger.logEnabled = Debug.isDebugBuild;
#endif

		

		transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();

		SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
	}

    private void OnDestroy()
	{
	 
		SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
		SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
		SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

		if (NetworkManager.Singleton == null)
			return;

		NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
		NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
	}

	private void OnApplicationQuit() => Disconnect();

	public async void StartHost(uint maxMembers)
	{
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;

		Debug.Log("Start Host in GameLobbyManager_script has been reached!");

		CurrentLobby = await SteamMatchmaking.CreateLobbyAsync((int)maxMembers);
    }

	public void StartClient(SteamId id)
	{
		NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

		transport.targetSteamId = id;

		Debug.Log($"Joining room hosted by {transport.targetSteamId}", this);

		if (NetworkManager.Singleton.StartClient())
			Debug.Log("Client has joined!", this);
	}

	public void Disconnect()
	{
		CurrentLobby?.Leave();

		if (NetworkManager.Singleton == null)
			return;

		NetworkManager.Singleton.Shutdown();
	}

	public async Task<bool> RefreshLobbies(int maxResults = 20)
	{
		try
		{
			Lobbies.Clear();

		var lobbies = await SteamMatchmaking.LobbyList
                .FilterDistanceClose()
		.WithMaxResults(maxResults)
		.RequestAsync();

		if (lobbies != null)
		{
			for (int i = 0; i < lobbies.Length; i++)
				Lobbies.Add(lobbies[i]);
		}

		return true;
		}
		catch (System.Exception ex)
		{
			Debug.Log("Error fetching lobbies", this);
			Debug.LogException(ex, this);
			return false;
		}
	}

	private Steamworks.ServerList.Internet GetInternetRequest()
	{
		var request = new Steamworks.ServerList.Internet();
		//request.AddFilter("secure", "1");
		//request.AddFilter("and", "1");
		//request.AddFilter("gametype", "1");
		return request;
	}

	#region Steam Callbacks

	private void HandleTransport(SteamId id) => NetworkManager.Singleton.GetComponent<FacepunchTransport>().targetSteamId = id;

	private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)

	{
		bool isSame = lobby.Owner.Id.Equals(id);

		Debug.Log($"Owner: {lobby.Owner}");
		Debug.Log($"Id: {id}");
		Debug.Log($"IsSame: {isSame}", this);

		StartClient(id);
		HandleTransport(id);
	}

	private void OnLobbyInvite(Friend friend, Lobby lobby) => Debug.Log($"You got a invite from {friend.Name}", this);

	private void OnLobbyMemberLeave(Lobby lobby, Friend friend) { }

	private void OnLobbyMemberJoined(Lobby lobby, Friend friend) { }

	private void OnLobbyEntered(Lobby lobby)
    {
		Debug.Log($"You have entered in lobby, clientId={NetworkManager.Singleton.LocalClientId}", this);

		if (NetworkManager.Singleton.IsHost)
			return;

		StartClient(lobby.Owner.Id);
	}

    private void OnLobbyCreated(Result result, Lobby lobby)
	{
		if (result != Result.OK)
        {
			Debug.LogError($"Lobby couldn't be created!, {result}", this);
			return;
		}

		lobby.SetFriendsOnly(); // Set to friends only!
		lobby.SetData("name", "Random Cool Lobby");
		lobby.SetJoinable(true);

		Debug.Log("Lobby has been created!");
	}

	#endregion

	#region Network Callbacks

	private void ClientConnected(ulong clientId) => Debug.Log($"I'm connected, clientId={clientId}");

    private void ClientDisconnected(ulong clientId)
	{
		Debug.Log($"I'm disconnected, clientId={clientId}");

		NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
		NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnected;
	}

	private void OnServerStarted() { }

    private void OnClientConnectedCallback(ulong clientId) => Debug.Log($"Client connected, clientId={clientId}", this);

    private void OnClientDisconnectCallback(ulong clientId) => Debug.Log($"Client disconnected, clientId={clientId}", this);

    #endregion
}
