using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerTest : MonoBehaviour
{
    [Header("ตั้งค่าการบิน (คลิกซ้ายค้าง หรือ Spacebar)")]
    public float flyForce = 5f;

    [Header("ตั้งค่าการเอียงตัว")]
    public float baseRotationZ = -90f;
    public float tiltUpwardOffset = 20f;
    public float tiltDownwardOffset = -20f;
    public float rotationSpeed = 15f;

    [Header("ตั้งค่าการชนกำแพง (Wall Collision)")]
    public int wallDamage = 10;            // ดาเมจเมื่อชนโดนกำแพง
    public float wallBounceForce = 6f;     // แรงเด้งผลักให้ออกจากกำแพง
    public float wallStunDuration = 0.15f; // หน่วงเวลาชั่วขณะเพื่อให้แรงเด้งทำงานก่อนบินต่อ

    private Rigidbody2D rb;
    private bool canControl = true;
    private bool isWinning = false;
    private bool isDead = false;
    private bool isFlapping = false;

    private bool IsActive => !isWinning && !isDead;
    private bool CanFly => IsActive && canControl;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // ล็อกไม่ให้หมุนเคว้ง และล็อกแกน X เพื่อไม่ให้ตัวละครเลื่อนไปข้างหลัง
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    private void Update()
    {
        bool mousePress = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool spacePress = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        isFlapping = mousePress || spacePress;

        if (IsActive)
        {
            HandleRotationSmooth();
        }
    }

    private void FixedUpdate()
    {
        if (isFlapping && CanFly)
        {
            FlyUp();
        }
    }

    private void FlyUp()
    {
        rb.linearVelocity = new Vector2(0f, flyForce);
    }

    // ระบบเด้งและลดเลือดเมื่อชนกำแพง
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            // 1. ลดเลือดผู้เล่น
            if (TryGetComponent(out PlayerHealthTest healthTest))
            {
                healthTest.TakeDamage(wallDamage);
            }
            else if (TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(wallDamage);
            }

            // 2. คำนวณทิศทางแรงผลักให้ออกจากกำแพง
            if (collision.contactCount > 0)
            {
                Vector2 normal = collision.contacts[0].normal;

                // ถ้าชนเพดาน (normal ชี้ลง) จะเด้งลง, ถ้าชนพื้น (normal ชี้ขึ้น) จะเด้งขึ้น
                rb.linearVelocity = new Vector2(0f, normal.y * wallBounceForce);

                // สตันสั้นๆ เพื่อให้ผู้เล่นไม่สามารถกดบินค้างทับแรงเด้งทันที
                ApplyStun(wallStunDuration);
            }
        }
    }

    public void ApplyStun(float duration)
    {
        if (isDead) return;
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        canControl = false;
        yield return new WaitForSeconds(duration);
        canControl = true;
    }

    private void HandleRotationSmooth()
    {
        float targetAngle;

        if (rb.linearVelocity.y > 0.1f) targetAngle = baseRotationZ + tiltUpwardOffset;
        else if (rb.linearVelocity.y < -0.1f) targetAngle = baseRotationZ + tiltDownwardOffset;
        else return;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void TriggerWinDash()
    {
        isWinning = true;
        canControl = false;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);
        rb.linearVelocity = new Vector2(5f, 0f);
    }

    public void TriggerDeath()
    {
        isDead = true;
        canControl = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }
}