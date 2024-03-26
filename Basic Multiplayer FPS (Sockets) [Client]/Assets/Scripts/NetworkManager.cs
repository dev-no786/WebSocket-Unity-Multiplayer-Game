using System;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField] private GameManager gameManager;
    private WebSocket _webSocket;

    public ChatBox _ChatBox;
    
    #region Network Events

    public Action<UserJSON> OnNewPlayer;
    public Action OnGameJoin;
    public Action<EnemiesJSON> OnSpawnEnemies;
    public Action<UserJSON> OnForeignPlayer;
    public Action<PositionJSON> OnPlayerMove;
    public Action<RotationJSON> OnPlayerRotate;
    public Action<string> OnPlayerShoot;
    public Action<string> OnPlayerLeft;
    public Action<UserHealthJSON> OnHealthChange;
    
    #endregion
    
    private string septr = "#";
    private string networkId;
    public string NetworkId
    {
        get { return networkId; }
    }

    [SerializeField]private string gameId;
    public string GameId => gameId;
    
    public string playerName;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            
            //StartCoroutine(gameManager.SubNetworkEvents());
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    void JoinGame()
    {
        
    }

    /*IEnumerator ConnectToServer()
    {
        yield return new WaitForSeconds(0.5f);
    }*/
    
    private async void Start()
    {
        EnemySpawner _enemySpawner = GetComponent<EnemySpawner>();
        PlayerSpawner _playerSpawner = GetComponent<PlayerSpawner>();

        
        // websocket = new WebSocket("ws://echo.websocket.org");
        _webSocket = new WebSocket("ws://localhost:3000");

        _webSocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            
        };

        _webSocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        _webSocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        _webSocket.OnMessage += (bytes) =>
        {
            // Reading a plain text message
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
            string method = "";
            string jsonString = "";
            
            if (message.IndexOf(septr, StringComparison.Ordinal) != -1)
            {
                method = message.Split(septr)[0];
                jsonString = message.Split(septr)[1];
            }

            switch (method)
            {
                case NetworkConstant.CHAT:
                    var chatJson = JsonUtility.FromJson<ChatBox.ChatJson>(jsonString);
                    _ChatBox.GetNewChatLine(chatJson.chatMsg, chatJson.name);
                    break;
                
                case NetworkConstant.METHOD_JOIN:
                    Debug.Log("Id received :"+jsonString);
                    networkId = jsonString;
                    // send enemy and player spawn point list and player-name 
                    PlayerJSON response = new PlayerJSON(playerName, _playerSpawner.playerSpawnPoints,
                        _enemySpawner.enemySpawnPoints);
                    string json = JsonUtility.ToJson(response);
                    Debug.Log("send response : " + json);
                    _webSocket.SendText(NetworkConstant.METHOD_PLAY + septr + json);
                    break;
                
                case NetworkConstant.METHOD_NEWPLAYER:
                    // spawn new player
                    var userJson = UserJSON.CreateFromJson(jsonString);
                    Debug.Log("new player :"+userJson.name);
                    if (userJson.clientId == networkId)
                    {
                        _ChatBox.gameObject.SetActive(true);
                    }

                    OnNewPlayer?.Invoke(userJson);
                    break;
                
                case NetworkConstant.METHOD_SPAWNENEMIES:
                    Debug.Log("enemies :"+jsonString);
                    var enemiesJson = JsonUtility.FromJson<EnemiesJSON>(jsonString);
                    OnSpawnEnemies?.Invoke(enemiesJson);
                    //spawn foreign player now that enemies are placed
                    _webSocket.SendText(NetworkConstant.METHOD_SPAWNFOREIGNPLAYERS + septr + "");
                    break;
                
                case NetworkConstant.METHOD_SPAWNFOREIGNPLAYERS:
                    UserJSON foreignUserJson = UserJSON.CreateFromJson(jsonString);
                    Debug.Log("non local play received: "+foreignUserJson.clientId);
                    OnForeignPlayer?.Invoke(foreignUserJson);
                    break;
                
                case NetworkConstant.METHOD_PLAYERMOVE:
                    PositionJSON newPositionJson = JsonUtility.FromJson<PositionJSON>(jsonString);
                    Debug.Log("player move - "+newPositionJson.clientId + "-"
                              + jsonString);
                    if (newPositionJson.clientId != networkId)
                    {
                        OnPlayerMove?.Invoke(newPositionJson);
                    }
                    break;
                case NetworkConstant.METHOD_PLAYERROTATE:
                    var newRotationJson = JsonUtility.FromJson<RotationJSON>(jsonString);
                    Debug.Log("player move - "+newRotationJson.clientId + "-"
                              + jsonString);
                    if (newRotationJson.clientId != networkId)
                    {
                        OnPlayerRotate?.Invoke(newRotationJson);
                    }
                    break;
                
                case NetworkConstant.METHOD_PLAYERSHOOT:
                    string playerId = jsonString;
                    Debug.Log("player shoot by - "+playerId +"-");

                    if (playerId != networkId)
                    {
                        Debug.Log("shoot event invoked!!");
                        OnPlayerShoot?.Invoke(playerId);
                    }
                    break;
                
                case NetworkConstant.METHOD_HEALTH:
                    UserHealthJSON healthJson = UserHealthJSON.CreateFromJson(jsonString);
                    Debug.Log("health change of player- " + healthJson.clientId + "-"
                              + healthJson.name + " hp: " + healthJson.health);
                    OnHealthChange?.Invoke(healthJson);
                    break;
                case NetworkConstant.METHOD_PLAYERLEFT:
                    string clientId = jsonString;
                    OnPlayerLeft?.Invoke(clientId);
                    break;
            }
        };

        // Keep sending messages at every 0.3s
        //InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        await _webSocket.Connect();
    }

    public async void JoinGameServer()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendText(NetworkConstant.METHOD_JOIN + septr + "");
        }
    }
    
    [ContextMenu("Join Old Game")]
    public async void JoinOldGame()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var gameJson = new GameIdJson(gameId);
            string res = JsonUtility.ToJson(gameJson);
            await _webSocket.SendText(NetworkConstant.METHOD_OLDROOM + septr + res);
        }
    }
    
    public async void SendChatMsg(string msg, string name)
    {
        var resJson = JsonUtility.ToJson(msg);
    }
    
    public async void SendChatMsg(ChatBox.ChatJson chatJson)
    {
        string resJson = JsonUtility.ToJson(chatJson);
        await _webSocket.SendText(NetworkConstant.CHAT + septr + resJson);
    }
    
    [ContextMenu("shoot")]
    public async void ShootCmd()
    {
        if (networkId == "") return;
        
        //ShootJSON res = ShootJSON.CreateFromJson(networkId);
        await _webSocket.SendText(NetworkConstant.METHOD_PLAYERSHOOT + septr + "");
    }
    
    [ContextMenu("move")]
    public async void MoveCmd()
    {
        if (networkId == "") return;
        
        //ShootJSON res = ShootJSON.CreateFromJson(networkId)
        Vector3 newPos = Vector3.down * 5f;
        PositionJSON positionJson = new PositionJSON(newPos, networkId);
        string res = JsonUtility.ToJson(positionJson);
        await _webSocket.SendText(NetworkConstant.METHOD_PLAYERMOVE + septr + res);
    }
    
    public async void MoveCmd(Vector3 _position)
    {
        if (networkId == "") return;
        
        //ShootJSON res = ShootJSON.CreateFromJson(networkId)
        
        PositionJSON positionJson = new PositionJSON(_position, networkId);
        string res = JsonUtility.ToJson(positionJson);
        await _webSocket.SendText(NetworkConstant.METHOD_PLAYERMOVE + septr + res);
    }
    
    public async void RotateCmd(Quaternion _rotation)
    {
        if (networkId == "") return;
        
        //ShootJSON res = ShootJSON.CreateFromJson(networkId)
        
        RotationJSON rotationJson = new RotationJSON(_rotation, networkId);
        string res = JsonUtility.ToJson(rotationJson);
        await _webSocket.SendText(NetworkConstant.METHOD_PLAYERROTATE + septr + res);
    }
    
    [ContextMenu("health change")]
    public async void HealthCmd()
    {
        if (networkId == "") return;
        
        //ShootJSON res = ShootJSON.CreateFromJson(networkId)
        Vector3 newPos = Vector3.down * 5f;
        HealthChangeJson healthChangeJson;
        healthChangeJson = new HealthChangeJson(networkId, 75, "tom", false);
        string res = JsonUtility.ToJson(healthChangeJson);
        await _webSocket.SendText(NetworkConstant.METHOD_HEALTH + septr + res);
        var enemyHealthChange = new HealthChangeJson(networkId, 87, "tom", true, networkId);
        res = JsonUtility.ToJson(enemyHealthChange);
        await _webSocket.SendText(NetworkConstant.METHOD_HEALTH + septr + res);
    }
    
    public async void HealthCmd(HealthChangeJson healthChangeJson)
    {
        if (networkId == "") return;
        
        string res = JsonUtility.ToJson(healthChangeJson);
        await _webSocket.SendText(NetworkConstant.METHOD_HEALTH + septr + res);
    }
    
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (_webSocket != null)
            _webSocket.DispatchMessageQueue();
#endif
    }
    
    private async void OnApplicationQuit()
    {
        await _webSocket.Close();
    }
}
