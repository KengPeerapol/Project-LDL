using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerTest : MonoBehaviour
{
    [Header("ตั้งค่า Intro Dash (พุ่งเข้าจอตอนเริ่ม)")]
    [Tooltip("เปิดใช้งานระบบพุ่งเข้าจอตอนเริ่มเกมหรือไม่")]
    public bool playIntroDash = true;

    [Tooltip("ระยะที่ตัวละครจะเกิดนอกจอทางซ้าย (วัดจากจุดที่วางใน Scene)")]
    public float introSpawnOffsetX = 8f;

    [Tooltip("ความเร็วในการพุ่งเข้ามาในจอ")]
    public float introDashSpeed = 10f;

    [Header("ตั้งค่าการบิน: กดค้าง (Hold Only)")]
    [Tooltip("ความเร่งในการพุ่งขึ้นตอนกดค้าง (ยิ่งเยอะยิ่งลอยขึ้นเร็ว)")]
    public float holdAcceleration = 18f;

    [Tooltip("เพดานความเร็วลอยขึ้นสูงสุด")]
    public float maxAscentSpeed = 5.5f;

    [Header("ตั้งค่าการหล่น / แรงโน้มถ่วง")]
    [Tooltip("ค่าแรงโน้มถ่วงตอนปล่อยมือ (ค่ายิ่งน้อย ยิ่งตกช้าลง เช่น 0.5 - 0.7)")]
    public float customGravityScale = 0.6f;

    [Tooltip("จำกัดความเร็วตกสูงสุด ไม่ให้ตกเร็วเกินไป")]
    public float maxFallSpeed = 4f;

    [Header("ตั้งค่าการเอียงตัว")]
    public float baseRotationZ = -90f;
    public float tiltUpwardOffset = 20f;
    public float tiltDownwardOffset = -20f;
    public float rotationSpeed = 15f;

    [Header("ตั้งค่าการชนกำแพง (Wall Collision)")]
    public int wallDamage = 10;
    public float wallBounceForce = 7f;
    public float wallStunDuration = 0.15f;
    public float damageCooldown = 0.5f;

    [Header("ตั้งค่าการพุ่งชนะ (Win Dash)")]
    [Tooltip("ความเร็วในการพุ่งหลุดขอบจอตอนชนะ (หน่วย/วินาที)")]
    public float winDashSpeed = 14f;

    private Rigidbody2D rb;
    private AudioManger audioManager;

    private bool canControl = false;
    private bool isIntroPlaying = false;
    private bool isWinning = false;
    private bool isDead = false;

    // ตัวแปรจับสถานะการกดค้าง
    private bool isHoldingInput = false;
    private float lastDamageTime = -999f;

    // ⭐ Property สำหรับให้สคริปต์อื่นเช็กสถานะตัวละคร
    public bool CanShootAndControl => canControl && !isIntroPlaying && !isWinning && !isDead;
    private bool IsActive => !isWinning && !isDead && !isIntroPlaying;
    private bool CanFly => IsActive && canControl;

    private void Awake()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManger>();
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playIntroDash)
        {
            StartCoroutine(IntroDashRoutine());
        }
        else
        {
            SetupNormalPlay();
        }
    }

    private IEnumerator IntroDashRoutine()
    {
        isIntroPlaying = true;
        canControl = false;

        Vector3 targetDestination = transform.position;
        transform.position = targetDestination + new Vector3(-introSpawnOffsetX, 0f, 0f);

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);

        while (Vector3.Distance(transform.position, targetDestination) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetDestination, introDashSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetDestination;
        SetupNormalPlay();
        isIntroPlaying = false;

        Debug.Log("<color=green>[Player] เข้าประจำตำแหน่งเรียบร้อย เริ่มเกมได้!</color>");
    }

    private void SetupNormalPlay()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        rb.gravityScale = customGravityScale;
        canControl = true;
    }

    private void Update()
    {
        if (isWinning || isDead || isIntroPlaying) return;

        // ⭐ ตรวจจับเฉพาะการกดค้าง (คลิกขวา หรือ Spacebar)
        bool mouseHold = Mouse.current != null && Mouse.current.rightButton.isPressed;
        bool spaceHold = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        isHoldingInput = mouseHold || spaceHold;

        if (IsActive)
        {
            HandleRotationSmooth();
        }
    }

    private void FixedUpdate()
    {
        if (isWinning)
        {
            rb.linearVelocity = new Vector2(winDashSpeed, 0f);
            return;
        }

        if (isIntroPlaying) return;

        if (CanFly)
        {
            // ⭐ เมื่อกดค้าง: เร่งความเร็วลอยตัวขึ้นสู่ maxAscentSpeed ทันที
            if (isHoldingInput)
            {
                float currentY = rb.linearVelocity.y;
                float targetY = Mathf.MoveTowards(currentY, maxAscentSpeed, holdAcceleration * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(0f, targetY);
            }
        }

        // จำกัดความเร็วตอนตก ไม่ให้ทิ้งดิ่งเร็วเกินไป
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleWallCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleWallCollision(collision);
    }

    private void HandleWallCollision(Collision2D collision)
    {
        if (isDead || isWinning || isIntroPlaying) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                lastDamageTime = Time.time;

                if (TryGetComponent(out PlayerHealthTest healthTest))
                {
                    healthTest.TakeDamage(wallDamage);
                }
            }

            float pushDirectionY = 0f;
            if (collision.contactCount > 0)
            {
                Vector2 contactNormal = collision.GetContact(0).normal;
                if (Mathf.Abs(contactNormal.y) > 0.1f)
                {
                    pushDirectionY = Mathf.Sign(contactNormal.y);
                }
            }

            if (pushDirectionY == 0f)
            {
                pushDirectionY = (transform.position.y >= collision.collider.bounds.center.y) ? 1f : -1f;
            }

            rb.linearVelocity = new Vector2(0f, pushDirectionY * wallBounceForce);
            transform.position += new Vector3(0f, pushDirectionY * 0.15f, 0f);

            ApplyStun(wallStunDuration);
        }
    }

    public void ApplyStun(float duration)
    {
        if (isDead || isWinning || isIntroPlaying) return;
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
        isHoldingInput = false;

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);
        rb.linearVelocity = new Vector2(winDashSpeed, 0f);

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        Debug.Log($"<color=green>[Player] ชนะเกม! กำลังเร่งเครื่องพุ่งหลุดเฟรมด้วยความเร็ว {winDashSpeed}...</color>");
    }

    public void TriggerDeath()
    {
        isDead = true;
        canControl = false;
        isHoldingInput = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }
}