using System.Collections;
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

    [Header("--- Thruster Trail Effect (เอฟเฟกต์ไฟพุ่งท้าย) ---")]
    [Tooltip("เปิด/ปิด เอฟเฟกต์ไฟไอพ่นพุ่งด้านหลัง")]
    public bool enableThrusterEffect = true;

    [Tooltip("⭐ ตำแหน่งเกิดไฟไอพ่น (ปรับ X/Y ตรงนี้เพื่อขยับตำแหน่งไฟท้ายจรวด)")]
    public Vector2 thrusterOffset = new Vector2(0.5f, 0f);

    [Tooltip("สีหลักของไฟไอพ่น (เช่น สีส้ม/เหลือง)")]
    public Color fireColor = new Color(1f, 0.5f, 0.1f, 1f);

    [Tooltip("ขนาดของเม็ดไฟไอพ่น")]
    public float particleSize = 0.25f;

    [Tooltip("ความถี่ในการปล่อยเม็ดไฟ (วินาที)")]
    public float spawnInterval = 0.03f;

    [Tooltip("ระยะเวลาที่เม็ดไฟคงอยู่ก่อนจางหาย")]
    public float particleLifetime = 0.25f;

    [Tooltip("Prefab เอฟเฟกต์ไอพ่นแบบลากใส่เอง (Optional - ถ้าใส่ Prefab ไว้ จะใช้ Prefab นี้แทนระบบโค้ด)")]
    public GameObject thrusterEffectPrefab;

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
    private float spawnTimer = 0f;
    private GameObject instantiatedThrusterChild;

    private static Sprite cachedSquareSprite;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            cachedAudioManager = audioObj.GetComponent<AudioManger>();
        }
    }

    private void Start()
    {
        if (thrusterEffectPrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.TransformDirection(thrusterOffset);
            instantiatedThrusterChild = Instantiate(thrusterEffectPrefab, spawnPos, transform.rotation, transform);
        }
    }

    private void Update()
    {
        if (isDestroyed) return;

        transform.position += Vector3.left * (flySpeed * Time.deltaTime);

        if (enableThrusterEffect && thrusterEffectPrefab == null)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnThrusterParticle();
            }
        }

        if (transform.position.x <= despawnLeftX)
        {
            Destroy(gameObject);
        }
    }

    private void SpawnThrusterParticle()
    {
        Sprite squareSprite = GetOrCreateSquareSprite();

        Vector3 spawnPos = transform.position + transform.TransformDirection(thrusterOffset);

        GameObject particleObj = new GameObject("Thruster_Particle");
        particleObj.transform.position = spawnPos + (Vector3)(Random.insideUnitCircle * 0.05f);

        float size = Random.Range(particleSize * 0.7f, particleSize * 1.3f);
        particleObj.transform.localScale = Vector3.one * size;

        SpriteRenderer sr = particleObj.AddComponent<SpriteRenderer>();
        sr.sprite = squareSprite;

        float colorVariation = Random.Range(0f, 1f);
        Color pColor = Color.Lerp(new Color(1f, 0.9f, 0.2f, 1f), fireColor, colorVariation);
        sr.color = pColor;
        sr.sortingOrder = 5;

        ThrusterParticleMotion motion = particleObj.AddComponent<ThrusterParticleMotion>();
        motion.Setup(particleLifetime);
    }

    private static Sprite GetOrCreateSquareSprite()
    {
        if (cachedSquareSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            cachedSquareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return cachedSquareSprite;
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

    public void DestroyByLaser()
    {
        if (!canBeDestroyedByLaser || isDestroyed) return;
        isDestroyed = true;

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }

        if (scoreReward > 0 && GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.AddScore(scoreReward);
            Debug.Log($"<color=green>[Missile] ยิงทำลายขีปนาวุธสำเร็จ! ได้รับคะแนน: +{scoreReward}</color>");
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, transform.rotation);
        }

        if (cachedAudioManager != null && cachedAudioManager.HurtMissile != null)
        {
            cachedAudioManager.PlaySFX(cachedAudioManager.HurtMissile);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 thrusterWorldPos = transform.position + transform.TransformDirection(thrusterOffset);
        Gizmos.DrawWireSphere(thrusterWorldPos, 0.15f);
    }
}

// ⭐ คลาสสำหรับจัดการการเคลื่อนที่และจางหายของไฟไอพ่น
public class ThrusterParticleMotion : MonoBehaviour
{
    private float lifeTime;
    private float totalDuration;
    private Vector3 initialScale;
    private SpriteRenderer sr;

    public void Setup(float duration)
    {
        totalDuration = duration;
        lifeTime = duration;
        initialScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.position += Vector3.right * (2.5f * Time.deltaTime);

        lifeTime -= Time.deltaTime;
        float ratio = Mathf.Clamp01(lifeTime / totalDuration);

        transform.localScale = initialScale * ratio;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = ratio;
            sr.color = c;
        }

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}