using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject playerFrom;

    private void OnCollisionEnter(Collision collision)
    {
        var hit = collision.gameObject;

        if (hit.TryGetComponent(out Health health))
        {
            health.TakeDamage(playerFrom.GetComponent<PlayerController>(), 10);
        }
        
        Destroy(gameObject);
    }
}
