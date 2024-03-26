using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerJSON
{
    public string name;
    public List<PointJSON> playerSpawnPoints;
    public List<PointJSON> enemySpawnPoints;

    public PlayerJSON(string name, List<SpawnPoint> _playerSpawnPoints, List<SpawnPoint> _enemySpawnPoints)
    {
        playerSpawnPoints = new List<PointJSON>();
        enemySpawnPoints = new List<PointJSON>();
        foreach (SpawnPoint playerPoint in _playerSpawnPoints)
        {
            PointJSON pointJson = new PointJSON(playerPoint);
            playerSpawnPoints.Add(pointJson);
        }
        foreach (SpawnPoint enemyPoint in _enemySpawnPoints)
        {
            PointJSON pointJson = new PointJSON(enemyPoint);
            enemySpawnPoints.Add(pointJson);
        }

        this.name = name;
    }
    
}

[Serializable]
public class GameIdJson
{
    [SerializeField] public string gameId;
    public GameIdJson(string id)
    {
        gameId = id;
    }
}

[Serializable]
public class PointJSON
{
    public float[] position; 
    public float[] rotation;
    public PointJSON(SpawnPoint spawnPoint)
    {
        position = new float[]
        {
            spawnPoint.transform.position.x,
            spawnPoint.transform.position.y,
            spawnPoint.transform.position.z
        };
        rotation = new float[]
        {
            spawnPoint.transform.rotation.eulerAngles.x,
            spawnPoint.transform.rotation.eulerAngles.y,
            spawnPoint.transform.rotation.eulerAngles.z,
        };
    }
}

[Serializable]
public class PositionJSON
{
    //for just sending if only position data si changed
    [SerializeField]public float[] position;
    [SerializeField]public string clientId;
    [SerializeField]public string name;
    
    public PositionJSON(Vector3 spawnPoint, string _clientId, string _name = "")
    {
        position = new float[]
        {
            spawnPoint.x,
            spawnPoint.y,
            spawnPoint.z
        };
        clientId = _clientId;
        name = _name;
    }
}

[Serializable]
public class UserJSON
{
    //when new user has joined can be used to tell current position,name and health
    [SerializeField]public float[] position;
    [SerializeField]public float[] rotation;
    [SerializeField]public string name;
    [SerializeField]public int health;
    [SerializeField]public string clientId;
    
    public static UserJSON CreateFromJson(string data)
    {
        return JsonUtility.FromJson<UserJSON>(data);
    }
    
}

public class HealthChangeJson
{
    public string name;
    public int healthChange;
    public string from;
    public bool isEnemy;
    public string userId;
    
    public HealthChangeJson(string _userId, int healthChange, string _from, bool _isEnemy, string name = "")
    {
        this.userId = _userId;
        this.name = name;
        this.healthChange = healthChange;
        this.from = _from;
        this.isEnemy = _isEnemy;
    }
}

[Serializable]
public class EnemiesJSON
{
    //send enemies info to spawn for player clients
    [SerializeField]
    public List<UserJSON> enemies;

    public static EnemiesJSON CreateFromJson(string data)
    {
        return JsonUtility.FromJson<EnemiesJSON>(data);
    }
}
[Serializable]
public class ShootJSON
{
    //send which player shot to the server 
    public string name;

    public static ShootJSON CreateFromJson(string data)
    {
        return JsonUtility.FromJson<ShootJSON>(data);
    }
}
[Serializable]
public class UserHealthJSON
{
    //send user health to the server to show other clients 
    [SerializeField]public string clientId;
    [SerializeField]public int health;
    [SerializeField]public string name = "";
    
    public static UserHealthJSON CreateFromJson(string data)
    {
        return JsonUtility.FromJson<UserHealthJSON>(data);
    }
}

