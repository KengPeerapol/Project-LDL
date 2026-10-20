using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    [Header("Missile Movement")]
    [Tooltip("ความเร็วในการพุ่งไปทางซ้าย (หน่วย/วินาที)")]
    public float flySpeed = 16f;

    [Tooltip("พิกัด X นอกจอฝั่งซ้าย ที่จะลบจรวดทิ้งอัตโนมัติ")]
    public float despawnLeftX = -12f;

    [Header("Damage")]
    public int damage = 20;

    [Header("Laser Destruction Test (ระบบทำลายด้วยเลเซอร์)")]
    [Tooltip("⭐ ติ๊กถูก = เลเซอร์สามารถยิงทำลายมิสไซล์ลูกนี้ได้ | ติ๊กออก = เลเซอร์ยิงไม่เข้า (สำหรับเปิด/ปิดทดสอบ)")]
    public bool canBeDestroyedByLaser = true;

    [Header("Destruction Effects (เอฟเฟกต์ตอนโดนยิงทำลาย)")]
    [Tooltip("Prefab เอฟเฟกต์ระเบิดตอนโดนเลเซอร์ยิงทำลาย (ถ้ามี)")]
    public GameObject explosionEffectPrefab;

    [Tooltip("แต้มคะแนนที่ได้รับเมื่อยิงทำลายมิสไซล์ได้ (ใส่ 0 ถ้าไม่ต้องการให้แต้ม)")]
    public int scoreReward = 10;

    // Optimized Cached References & Flags
    private bool isDestroyed = false;
    private Collider2D cachedCollider;
    private AudioManger cachedAudioManager;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();

        // แคช AudioManager ครั้งเดียวตอนเกิด
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            cachedAudioManager = audioObj.GetComponent<AudioManger>();
        }
    }

    private void Update()
    {
        // พุ่งตรงไปทางซ้ายด้วยความเร็วคงที่
        transform.position += Vector3.left * (flySpeed * Time.deltaTime);

        if (transform.position.x <= despawnLeftX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.gameObject);
    }

    private void CheckHit(GameObject hitObj)
    {
        if (isDestroyed) return;

        if (hitObj.CompareTag("Player"))
        {
            if (hitObj.TryGetComponent(out PlayerHealthTest healthTest))
            {
                healthTest.TakeDamage(damage, "Missile");
            }

            Destroy(gameObject);
        }
    }

    // ⭐ ฟังก์ชันสำหรับให้ Charge Laser เรียกใช้เพื่อทำลายมิสไซล์
    public void DestroyByLaser()
    {
        if (!canBeDestroyedByLaser || isDestroyed) return;
        isDestroyed = true;

        // ปิด Collider ทันทีเพื่อไม่ให้เลเซอร์ตรวจจับซ้ำในเฟรมถัดไป
        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }

        // เพิ่มคะแนน (ถ้าตั้งไว้)
        if (scoreReward > 0 && GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.AddScore(scoreReward);
            Debug.Log($"<color=green>[Missile] ยิงทำลายขีปนาวุธสำเร็จ! ได้รับคะแนน: +{scoreReward}</color>");
        }

        // สร้างเอฟเฟกต์ระเบิด (ถ้าใส่ Prefab ไว้)
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, transform.rotation);
        }

        // เล่นเสียงแจ้งเตือน/ระเบิด
        if (cachedAudioManager != null && cachedAudioManager.HurtMissile != null)
        {
            cachedAudioManager.PlaySFX(cachedAudioManager.HurtMissile);
        }

        Destroy(gameObject);
    }
}