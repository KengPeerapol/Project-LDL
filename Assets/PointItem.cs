using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PointItem : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("จำนวนคะแนนที่ได้รับเมื่อเก็บ")]
    public int scoreValue = 20;

    [Header("Lifetime Settings")]
    [Tooltip("ระยะเวลาก่อนที่แต้มจะหายไปเองหากไม่เก็บ (วินาที)")]
    public float lifeTime = 8f;

    [Header("Movement Settings")]
    [Tooltip("ความเร็วและทิศทางการเคลื่อนที่")]
    public Vector2 moveVelocity = Vector2.left * 4f;

    [Tooltip("ให้เด้งสะท้อนเมื่อชนเพดาน/พื้นกำแพงหรือไม่")]
    public bool bounceOnWall = true;
    public string wallTag = "Wall";

    [Header("Effects")]
    public GameObject collectEffect;

    private Rigidbody2D rb;
    private bool isCollected = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Start()
    {
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }

        ApplyVelocity();
    }

    private void FixedUpdate()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            moveVelocity = rb.linearVelocity;
        }
    }

    public void Setup(int score, Vector2 initialVelocity)
    {
        scoreValue = score;

        if (initialVelocity.sqrMagnitude > 0.05f)
        {
            moveVelocity = initialVelocity;
            moveVelocity.x = -Mathf.Abs(moveVelocity.x); // มุ่งหน้าไปทางซ้ายเสมอ
        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveVelocity;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject target)
    {
        if (isCollected) return;

        // ⭐ 1. ชนกำแพง (Wall) -> สะท้อนกลับเข้ามาในจอทันที
        if (bounceOnWall && target.CompareTag(wallTag))
        {
            float normalY = (transform.position.y > target.transform.position.y) ? 1f : -1f;
            Vector2 wallNormal = new Vector2(0f, normalY);

            // คำนวณแรงสะท้อน
            Vector2 reflectDir = Vector2.Reflect(moveVelocity.normalized, wallNormal).normalized;
            reflectDir.x = -Mathf.Abs(reflectDir.x); // บังคับให้พุ่งไปซ้ายต่อ

            // หากไม่มีแรงสะท้อนแกน Y ให้ใส่แรงส่งแกน Y ขั้นต่ำ
            if (Mathf.Abs(reflectDir.y) < 0.2f)
            {
                reflectDir.y = normalY * 0.5f;
                reflectDir = reflectDir.normalized;
            }

            float currentSpeed = Mathf.Max(moveVelocity.magnitude, 3.5f);
            moveVelocity = reflectDir * currentSpeed;
            ApplyVelocity();
            return;
        }

        // ⭐ 2. ชน Player -> รับแต้มคะแนน
        if (target.CompareTag("Player") || target.transform.root.CompareTag("Player"))
        {
            isCollected = true;

            if (GameScoreManager.Instance != null)
            {
                GameScoreManager.Instance.AddScore(scoreValue);
            }

            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}