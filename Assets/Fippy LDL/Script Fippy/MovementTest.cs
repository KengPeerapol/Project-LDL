using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class MovementTest : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Random Angle Settings")]
    public float minAngle = -30f;
    public float maxAngle = 30f;

    [Header("Collision Settings")]
    public string wallTag = "Wall"; // กำหนด Tag กำแพงที่จะให้สะท้อน

    private Rigidbody2D rb;
    private Vector2 lastVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // ปิดแรงโน้มถ่วงและล็อกไม่ให้ฟิสิกส์หมุนตัววัตถุเอง
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        // 1. สุ่มมุมเอียงขึ้น-ลง[cite: 12]
        float randomAngle = Random.Range(minAngle, maxAngle);

        // 2. คำนวณทิศทางพุ่งไปทางซ้ายตามมุมที่สุ่มได้[cite: 12]
        Vector2 moveDirection = Quaternion.Euler(0f, 0f, randomAngle) * Vector2.left;

        // 3. กำหนดความเร็วเริ่มต้น[cite: 12]
        rb.linearVelocity = moveDirection * speed;
        lastVelocity = rb.linearVelocity;
    }

    private void FixedUpdate()
    {
        // บันทึกความเร็วก่อนเกิดการชนในแต่ละเฟรม
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            lastVelocity = rb.linearVelocity;
        }
    }

    private void Update()
    {
        Vector2 currentVelocity = rb.linearVelocity;

        // หันหัวไปตามทิศทางความเร็วจริงตลอดเวลา (ทั้งตอนพุ่งและตอนเด้ง)[cite: 12]
        if (currentVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // เมื่อชนกับวัตถุที่มี Tag กำแพง
        if (collision.gameObject.CompareTag(wallTag))
        {
            // คำนวณมุมสะท้อนจากระนาบพื้นผิวที่ชน
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal).normalized;

            // คืนค่าความเร็วให้คงที่ตามทิศทางสะท้อนใหม่ทันที
            rb.linearVelocity = reflectDir * speed;
            lastVelocity = rb.linearVelocity;
        }
    }
}