using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Bullet bulletPrefab;
    public Transform bulletSpawn;
    public bool isLocalPlayer;
    
    private Vector3 oldPosition, currentPosition;
    private Quaternion oldRotaion, currentRotaion;

    private Transform thisTransform;
    [SerializeField] private string clientId;
    public string ClientId
    {
        get { return clientId; }
    }
    
    [SerializeField] private PlayerUI PlayerUI;
    
    // Start is called before the first frame update
    void Start()
    {
        oldPosition = transform.position;
        currentPosition = oldPosition;
        currentRotaion = transform.rotation;
        oldRotaion = currentRotaion;
        thisTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        
        MovePlayer();
        currentPosition = thisTransform.position;
        currentRotaion = thisTransform.rotation;

        if (currentPosition != oldPosition)
        {
            oldPosition = currentPosition;
            //send new position
            NetworkManager.Instance.MoveCmd(currentPosition);
        }

        if (currentRotaion != oldRotaion)
        {
            oldRotaion = currentRotaion;
            //send new rotation
            NetworkManager.Instance.RotateCmd(currentRotaion);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !ChatBox.IsClientChatting)
        {
            FireBullet();
        }
    }

    public void SetCamera()
    {
        if (isLocalPlayer)
        {
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.position = Vector3.zero;
            Camera.main.transform.position = Vector3.up * 2;
        }
    }
    
    private void FireBullet()
    {
        NetworkManager.Instance.ShootCmd();
        Bullet bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        bullet.playerFrom = this.gameObject;
        bullet.transform.forward = bulletSpawn.transform.forward;
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.transform.up * 8f, ForceMode.VelocityChange);
        //bullet.GetComponent<Rigidbody>().velocity = ;
        
        Destroy(bullet.gameObject, 2.0f);
    }
    
    private void MovePlayer()
    {
        if (ChatBox.IsClientChatting)
        {
            return;
        }
        float x = Input.GetAxis("Horizontal") * Time.deltaTime * 150f;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * 3f;

        thisTransform.Rotate(0, x, 0);
        thisTransform.Translate(0, 0, z);
    }

    public void FireBulletByForeignPlayer()
    {
        Bullet bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        bullet.playerFrom = this.gameObject;
        bullet.transform.forward = bulletSpawn.transform.forward;
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.transform.up * 8f, ForceMode.VelocityChange);
        //bullet.GetComponent<Rigidbody>().velocity = ;
        
        Destroy(bullet.gameObject, 2.0f);
        Debug.Log("FireBulletByForeignPlayer exec by" + gameObject.name);
    }
    
    public void MoveNetworkedPlayer(Vector3 position)
    {
        thisTransform.position = position;
    }

    public void RotateNetworkedPlayer(Quaternion rotation)
    {
        thisTransform.rotation = rotation;
    }
    
    public void Respawn()
    {
        if (isLocalPlayer)
        {
            Vector3 spawnPoint = Vector3.zero;
            //thisTransform.rotation = Quaternion.Euler(0, 180, 0);
            thisTransform.rotation = Quaternion.Euler(0, 0, 0);
            thisTransform.position = spawnPoint;
        }
    }

    public void InitPlayerInfo(string _name, string _clientId, bool isLocal, int _health = 100)
    {
        PlayerUI.SetName(_name);
        isLocalPlayer = isLocal;
        clientId = _clientId;
        if (isLocal) gameObject.name = "Player - " + _name;
        else gameObject.name = "Player - " + _name + "(foreign)";
        
        gameObject.SetActive(true);
        //SetCamera();
        if (TryGetComponent(out Health health))
        {
            health.currentHealth = _health;
            health.OnHealthChanged();
            health.isEnemy = false;
        }
    }
    
    public void InitEnemy(string name, int _health = 100)
    {
        isLocalPlayer = false;
        clientId = name;
        PlayerUI.SetName(name);
        if (TryGetComponent(out Health health))
        {
            health.currentHealth = _health;
            health.OnHealthChanged();
            health.isEnemy = true;
            health.isLocalPlayer = false;
        }
    }
}
