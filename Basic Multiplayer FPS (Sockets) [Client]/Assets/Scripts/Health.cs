using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public const int maxHealth = 100;
    public bool destroyOnDeath;

    public int currentHealth = maxHealth;

    public bool isEnemy = false;
    public RectTransform healthBar;

    public bool isLocalPlayer;

    [SerializeField] private float ratio;

    private PlayerController _playerController;

    private void Awake()
    {
        ratio = (healthBar.sizeDelta.x / maxHealth);
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        isLocalPlayer = _playerController.isLocalPlayer;
        //currentHealth = maxHealth;
        //healthBar.sizeDelta = new Vector2((ratio * maxHealth), healthBar.sizeDelta.y);
    }

    public void TakeDamage(PlayerController playerFrom, int damageAmount)
    {
        //if they are not local player then do not register damage and avoid sending health change when the bullet from non local player hits
        if (!playerFrom.isLocalPlayer)
            return;
        
        currentHealth -= damageAmount;
        OnHealthChanged();
        // TODO network
        HealthChangeJson healthChangeJson;
        healthChangeJson = new HealthChangeJson(_playerController.ClientId, damageAmount, playerFrom.ClientId, isEnemy);
        Debug.Log("sending health info...");
        NetworkManager.Instance.HealthCmd(healthChangeJson);
    }

    private void OnEnable()
    {
        ratio = (healthBar.sizeDelta.x / maxHealth);
    }

    public void OnHealthChanged()
    {
        
        healthBar.sizeDelta = new Vector2(( ratio * currentHealth), healthBar.sizeDelta.y);
        if (currentHealth <= 0)
        {
            if (destroyOnDeath)
            {
                Debug.Log("destroyed "+gameObject.name);
                //remove the controller obj from player/enemy list
                if (isEnemy)
                {
                    //if the obj is enemy
                    FindObjectOfType<GameManager>().RemoveEnemyFromGame(ref _playerController);
                }
                //Destroy(this.gameObject);
            }
            else
            {
                currentHealth = maxHealth;
                healthBar.sizeDelta = new Vector2((ratio * maxHealth), healthBar.sizeDelta.y);
                _playerController.Respawn();
            }
        }
    }
}