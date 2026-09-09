using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BoomerMovementTest : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Tooltip("ติ๊กถูกเพื่อให้หันหัวตามทิศพุ่ง (ติ๊กออกสำหรับศัตรูที่ต้องการให้หมุนควงสว่านเอง)")]
    public bool rotateTowardsDirection = true; // ⭐ เพิ่มตัวเลือกเปิด-ปิดการล็อกมุม

    [Header("Random Angle Settings")]
    public float minAngle = -30f;
    public float maxAngle = 30f;

    [Header("Collision Settings")]
    public string wallTag = "Wall";

    private Rigidbody2D rb;
    private Vector2 lastVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

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
        // หากปิดการหันตามทิศทางไว้ ให้ข้ามไป เพื่อให้ RotateSelf หมุนตัวได้
        if (!rotateTowardsDirection) return;

        Vector2 currentVelocity = rb.linearVelocity;

        if (currentVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(wallTag))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal).normalized;

            rb.linearVelocity = reflectDir * speed;
            lastVelocity = rb.linearVelocity;
        }
    }
}