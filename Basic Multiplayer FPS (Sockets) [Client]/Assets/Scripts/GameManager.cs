using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerSpawner _PlayerSpawner;
    [SerializeField] private EnemySpawner _enemySpawner;

    [SerializeField] private PlayerController playerPrefab;

    [SerializeField] private List<PlayerController> activePlayers = new List<PlayerController>();
    [SerializeField] private List<PlayerController> enemiesNetworkObjects = new List<PlayerController>();
    
    private void OnEnable()
    {
        StartCoroutine(SubNetworkEvents());
    }

    public IEnumerator SubNetworkEvents()
    {
        yield return new WaitForEndOfFrame();
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnSpawnEnemies += OnSpawnEnemies;
            NetworkManager.Instance.OnNewPlayer += OnNewPlayer;
            NetworkManager.Instance.OnForeignPlayer += OnNewPlayer;
            NetworkManager.Instance.OnPlayerMove += OnPlayerMove;
            NetworkManager.Instance.OnPlayerShoot += OnPlayerShoot;
            NetworkManager.Instance.OnHealthChange += OnHealthChange;
            NetworkManager.Instance.OnPlayerRotate += OnPlayerRotate;
            NetworkManager.Instance.OnPlayerLeft += OnPlayerLeft;
        }
    }

    private void OnHealthChange(UserHealthJSON obj)
    {
        foreach (PlayerController pc in activePlayers)
        {
            if (pc.ClientId == obj.clientId)
            {
                pc.GetComponent<Health>().currentHealth = obj.health;
                pc.GetComponent<Health>().OnHealthChanged();
                return;
            }
        }
        _enemySpawner.CheckAndUpdateEnemyHealth(obj);
    }

    private void OnDisable()
    {
        NetworkManager.Instance.OnSpawnEnemies -= OnSpawnEnemies;
        NetworkManager.Instance.OnNewPlayer -= OnNewPlayer;
        NetworkManager.Instance.OnForeignPlayer -= OnNewPlayer;
        NetworkManager.Instance.OnPlayerMove -= OnPlayerMove;
        NetworkManager.Instance.OnPlayerShoot -= OnPlayerShoot;
        NetworkManager.Instance.OnHealthChange -= OnHealthChange;
        NetworkManager.Instance.OnPlayerRotate -= OnPlayerRotate;
        NetworkManager.Instance.OnPlayerLeft -= OnPlayerLeft;
    }

    private void OnSpawnEnemies(EnemiesJSON enemiesJson)
    {
        _enemySpawner.SpawnEnemies(enemiesJson);
    }

    
    private void OnNewPlayer(UserJSON obj)
    {
        Debug.Log("VAR");
        Vector3 pos = new Vector3(obj.position[0], obj.position[1], obj.position[2]);
        Quaternion rot = Quaternion.Euler(obj.rotation[0], obj.rotation[1], obj.rotation[2]);
        string playerName = obj.name;
        bool isLocal = NetworkManager.Instance.NetworkId == obj.clientId;

        PlayerController pc = Instantiate(playerPrefab, pos, rot);
        
        activePlayers.Add(pc);
        pc.isLocalPlayer = isLocal;
        pc.InitPlayerInfo(playerName, obj.clientId, isLocal, obj.health);
        
    }

    private void OnPlayerMove(PositionJSON json)
    {
        foreach (PlayerController player in activePlayers)
        {
            if (json.clientId == player.ClientId)
            {
                Vector3 position = new Vector3(json.position[0], json.position[1], json.position[2]);
                player.MoveNetworkedPlayer(position);
                break;
            }
        }
    }

    private void OnPlayerRotate(RotationJSON json)
    {
        foreach (PlayerController player in activePlayers)
        {
            if (json.clientId == player.ClientId)
            {
                Quaternion rot = Quaternion.Euler(json.rotation[0], json.rotation[1], json.rotation[2]);
                player.RotateNetworkedPlayer(rot);
                break;
            }
        }
    }
    
    private void OnPlayerShoot(string playerId)
    {
        foreach (PlayerController player in activePlayers)
        {
            Debug.Log(player.ClientId +" == " + playerId + " "+(playerId == player.ClientId));
            if (playerId == player.ClientId)
            {
                Debug.Log("shot by foreign player=" + player.gameObject.name);
                player.FireBulletByForeignPlayer();
                break;
            }
        }
    }

    private void OnPlayerLeft(string playerId)
    {
        int index = 0;
        foreach (PlayerController player in activePlayers)
        {
            if (playerId == player.ClientId)
            {
                activePlayers.Remove(player);
                Destroy(player.gameObject);
                break;
            }
            index++;
        }
    }

    public void RemoveEnemyFromGame(ref PlayerController enemyToRemove)
    {
        _enemySpawner.RemoveEnemyFromList(ref enemyToRemove);
    }
}