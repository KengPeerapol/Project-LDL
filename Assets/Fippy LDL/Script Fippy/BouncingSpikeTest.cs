using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BouncingSpikeTest : MonoBehaviour
{
    [Header("Spike Settings")]
    public int damage = 15;
    public float lifeTime = 5f;
    public int maxBounces = 4;

    [Header("Natural Bounce Physics (ความสมจริง)")]
    [Tooltip("ความยืดหยุ่นของการเด้ง (0.95 = ชะลอความเร็วเล็กน้อยตามแรงปะทะ)")]
    [Range(0.7f, 1.1f)]
    public float bounciness = 0.98f;

    [Tooltip("องศาเบี่ยงเบนแบบสุ่ม (ช่วยให้วิถีไม่เป็นเส้นตรงทื่อๆ เหมือนหุ่นยนต์)")]
    public float bounceRandomSpread = 8f;

    [Tooltip("ความเฉียงขั้นต่ำ (ป้องกันไม่ให้หนามเด้งขึ้นลงแนวดิ่งทื่อๆ อยู่กับที่)")]
    [Range(0.2f, 0.8f)]
    public float minDiagonalSlant = 0.55f;

    [Tooltip("ความเร็วในการหันหัวตามวิถีพุ่ง (ยิ่งน้อย ยิ่งสะบัดท้ายเลี้ยวโค้งสวยงาม)")]
    public float turnSmoothSpeed = 16f;

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

        IgnoreExistingEntities();
        SnapRotation(direction);
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            lastVelocity = rb.linearVelocity;
        }
    }

    private void Update()
    {
        // ⭐ หมุนสะบัดหัวหนามตามทิศทางพุ่งใหม่อย่างนุ่มนวล
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg + rotationOffset;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSmoothSpeed * Time.deltaTime);
        }
    }

    // รองรับ Collider แบบ Solid ทั่วไป
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 normal = (collision.contactCount > 0) ? collision.GetContact(0).normal : Vector2.zero;
            BounceOffWall(normal);
        }
        else
        {
            HandleOtherHit(collision.gameObject);
        }
    }

    // รองรับ Collider ที่เปิด Is Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            Vector2 normal = ((Vector2)transform.position - hitPoint).normalized;

            if (normal == Vector2.zero || Vector2.Dot(lastVelocity, normal) >= 0f)
            {
                normal = (Mathf.Abs(lastVelocity.y) > Mathf.Abs(lastVelocity.x))
                    ? new Vector2(0f, -Mathf.Sign(lastVelocity.y))
                    : new Vector2(-Mathf.Sign(lastVelocity.x), 0f);
            }

            BounceOffWall(normal);
        }
        else
        {
            HandleOtherHit(collision.gameObject);
        }
    }

    private void BounceOffWall(Vector2 normal)
    {
        if (normal == Vector2.zero)
        {
            normal = new Vector2(0f, -Mathf.Sign(lastVelocity.y));
        }

        // 1. คำนวณมุมสะท้อน
        Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal).normalized;

        // 2. สุ่มมุม Ricochet เล็กน้อย
        float randomAngle = Random.Range(-bounceRandomSpread, bounceRandomSpread);
        reflectDir = Quaternion.Euler(0f, 0f, randomAngle) * reflectDir;

        // 3. บังคับมุมเฉียงเมื่อชนขอบกำแพงบน-ล่าง ป้องกันการเด้งขึ้นลงในแนวดิ่ง
        if (Mathf.Abs(normal.y) > 0.5f)
        {
            float signX = Mathf.Sign(reflectDir.x);
            if (signX == 0) signX = -1f;

            if (Mathf.Abs(reflectDir.x) < minDiagonalSlant)
            {
                reflectDir.x = signX * minDiagonalSlant;
            }
            reflectDir = reflectDir.normalized;
        }

        // 4. คำนวณความเร็วหลังชน
        currentSpeed *= bounciness;
        rb.linearVelocity = reflectDir * currentSpeed;
        lastVelocity = rb.linearVelocity;

        // 5. ดันตัวออกจากระนาบกำแพงเล็กน้อย ป้องกันการติดขอบ
        transform.position += (Vector3)(normal * 0.08f);

        bounceCount++;
        if (maxBounces > 0 && bounceCount >= maxBounces)
        {
            Destroy(gameObject);
        }
    }

    private void HandleOtherHit(GameObject target)
    {
        // ทะลุหนามด้วยกันเอง, ศัตรู และไอเทม
        if (target.GetComponent<BouncingSpikeTest>() != null ||
            target.GetComponent<BouncingSpike>() != null ||
            target.CompareTag("Enemy") ||
            target.CompareTag("Item"))
        {
            return;
        }

        // ชนโดน Player
        if (target.CompareTag("Player"))
        {
            if (hasHitPlayer) return;
            hasHitPlayer = true;

            // ปิด Collider ทันที ไม่ให้เกิดแรงกระแทกฟิสิกส์ Player จะได้ไม่ชะงัก
            if (myCollider != null) myCollider.enabled = false;

            if (target.TryGetComponent(out PlayerHealthTest playerTest))
            {
                playerTest.TakeDamage(damage);
            }
            else if (target.TryGetComponent(out PlayerHealth player))
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }

    private void IgnoreExistingEntities()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (enemyCollider != null && myCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, enemyCollider, true);
            }
        }
    }

    private void SnapRotation(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }
    }
}