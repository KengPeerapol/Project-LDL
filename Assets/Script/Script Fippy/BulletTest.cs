using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletTest : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 12f;
    public float lifeTime = 3f;
    public int damage = 25;

    [Header("Piercing Settings")]
    [Tooltip("กระสุนจะทะลุศัตรูได้หรือไม่")]
    public bool isPiercing = false;

    [Header("Bounce Settings (การเด้งกำแพง)")]
    [Tooltip("จำนวนครั้งที่สามารถเด้งสะท้อนกำแพงได้ (0 = ไม่เด้ง)")]
    public int maxBounces = 0; // ⭐ โควตาการเด้งกำแพง
    private int currentBounces = 0;

    private Rigidbody2D rb;
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    // รับค่า direction, damage, scale, piercing และจำนวนครั้งที่เด้งกำแพงได้
    public void Setup(Vector2 direction, int bulletDamage, float sizeMultiplier = 1f, bool pierce = false, int bounces = 0)
    {
        damage = bulletDamage;
        isPiercing = pierce;
        maxBounces = bounces;
        currentBounces = 0;
        transform.localScale *= sizeMultiplier;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ⭐ 1. ชนกำแพง (Wall)
        if (collision.CompareTag("Wall"))
        {
            if (currentBounces < maxBounces)
            {
                currentBounces++;
                BounceOffWall(collision);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        // 2. ชนศัตรู (Enemy)
        else if (collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy"))
        {
            if (hitTargets.Contains(collision)) return;
            hitTargets.Add(collision);

            if (collision.TryGetComponent(out EnemyHealthTest enemyTest))
            {
                enemyTest.TakeDamage(damage);
            }
            else
            {
                EnemyHealthTest rootEnemy = collision.GetComponentInParent<EnemyHealthTest>();
                if (rootEnemy != null)
                {
                    rootEnemy.TakeDamage(damage);
                }
            }

            if (!isPiercing)
            {
                Destroy(gameObject);
            }
        }
    }

    // ⭐ ฟังก์ชันคำนวณการสะท้อนองศาของกระสุนเมื่อชนกำแพง
    private void BounceOffWall(Collider2D wallCollider)
    {
        // 1. หาจุดปะทะและเวกเตอร์ตั้งฉาก (Normal) ของขอบกำแพง
        Vector2 hitPoint = wallCollider.ClosestPoint(transform.position);
        Vector2 wallNormal = ((Vector2)transform.position - hitPoint).normalized;

        if (wallNormal == Vector2.zero)
        {
            float normalY = (transform.position.y >= wallCollider.bounds.center.y) ? 1f : -1f;
            wallNormal = new Vector2(0f, normalY);
        }
        else
        {
            // ตรึงแกนให้แม่นยำ (สะท้อนเพดาน/พื้น หรือกำแพงข้าง)
            if (Mathf.Abs(wallNormal.y) > Mathf.Abs(wallNormal.x))
            {
                wallNormal = new Vector2(0f, Mathf.Sign(wallNormal.y));
            }
            else
            {
                wallNormal = new Vector2(Mathf.Sign(wallNormal.x), 0f);
            }
        }

        // 2. สะท้อนเวกเตอร์ความเร็ว
        Vector2 currentVel = rb.linearVelocity;
        Vector2 reflectDir = Vector2.Reflect(currentVel.normalized, wallNormal).normalized;

        // คืนค่าความเร็ว
        rb.linearVelocity = reflectDir * speed;

        // 3. ปรับมุมหมุนหัวกระสุนให้ตรงตามทิศทางที่สะท้อนออกไป
        float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}