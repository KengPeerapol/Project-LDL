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

    [Header("ตั้งค่าการบิน (คลิกขวาค้าง หรือ Spacebar)")]
    public float flyForce = 5f;

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
    private bool isFlapping = false;
    private float lastDamageTime = -999f;

    // ⭐ Property สำหรับให้สคริปต์อื่น (เช่น ปืน) เช็กว่าอนุญาตให้ยิงหรือยัง
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

    // ⭐ Coroutine นำตัว Player พุ่งจากนอกจอเข้ามาที่จุดเริ่มต้น
    private IEnumerator IntroDashRoutine()
    {
        isIntroPlaying = true;
        canControl = false;

        // 1. จำตำแหน่งที่วางไว้ใน Scene
        Vector3 targetDestination = transform.position;

        // 2. ย้ายตำแหน่ง Player ไปอยู่นอกจอทางซ้าย
        transform.position = targetDestination + new Vector3(-introSpawnOffsetX, 0f, 0f);

        // 3. ปิดฟิสิกส์แรงโน้มถ่วงชั่วคราว และจัดมุมหันหัวตรง
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);

        // 4. บินพุ่งเข้ามายังจุดเป้าหมายอย่างนุ่มนวล
        while (Vector3.Distance(transform.position, targetDestination) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetDestination, introDashSpeed * Time.deltaTime);
            yield return null;
        }

        // 5. ปรับพิกัดเข้าล็อกเป๊ะๆ
        transform.position = targetDestination;

        // 6. คืนค่าฟิสิกส์และเปิดการควบคุมให้เล่นเกมได้ตามปกติ
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

        bool mousePress = Mouse.current != null && Mouse.current.rightButton.isPressed;
        bool spacePress = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        isFlapping = mousePress || spacePress;

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

        if (isFlapping && CanFly)
        {
            FlyUp();
        }

        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    private void FlyUp()
    {
        rb.linearVelocity = new Vector2(0f, flyForce);
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
        isFlapping = false;

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
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }
}