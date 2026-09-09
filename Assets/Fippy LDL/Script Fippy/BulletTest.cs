using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletTest : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public float lifeTime = 3f;
    public int damage = 25;

    private Rigidbody2D rb;

    // เพิ่มการรับค่าดาเมจและขนาดของกระสุน
    public void Setup(Vector2 direction, int bulletDamage, float sizeMultiplier = 1f)
    {
        damage = bulletDamage;
        transform.localScale *= sizeMultiplier; // ปรับขนาดของกระสุน

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent(out EnemyHealthTest enemyTest))
            {
                enemyTest.TakeDamage(damage);
            }
            else if (collision.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}