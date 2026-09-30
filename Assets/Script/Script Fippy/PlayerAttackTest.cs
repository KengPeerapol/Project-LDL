using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackTest : MonoBehaviour
{
    [Header("Gun & Aiming Settings")]
    [Tooltip("ลาก GameObject 'GunPivot' ที่สร้างไว้มาใส่ช่องนี้")]
    public Transform gunPivot;              // ตัวควบคุมจุดหมุนปืน
    [Tooltip("องศาชดเชย (Sprite แคปซูลแนวตั้งให้ใช้ -90 เพื่อให้ชี้ไปทางขวา)")]
    public float gunRotationOffset = -90f;  // ชดเชยให้ปลายแคปซูลชี้ตรงกับเมาส์

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    public float firePointDistance = 0.7f;

    [Header("Aim Angle Clamp (จำกัดมุมชี้ด้านหน้า)")]
    [Range(0f, 360f)]
    public float maxAimAngle = 80f;

    [Header("Charge Attack Settings")]
    public float chargeTimeRequired = 1.0f;
    public float chargeCooldown = 5.0f;
    public int normalDamage = 25;
    public int chargedDamage = 100;
    public float chargedBulletScale = 2.5f;

    private float nextFireTime = 0f;
    private float currentChargeTimer = 0f;
    private float chargeCooldownTimer = 0f;
    private bool isCharging = false;
    private bool hasLoggedFullCharge = false;

    private Camera mainCamera;
    private Vector2 currentAimDirection = Vector2.right;

    // ⭐ อ้างอิงเฉพาะ PlayerHealthTest ตัวเดียว
    private PlayerHealthTest playerHealthTest;
    private PlayerControllerTest playerController;

    private void Start()
    {
        mainCamera = Camera.main;
        playerHealthTest = GetComponent<PlayerHealthTest>();
        playerController = GetComponent<PlayerControllerTest>();
    }

    private void Update()
    {
        // บล็อกไม่ให้เล็งหรือยิง หากตัวละครตาย หรืออยู่ในช่วงบิน Intro เข้าจอ / ชนะเกม
        if (IsPlayerDead() || (playerController != null && !playerController.CanShootAndControl))
        {
            currentChargeTimer = 0f;
            isCharging = false;
            return;
        }

        UpdateCooldown();
        HandleMouseAiming();
        HandleShootingInput();
    }

    private void UpdateCooldown()
    {
        if (chargeCooldownTimer > 0f)
        {
            chargeCooldownTimer -= Time.deltaTime;

            if (chargeCooldownTimer <= 0f)
            {
                chargeCooldownTimer = 0f;
                Debug.Log("<color=green>[Charged Shot] ชาร์จยิงพร้อมใช้งานแล้ว! (Ready)</color>");
            }
        }
    }

    // ⭐ ตรวจสอบสถานะการตายผ่าน PlayerHealthTest เพียงตัวเดียว
    private bool IsPlayerDead()
    {
        if (playerHealthTest != null && playerHealthTest.currentHealth <= 0f)
        {
            return true;
        }
        return false;
    }

    private void HandleMouseAiming()
    {
        if (mainCamera == null || Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 pivotOrigin = (gunPivot != null) ? (Vector2)gunPivot.position : (Vector2)transform.position;

        Vector2 rawDirection = (Vector2)mouseWorldPos - pivotOrigin;
        float targetAngle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;

        targetAngle = Mathf.Clamp(targetAngle, -maxAimAngle, maxAimAngle);

        float rad = targetAngle * Mathf.Deg2Rad;
        currentAimDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        if (gunPivot != null)
        {
            gunPivot.rotation = Quaternion.Euler(0f, 0f, targetAngle + gunRotationOffset);
        }

        if (firePoint != null && firePoint.parent != gunPivot)
        {
            firePoint.position = pivotOrigin + (currentAimDirection * firePointDistance);
        }
    }

    private void HandleShootingInput()
    {
        if (Mouse.current == null) return;

        // 1. เริ่มคลิกซ้าย
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isCharging = true;
            currentChargeTimer = 0f;
            hasLoggedFullCharge = false;
        }

        // 2. คลิกซ้ายค้างเพื่อชาร์จ
        if (Mouse.current.leftButton.isPressed && isCharging)
        {
            currentChargeTimer += Time.deltaTime;

            if (currentChargeTimer >= chargeTimeRequired && !hasLoggedFullCharge)
            {
                hasLoggedFullCharge = true;
                if (chargeCooldownTimer <= 0f)
                {
                    Debug.Log("<color=cyan>[Charged Shot] ชาร์จเต็ม 100%! ปล่อยคลิกซ้ายเพื่อยิงกระสุนยักษ์</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[Charged Shot] ชาร์จเต็ม แต่ติดคูลดาวน์! เหลือ {chargeCooldownTimer:F1} วิ</color>");
                }
            }
        }

        // 3. ปล่อยคลิกซ้ายเพื่อปล่อยกระสุน
        if (Mouse.current.leftButton.wasReleasedThisFrame && isCharging)
        {
            isCharging = false;

            if (currentChargeTimer >= chargeTimeRequired)
            {
                if (chargeCooldownTimer <= 0f)
                {
                    FireBullet(chargedDamage, chargedBulletScale);
                    chargeCooldownTimer = chargeCooldown;
                    Debug.Log($"<color=red>[Charged Shot] ยิงกระสุนชาร์จสำเร็จ! ดาเมจ {chargedDamage}</color>");
                }
                else
                {
                    FireBullet(normalDamage, 1.0f);
                    Debug.LogWarning("[Charged Shot] ติดคูลดาวน์ ยิงเป็นกระสุนธรรมดาแทน");
                }
            }
            else if (Time.time >= nextFireTime)
            {
                FireBullet(normalDamage, 1.0f);
                nextFireTime = Time.time + fireRate;
            }

            currentChargeTimer = 0f;
            hasLoggedFullCharge = false;
        }
    }

    private void FireBullet(int damage, float scale)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletTest bulletScript = newBullet.GetComponent<BulletTest>();
        if (bulletScript != null)
        {
            bulletScript.Setup(currentAimDirection, damage, scale);
        }

        // เรียกเล่นเสียงยิงปืน
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            AudioManger audioManager = audioObj.GetComponent<AudioManger>();
            if (audioManager != null && audioManager.Shoot != null)
            {
                audioManager.PlaySFX(audioManager.Shoot);
            }
        }
    }

    private void OnGUI()
    {
        if (playerController != null && !playerController.CanShootAndControl) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;

        if (chargeCooldownTimer > 0f)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(20, 20, 300, 30), $"Charged Shot Cooldown: {chargeCooldownTimer:F1}s", style);
        }
        else
        {
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(20, 20, 300, 30), "Charged Shot: READY", style);
        }

        if (isCharging)
        {
            style.normal.textColor = Color.yellow;
            float chargePercent = Mathf.Clamp01(currentChargeTimer / chargeTimeRequired) * 100f;
            GUI.Label(new Rect(20, 50, 300, 30), $"Charging: {chargePercent:F0}%", style);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, (Vector3)currentAimDirection * 2f);
    }

    public void ReduceChargeCooldown(float reductionTime)
    {
        if (chargeCooldownTimer > 0f)
        {
            chargeCooldownTimer = Mathf.Max(0f, chargeCooldownTimer - reductionTime);

            if (chargeCooldownTimer <= 0f)
            {
                Debug.Log("<color=green>[Item] ลดคูลดาวน์! ปืนพร้อมชาร์จยิงทันที!</color>");
            }
            else
            {
                Debug.Log($"<color=cyan>[Item] ลดคูลดาวน์ลง {reductionTime}s เหลืออีก {chargeCooldownTimer:F1}s</color>");
            }
        }
    }
}