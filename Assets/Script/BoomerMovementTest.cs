using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BoomerMovementTest : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Tooltip("ติ๊กถูกเพื่อให้หันหัวตามทิศพุ่ง (ติ๊กออกสำหรับศัตรูที่ต้องการให้หมุนควงสว่านเอง)")]
    public bool rotateTowardsDirection = false; // สำหรับ Boomer ตั้งค่าเริ่มต้นเป็น false เพื่อให้หมุนตัวได้

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
        float randomAngle = Random.Range(minAngle, maxAngle);
        Vector2 moveDirection = Quaternion.Euler(0f, 0f, randomAngle) * Vector2.left;

        rb.linearVelocity = moveDirection * speed;
        lastVelocity = rb.linearVelocity;
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
        if (!rotateTowardsDirection) return;

        Vector2 currentVelocity = rb.linearVelocity;
        if (currentVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // รองรับกรณีเปิด Is Trigger (ไม่ขัดจังหวะ Player และยังเด้งกำแพงได้)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleBounce(collision.gameObject);
    }

    // รองรับกรณีไม่ได้เปิด Is Trigger
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleBounce(collision.gameObject);
    }

    private void HandleBounce(GameObject target)
    {
        // 1. เด้งสะท้อนขอบกำแพงบน-ล่าง
        if (target.CompareTag(wallTag))
        {
            float normalY = (transform.position.y > target.transform.position.y) ? 1f : -1f;
            Vector2 wallNormal = new Vector2(0f, normalY);

            ReflectVelocity(wallNormal, false);
        }
        // 2. เด้งเมื่อชนกับ Enemy หรือ Item (เบี่ยงขึ้น-ลง ห้ามย้อนไปทางขวา)
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
            // บังคับให้พุ่งไปทางซ้ายเสมอ ห้ามย้อนกลับไปทางขวา
            reflectDir.x = -Mathf.Abs(reflectDir.x);

            // ให้มีแรงส่งไปข้างหน้าขั้นต่ำ ป้องกันการลอยขึ้นลงอยู่กับที่
            if (reflectDir.x > -0.4f)
            {
                reflectDir.x = -0.6f;
            }
            reflectDir = reflectDir.normalized;
        }
        else
        {
            // ชนกำแพง: บังคับให้ทิศทางยังคงมุ่งหน้าไปทางซ้าย
            reflectDir.x = -Mathf.Abs(reflectDir.x);
            reflectDir = reflectDir.normalized;
        }

        rb.linearVelocity = reflectDir * speed;
        lastVelocity = rb.linearVelocity;

        if (rotateTowardsDirection)
        {
            float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}