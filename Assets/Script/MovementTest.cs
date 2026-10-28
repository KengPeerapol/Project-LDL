using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class MovementTest : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Tooltip("ติ๊กถูกเพื่อให้หันหัวตามทิศพุ่ง (ติ๊กออกสำหรับศัตรูที่ต้องการให้หมุนควงสว่านเอง)")]
    public bool rotateTowardsDirection = true;

    [Header("Random Angle Settings")]
    public float minAngle = -30f;
    public float maxAngle = 30f;

    [Header("Collision & Bounce Settings")]
    public string wallTag = "Wall";

    [Tooltip("ให้เด้งสะท้อนเมื่อชนกับ Enemy หรือ Item ด้วยกันเองหรือไม่")]
    public bool bounceWithEntities = true;

    private Rigidbody2D rb;
    private Collider2D myCollider;
    private Vector2 lastVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        // ⭐ หากได้รับความเร็วส่งต่อมาจากตัวก่อนหน้าอยู่แล้ว ให้ใช้ความเร็วนั้นต่อไปเลย ห้ามสุ่มทับ
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            lastVelocity = rb.linearVelocity;
        }
        else
        {
            // ถ้าเกิดจาก Spawner ตามปกติ (ยังไม่มีความเร็วเดิม) ให้สุ่มองศาพุ่งใหม่
            float randomAngle = Random.Range(minAngle, maxAngle);
            Vector2 moveDirection = Quaternion.Euler(0f, 0f, randomAngle) * Vector2.left;

            rb.linearVelocity = moveDirection * speed;
            lastVelocity = rb.linearVelocity;
        }

        UpdateRotation();
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
        UpdateRotation();
    }

    // ⭐ ปรับมุมหันหน้าให้ชี้ไปตามทิศทางพุ่งปัจจุบัน
    public void UpdateRotation()
    {
        if (!rotateTowardsDirection) return;

        Vector2 currentVelocity = rb != null ? rb.linearVelocity : lastVelocity;
        if (currentVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleBounce(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleBounce(collision.gameObject);
    }

    private void HandleBounce(GameObject target)
    {
        // 1. เด้งสะท้อนกำแพงบน-ล่าง (Wall)
        if (target.CompareTag(wallTag))
        {
            float normalY = (transform.position.y > target.transform.position.y) ? 1f : -1f;
            Vector2 wallNormal = new Vector2(0f, normalY);

            ReflectVelocity(wallNormal, false);
        }
        // 2. เด้งสะท้อนเมื่อชนกับ Enemy หรือ Item
        else if (bounceWithEntities && (target.CompareTag("Enemy") || target.CompareTag("Item")))
        {
            float diffY = transform.position.y - target.transform.position.y;
            float normalY = (diffY >= 0f) ? 1f : -1f;

            Vector2 entityNormal = new Vector2(0f, normalY);
            ReflectVelocity(entityNormal, true);
        }
    }

    private void ReflectVelocity(Vector2 normal, bool isEntityBounce)
    {
        Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal).normalized;

        if (isEntityBounce)
        {
            // บังคับให้แกน X มุ่งไปทางซ้ายเสมอ ห้ามสะท้อนย้อนไปขวา
            reflectDir.x = -Mathf.Abs(reflectDir.x);

            if (reflectDir.x > -0.4f)
            {
                reflectDir.x = -0.6f;
            }
            reflectDir = reflectDir.normalized;
        }
        else
        {
            reflectDir.x = -Mathf.Abs(reflectDir.x);
            reflectDir = reflectDir.normalized;
        }

        rb.linearVelocity = reflectDir * speed;
        lastVelocity = rb.linearVelocity;

        UpdateRotation();
    }
}