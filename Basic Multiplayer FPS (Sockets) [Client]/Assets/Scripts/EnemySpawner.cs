using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public SpawnPoint spawnPoint;
    public int noOfEnemies;
    [HideInInspector] public List<SpawnPoint> enemySpawnPoints;
    [SerializeField] private List<PlayerController> enemyObjects = new List<PlayerController>();
    
    private void Awake()
    {
        CreateSpawnPointsList();
    }

    private void Start()
    {
        
        //SpawnEnemies();
    }
    
    public void SpawnEnemies(EnemiesJSON enemiesJson)
    {
        for (int k = 0; k < enemiesJson.enemies.Count; k++)
        {
            Vector3 spawnPosition = new Vector3(enemiesJson.enemies[k].position[0],
                                                enemiesJson.enemies[k].position[1],
                                                enemiesJson.enemies[k].position[2]);
            Quaternion spawnRotation = Quaternion.Euler(enemiesJson.enemies[k].rotation[0],
                                                        enemiesJson.enemies[k].position[1],
                                                        enemiesJson.enemies[k].position[2]);
            
            GameObject newEnemy = Instantiate(enemy, spawnPosition, spawnRotation);
            //newEnemy.name = " Enemy - " + k;
            newEnemy.name = enemiesJson.enemies[k].name;
            int health = enemiesJson.enemies[k].health;

            newEnemy.GetComponent<PlayerController>().InitEnemy(newEnemy.name, health);
            enemyObjects.Add(newEnemy.GetComponent<PlayerController>());
        }
    }

    public void CheckAndUpdateEnemyHealth(UserHealthJSON healthJson)
    {
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            if (healthJson.clientId == enemyObjects[i].name)
            {
                string _name = enemyObjects[i].gameObject.name;
                enemyObjects[i].GetComponent<Health>().currentHealth = healthJson.health;
                enemyObjects[i].GetComponent<Health>().OnHealthChanged();
                break;
            }
        }
    }

    public void RemoveEnemyFromList(ref PlayerController enemyToRemove)
    {
        Debug.Log("Removing enemy -" + enemyToRemove.gameObject.name);
        enemyObjects.Remove(enemyToRemove);
        Destroy(enemyToRemove.gameObject);
    }
    
    private void CreateSpawnPointsList()
    {
        for (int i = 0; i < noOfEnemies; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-8f, 8f), 0, Random.Range(-8f, 8f));
            Quaternion rot = Quaternion.Euler(0, Random.Range(0, 180), 0);

            SpawnPoint newEnemySpawnPoint = Instantiate(spawnPoint, pos, rot);
            enemySpawnPoints.Add(newEnemySpawnPoint);
        }
    }

    
}