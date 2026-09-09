using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BouncingSpikeTest : MonoBehaviour
{
    [Header("Spike Settings")]
    public int damage = 15;
    public float lifeTime = 5f;
    public int maxBounces = 4;

    [Header("Rotation Settings")]
    [Tooltip("ปรับองศาชดเชยให้ปลายแหลมหันไปข้างหน้า (เช่น -45, 0, 45, 90, 180)")]
    public float rotationOffset = -45f;

    private Rigidbody2D rb;
    private Collider2D myCollider;
    private float currentSpeed;
    private int bounceCount = 0;
    private Vector2 lastVelocity;
    private bool hasHitPlayer = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Setup(Vector2 direction, float speed)
    {
        currentSpeed = speed;
        rb.linearVelocity = direction.normalized * currentSpeed;
        lastVelocity = rb.linearVelocity;

        // ยกเว้นการชนกับศัตรูที่มีอยู่ในฉากทั้งหมด
        IgnoreExistingEnemies();

        UpdateRotation(direction);
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        // บันทึกความเร็วล่าสุดก่อนการชน เพื่อใช้คำนวณเวกเตอร์สะท้อน
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            lastVelocity = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. ทะลุหนามด้วยกันเอง (ไม่เด้งชนกัน)
        if (collision.gameObject.GetComponent<BouncingSpike>() != null)
        {
            Physics2D.IgnoreCollision(collision.otherCollider, collision.collider, true);
            rb.linearVelocity = lastVelocity;
            return;
        }

        // 2. ทะลุตัวศัตรู
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(collision.otherCollider, collision.collider, true);
            rb.linearVelocity = lastVelocity;
            return;
        }

        // 3. เด้งสะท้อนกำแพง (คำนวณมุมตกกระทบ = มุมสะท้อน)
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (collision.contactCount > 0)
            {
                Vector2 normal = collision.contacts[0].normal;
                Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal).normalized;

                // กำหนดความเร็วใหม่ให้พุ่งไปทิศสะท้อนด้วยความเร็วคงที่
                rb.linearVelocity = reflectDir * currentSpeed;
                lastVelocity = rb.linearVelocity;

                UpdateRotation(reflectDir);

                bounceCount++;
                if (maxBounces > 0 && bounceCount >= maxBounces)
                {
                    Destroy(gameObject);
                }
            }
        }
        // 4. ชนโดนผู้เล่น
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (hasHitPlayer) return;
            hasHitPlayer = true;

            // รองรับทั้ง PlayerHealthTest และ PlayerHealth เดิม
            if (collision.gameObject.TryGetComponent(out PlayerHealthTest playerTest))
            {
                playerTest.TakeDamage(damage);
            }
            else if (collision.gameObject.TryGetComponent(out PlayerHealth player))
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }

    private void IgnoreExistingEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, enemyCollider, true);
            }
        }
    }

    private void UpdateRotation(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }
    }
}