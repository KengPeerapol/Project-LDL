using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public int damage = 10; // กำหนดความแรงของกระสุน (ยิง 3 นัดศัตรูตาย)

    private Rigidbody2D rb;

    public void Setup(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ถ้าชนกำแพง ให้ทำลายกระสุน
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        // ถ้าชนศัตรู ให้ลดเลือดศัตรูแล้วทำลายกระสุน
        else if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // ส่งความแรงกระสุนไปตัดเลือดศัตรู
            }

            Destroy(gameObject); // ทำลายกระสุน
        }
    }
}