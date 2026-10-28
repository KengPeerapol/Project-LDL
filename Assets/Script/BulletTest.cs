using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletTest : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("ความเร็วพื้นฐานของกระสุน (จะถูกแทนที่ถ้าส่งค่าความเร็วมาจาก PlayerAttackTest)")]
    public float speed = 12f;
    public float lifeTime = 3f;
    public int damage = 25;

    [Header("Piercing Settings")]
    public bool isPiercing = false;

    [Header("Bounce Settings (การเด้งกำแพง)")]
    public int maxBounces = 0;
    private int currentBounces = 0;

    private Rigidbody2D rb;
    private readonly HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    // ⭐ เพิ่มพารามิเตอร์ customSpeed ให้ปรับแยกความเร็วกระสุนธรรมดาและกระสุนชาร์จได้
    public void Setup(Vector2 direction, int bulletDamage, float sizeMultiplier = 1f, bool pierce = false, int bounces = 0, float customSpeed = -1f)
    {
        damage = bulletDamage;
        isPiercing = pierce;
        maxBounces = bounces;
        currentBounces = 0;
        transform.localScale *= sizeMultiplier;

        if (customSpeed > 0f)
        {
            speed = customSpeed;
        }

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
        else if (collision.CompareTag("Enemy") || collision.transform.root.CompareTag("Enemy"))
        {
            EnemyHealthTest enemyTest = collision.GetComponentInParent<EnemyHealthTest>();

            // หากศัตรู/แต้มตัวนี้เปิด isInvulnerable กระสุนจะบินทะลุผ่านไปทันที
            if (enemyTest != null && enemyTest.isInvulnerable)
            {
                return;
            }

            if (hitTargets.Contains(collision)) return;
            hitTargets.Add(collision);

            if (enemyTest != null)
            {
                enemyTest.TakeDamage(damage);
            }

            if (!isPiercing)
            {
                Destroy(gameObject);
            }
        }
    }

    private void BounceOffWall(Collider2D wallCollider)
    {
        Vector2 hitPoint = wallCollider.ClosestPoint(transform.position);
        Vector2 wallNormal = ((Vector2)transform.position - hitPoint).normalized;

        if (wallNormal == Vector2.zero)
        {
            float normalY = (transform.position.y >= wallCollider.bounds.center.y) ? 1f : -1f;
            wallNormal = new Vector2(0f, normalY);
        }
        else
        {
            if (Mathf.Abs(wallNormal.y) > Mathf.Abs(wallNormal.x))
            {
                wallNormal = new Vector2(0f, Mathf.Sign(wallNormal.y));
            }
            else
            {
                wallNormal = new Vector2(Mathf.Sign(wallNormal.x), 0f);
            }
        }

        Vector2 currentVel = rb.linearVelocity;
        Vector2 reflectDir = Vector2.Reflect(currentVel.normalized, wallNormal).normalized;

        rb.linearVelocity = reflectDir * speed;

        float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}