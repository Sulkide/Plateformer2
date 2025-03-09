using UnityEngine;

public class Enemy : MonoBehaviour
{
   
    public float recoilForce = 10f;
   
    public float destructionDelay = 0.2f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile")  || collision.gameObject.layer == LayerMask.NameToLayer("ProjectileCollision"))
        {
            rb.freezeRotation = false;
            
            gameObject.layer = LayerMask.NameToLayer("ProjectileCollision");
            
            Vector2 recoilDirection = (transform.position - collision.transform.position).normalized;
      
            rb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);
            
            Destroy(gameObject, destructionDelay);
        }
    }
}