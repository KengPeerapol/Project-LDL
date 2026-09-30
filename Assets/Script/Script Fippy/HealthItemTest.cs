using UnityEngine;

public class HealthItemTest : MonoBehaviour
{
    [Header("Lifetime Settings (ระยะเวลาการคงอยู่)")]
    [Tooltip("เปิดใช้งานให้ไอเทมหายไปเองอัตโนมัติเมื่อหมดเวลา")]
    public bool autoDestroy = true;

    [Tooltip("ระยะเวลาที่ไอเทมจะคงอยู่ก่อนหายไป (วินาที)")]
    public float lifeTime = 5f; // ⭐ ตั้งเวลาให้ไอเทมสลายไปเอง เช่น 4 - 6 วินาที

    [Header("Pickup Settings (การเก็บไอเทม)")]
    [Tooltip("ทำลายไอเทมทิ้งทันทีเมื่อเก็บสำเร็จ (ถ้าติ๊กออก จะเข้าสู่โหมด Cooldown ซ่อนตัวแล้วเกิดใหม่)")]
    public bool destroyOnPickup = true; // ⭐ แนะนำเปิดไว้สำหรับไอเทมที่เสกออกมาในฉาก

    [Header("Heal Settings (ตั้งค่าการฟื้นฟู)")]
    [Tooltip("จำนวนเลือดที่จะฟื้นฟูเมื่อเก็บไอเทม")]
    public float healAmount = 25f;

    [Header("Cooldown Settings (กรณี destroyOnPickup = false)")]
    [Tooltip("ระยะเวลาก่อนที่ไอเทมชิ้นนี้จะสามารถเก็บได้อีกครั้ง (วินาที)")]
    public float pickupCooldown = 5.0f;

    [Header("Effects")]
    [Tooltip("เอฟเฟกต์เมื่อเก็บไอเทม (Particle System หรือเสียง)")]
    public GameObject pickupEffectPrefab;

    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;
    private bool isCoolingDown = false;
    private float currentCooldownTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // ⭐ นับถอยหลังตามค่า lifeTime เพื่อทำลายไอเทมทิ้งอัตโนมัติหากไม่มีคนเก็บ
        if (autoDestroy)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    private void Update()
    {
        // ระบบนับถอยหลังคูลดาวน์ (ทำงานเฉพาะเมื่อ destroyOnPickup = false)
        if (isCoolingDown)
        {
            currentCooldownTimer -= Time.deltaTime;
            if (currentCooldownTimer <= 0f)
            {
                isCoolingDown = false;
                SetItemActive(true); // คูลดาวน์หมด เปิดไอเทมให้เก็บได้ใหม่
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCoolingDown) return;

        // ตรวจสอบว่าชนกับ Player หรือไม่
        if (collision.CompareTag("Player") || collision.transform.root.CompareTag("Player"))
        {
            PlayerHealthTest playerHealth = collision.GetComponentInParent<PlayerHealthTest>();

            if (playerHealth != null)
            {
                // เช็กว่าเลือดเต็มหรือยัง ถ้าเต็มแล้วจะไม่ให้เก็บ
                if (playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    return;
                }

                // สั่งฮีลเลือด
                playerHealth.Heal(healAmount);
                Debug.Log($"<color=green>[Item] เก็บไอเทมฮีลสำเร็จ! ฟื้นฟูเลือด +{healAmount}</color>");

                // --- เพิ่มโค้ดเรียกเสียงเก็บไอเทม (Pick) ตรงนี้ ---
                GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
                if (audioObj != null)
                {
                    AudioManger audioManager = audioObj.GetComponent<AudioManger>();
                    if (audioManager != null && audioManager.Pick != null)
                    {
                        // สั่งเล่นเสียงโดยอ้างอิงจากตัวแปร Pick ใน AudioManger
                        audioManager.PlaySFX(audioManager.Pick);
                    }
                }
                // ---------------------------------

                // เล่นเอฟเฟกต์ (ถ้ามี)
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }

                // เลือกว่าจะทำลายทิ้งทันที หรือเข้าสู่ช่วง Cooldown รอเกิดใหม่
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    StartCooldown();
                }
            }
        }
    }

    private void StartCooldown()
    {
        isCoolingDown = true;
        currentCooldownTimer = pickupCooldown;
        SetItemActive(false); // ซ่อนภาพและปิดการชนชั่วคราว
    }

    private void SetItemActive(bool isActive)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = isActive;
        if (itemCollider != null) itemCollider.enabled = isActive;
    }
}