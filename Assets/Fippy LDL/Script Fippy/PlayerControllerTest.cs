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

    [Header("ตั้งค่าการบิน: กดครั้งเดียว (Tap)")]
    [Tooltip("แรงยกตัวเมื่อกดเคาะ 1 ครั้ง (ค่ายิ่งน้อย ยิ่งขึ้นทีละนิด เช่น 3 - 4)")]
    public float tapForce = 3.2f;

    [Header("ตั้งค่าการบิน: กดค้าง (Hold)")]
    [Tooltip("เวลากดค้างขั้นต่ำที่จะเริ่มเร่งเครื่องพุ่งไว (วินาที เช่น 0.12 - 0.15)")]
    public float holdThreshold = 0.12f;

    [Tooltip("ความเร่งในการพุ่งขึ้นตอนกดค้าง (ค่ายิ่งเยอะ ยิ่งไต่ระดับไว)")]
    public float holdAcceleration = 35f;

    [Tooltip("เพดานความเร็วลอยขึ้นสูงสุดตอนกดค้าง (พุ่งขึ้นได้เร็วสุดเท่าไหร่)")]
    public float maxHoldAscentSpeed = 7.5f;

    [Header("ตั้งค่าการหล่น / แรงโน้มถ่วง")]
    [Tooltip("ค่าแรงโน้มถ่วง (ค่ายิ่งน้อย ยิ่งตกช้าลง เช่น 0.5 - 0.7)")]
    public float customGravityScale = 0.6f;

    [Tooltip("จำกัดความเร็วตกสูงสุด ไม่ให้ตกเร็วเกินไปเวลาทิ้งดิ่ง")]
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
    private bool canControl = false;
    private bool isIntroPlaying = false;
    private bool isWinning = false;
    private bool isDead = false;

    // ตัวแปรจับสถานะ Tap / Hold
    private bool isHoldingInput = false;
    private bool tapRequested = false;
    private float holdTimer = 0f;
    private float lastDamageTime = -999f;

    // ⭐ Property สำหรับให้สคริปต์อื่นเช็กว่าพร้อมควบคุมหรือยัง
    public bool CanShootAndControl => canControl && !isIntroPlaying && !isWinning && !isDead;
    private bool IsActive => !isWinning && !isDead && !isIntroPlaying;
    private bool CanFly => IsActive && canControl;

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

        // ตรวจจับการกดปุ่มเฟรมแรก (Tap)
        bool mouseTap = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool spaceTap = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        // ตรวจจับการกดค้าง (Hold)
        bool mouseHold = Mouse.current != null && Mouse.current.rightButton.isPressed;
        bool spaceHold = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        isHoldingInput = mouseHold || spaceHold;

        if (mouseTap || spaceTap)
        {
            tapRequested = true;
            holdTimer = 0f;
        }

        if (isHoldingInput)
        {
            holdTimer += Time.deltaTime;
        }
        else
        {
            holdTimer = 0f;
        }

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
            // 1. ถ้าเป็นการเคาะ 1 ครั้ง (Tap)
            if (tapRequested)
            {
                rb.linearVelocity = new Vector2(0f, tapForce);
                tapRequested = false;
            }
            // 2. ถ้ากดค้างเกินเวลาที่กำหนด (Hold) -> เร่งความเร็วพุ่งขึ้นไวๆ
            else if (isHoldingInput && holdTimer >= holdThreshold)
            {
                float currentY = rb.linearVelocity.y;
                float targetY = Mathf.MoveTowards(currentY, maxHoldAscentSpeed, holdAcceleration * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(0f, Mathf.Max(targetY, tapForce));
            }
        }
        else
        {
            tapRequested = false;
        }

        // จำกัดความเร็วตกสูงสุด
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
                else if (TryGetComponent(out PlayerHealth health))
                {
                    health.TakeDamage(wallDamage);
                }
            }

            float wallCenterY = collision.collider.bounds.center.y;
            float pushDirectionY = (transform.position.y >= wallCenterY) ? 1f : -1f;

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
        tapRequested = false;
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
        tapRequested = false;
        isHoldingInput = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }
}