using System.Collections;
using UnityEngine;

public class EnemyHealthTest : MonoBehaviour
{
    [Header("Lifetime Settings (ระยะเวลาการคงอยู่)")]
    public bool autoDestroy = true;
    public float lifeTime = 5f;

    [Header("Health & Score Settings (เลือดและคะแนน)")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Tooltip("⭐ คะแนนที่ได้รับทันทีเมื่อยิงศัตรูตัวนี้ตาย")]
    public int scoreReward = 15;

    [Tooltip("ดาเมจที่ทำใส่ Player เมื่อชน (ใส่ 0 = ชนฟรีไม่เสียเลือด)")]
    public int contactDamage = 10;

    [Tooltip("ติ๊กถูก = ตัวนี้จะไม่โดนยิงและยิงไม่ตาย (กระสุนยิงทะลุผ่าน)")]
    public bool isInvulnerable = false;

    [Header("Escape Settings (การหลุดออกนอกจอ)")]
    [Tooltip("ติ๊กถูก = หากหลุดออกนอกจอจะนับเป็น Escape | ติ๊กออก = ลบทิ้งเงียบๆ ไม่คิดค่า Escape")]
    public bool canTriggerEscape = true;

    [Header("Player Collision (การชนกับผู้เล่น)")]
    public bool giveScoreOnPlayerHit = true;
    public bool destroyOnHitPlayer = true;
    public float damageCooldown = 0.3f;
    private float lastHitPlayerTime = -999f;

    [Header("Spike Death Settings (ปล่อยหนามตอนตาย)")]
    [Tooltip("ติ๊กถูกถ้าต้องการให้ตัวนี้ปล่อยหนามตอนตาย")]
    public bool spawnSpikesOnDeath = true;
    public GameObject spikePrefab;
    public int spikeCount = 4;
    public float spikeSpeed = 6f;

    [Header("Spike Rotation Variation")]
    public bool randomizeSpikeAngle = true;
    public bool useEnemyRotation = true;

    [Header("Hit Flash Effect")]
    public Color hitFlashColor = Color.red;
    public float flashDuration = 0.08f;

    [Header("Death Effects")]
    public GameObject deathEffectPrefab;

    // ==================== Optimized Private Cached ====================
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private WaitForSeconds cachedFlashWait;
    private AudioManger audioManager;
    private bool isDead = false;
    private bool killedByPlayerCollision = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // แคช WaitForSeconds เพื่อ Zero GC Alloc
        cachedFlashWait = new WaitForSeconds(flashDuration);

        // แคช AudioManager ครั้งเดียวตั้งแต่เริ่มเกม
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManger>();
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (autoDestroy)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    // ⭐ รับดาเมจจากการยิง
    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;

        if (spriteRenderer != null && gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashHitRoutine());
        }

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage);
    }

    private IEnumerator FlashHitRoutine()
    {
        spriteRenderer.color = hitFlashColor;
        yield return cachedFlashWait;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        HandlePlayerCollision(collider.gameObject);
    }

    private void HandlePlayerCollision(GameObject target)
    {
        if (isDead) return;

        if (target.CompareTag("Player") || target.transform.root.CompareTag("Player"))
        {
            if (Time.time < lastHitPlayerTime + damageCooldown) return;
            lastHitPlayerTime = Time.time;

            PlayerHealthTest healthTest = target.GetComponentInParent<PlayerHealthTest>();
            if (healthTest != null)
            {
                if (contactDamage > 0)
                {
                    healthTest.TakeDamage(contactDamage, "Enemy");
                    Debug.Log($"<color=red>[Collision] ชน Player! เสียเลือด: -{contactDamage}</color>");
                }
                else
                {
                    Debug.Log($"<color=cyan>[Collision] ชนเก็บแต้มสำเร็จ! (ดาเมจ 0 ไม่เสียเลือด)</color>");
                    if (audioManager != null && audioManager.CollectPoint != null)
                    {
                        audioManager.PlaySFX(audioManager.CollectPoint);
                    }
                }
            }

            if (giveScoreOnPlayerHit && GameScoreManager.Instance != null && scoreReward > 0)
            {
                GameScoreManager.Instance.AddScore(scoreReward);
                Debug.Log($"<color=green>[Score] ได้รับคะแนน: +{scoreReward}</color>");
            }

            if (destroyOnHitPlayer)
            {
                killedByPlayerCollision = true;
                Die();
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // ⭐ ให้คะแนนทันทีเมื่อโดนยิงตาย (ไม่ต้องเสกแต้มหรือร่างใหม่)
        if (!killedByPlayerCollision && GameScoreManager.Instance != null && scoreReward > 0)
        {
            GameScoreManager.Instance.AddScore(scoreReward);
            Debug.Log($"<color=green>[Score] ยิงศัตรูตาย! ได้รับคะแนนทันที: +{scoreReward}</color>");

            if (audioManager != null && audioManager.CollectPoint != null)
            {
                audioManager.PlaySFX(audioManager.CollectPoint);
            }
        }

        // ปล่อยหนามตอนตาย (ถ้าเปิดไว้และตัวศัตรูมีดาเมจ)
        if (contactDamage > 0 && spawnSpikesOnDeath && spikePrefab != null)
        {
            SpawnBouncingSpikes();
        }

        // เอฟเฟกต์ระเบิดตอนตาย
        if (deathEffectPrefab != null)
        {
            Quaternion randomRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            Instantiate(deathEffectPrefab, transform.position, randomRot);
        }

        Destroy(gameObject);
    }

    private void SpawnBouncingSpikes()
    {
        float startAngle = 0f;
        if (useEnemyRotation) startAngle += transform.eulerAngles.z;
        if (randomizeSpikeAngle) startAngle += Random.Range(0f, 360f);

        float angleStep = 360f / spikeCount;

        for (int i = 0; i < spikeCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;

            GameObject newSpike = Instantiate(spikePrefab, transform.position, Quaternion.identity);
            if (newSpike.TryGetComponent(out BouncingSpikeTest spikeScript))
            {
                spikeScript.Setup(direction, spikeSpeed);
            }
        }
    }
}