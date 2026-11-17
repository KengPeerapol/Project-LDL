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
   [Tooltip("คะแนนที่ได้รับทันทีเมื่อยิงศัตรูตัวนี้ตาย")]
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
   [Header("Spike Death Settings (ระเบิดหนามตอนตาย)")]
   [Tooltip("ติ๊กถูกถ้าต้องการให้ตัวนี้ปล่อยหนามตอนตาย")]
   public bool spawnSpikesOnDeath = true;
   [Tooltip("Prefab ของกระสุนหนาม")]
   public GameObject spikePrefab;
   [Tooltip("จำนวนหนามที่จะกระจายออกมารอบตัว")]
   public int spikeCount = 4;
   [Tooltip("ความเร็วของหนาม")]
   public float spikeSpeed = 6f;
   [Tooltip("ระยะห่างจุดเกิดหนามจากจุดกึ่งกลางตัว")]
   public float spikeSpawnOffset = 0.5f;
   [Header("Spike Rotation Variation (การหมุนทิศทางหนาม)")]
   [Tooltip("สุ่มมุมเริ่มต้นทุกครั้งที่ระเบิด เพื่อไม่ให้พุ่งออกมุมเดิมซ้ำๆ")]
   public bool randomizeAngle = true;
   [Tooltip("ใช้องศาการหมุนของตัวศัตรูในขณะนั้นเป็นฐาน (ถ้าศัตรูกำลังหมุนควงสว่านอยู่)")]
   public bool useCurrentRotationAsBase = true;
   [Header("Hit Flash Effect")]
   public Color hitFlashColor = Color.red;
   public float flashDuration = 0.08f;
   [Header("Procedural Pixel Shatter (ระบบเม็ดสี่เหลี่ยมตัวแตกสร้างอัตโนมัติ)")]
   [Tooltip("เปิด/ปิดระบบตัวแตกเป็นเม็ดสี่เหลี่ยม")]
   public bool enablePixelShatter = true;
   [Tooltip("จำนวนเม็ดสี่เหลี่ยมที่จะแตกกระจาย")]
   public int pixelCount = 16;
   [Tooltip("ขนาดของเม็ดสี่เหลี่ยม")]
   public float pixelSize = 0.22f;
   [Tooltip("แรงระเบิดดีดกระจายตัว")]
   public float explosionSpeed = 6.5f;
   [Tooltip("ระยะเวลาการแสดงผลก่อนจางหาย (วินาที)")]
   public float pixelDuration = 0.65f;
   [Tooltip("สีของเม็ดสี่เหลี่ยม (ปล่อย Color.clear ไว้เพื่อดึงสีตาม Sprite มอนสเตอร์อัตโนมัติ)")]
   public Color customPixelColor = Color.clear;
   [Header("Death Effects")]
   public GameObject deathEffectPrefab;
   // ==================== Optimized Private Cached ====================
   private SpriteRenderer spriteRenderer;
   private Color originalColor = Color.white;
   private Coroutine flashCoroutine;
   private WaitForSeconds cachedFlashWait;
   private AudioManger audioManager;
   private bool isDead = false;
   private bool killedByPlayerCollision = false;
   private static Sprite cachedSquareSprite;
   private void Awake()
   {
       spriteRenderer = GetComponent<SpriteRenderer>();
       if (spriteRenderer != null) originalColor = spriteRenderer.color;
       cachedFlashWait = new WaitForSeconds(flashDuration);
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
       else
       {
           // เล่นเสียงศัตรูเจ็บเมื่อโดนยิง (และยังไม่ตาย เพื่อไม่ให้เสียงตีกับเสียงตาย)
           if (audioManager != null && audioManager.EnemyHurt != null)
           {
               audioManager.PlaySFX(audioManager.EnemyHurt);
           }
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
               }
               else
               {
                   if (audioManager != null && audioManager.CollectPoint != null)
                   {
                       audioManager.PlaySFX(audioManager.CollectPoint);
                   }
               }
           }
           if (giveScoreOnPlayerHit && GameScoreManager.Instance != null && scoreReward > 0)
           {
               GameScoreManager.Instance.AddScore(scoreReward);
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
       if (!killedByPlayerCollision && GameScoreManager.Instance != null && scoreReward > 0)
       {
           GameScoreManager.Instance.AddScore(scoreReward);
           if (audioManager != null)
           {
               if (audioManager.CollectPoint != null)
               {
                   audioManager.PlaySFX(audioManager.CollectPoint);
               }
               if (audioManager.EnemyDie != null)
               {
                   audioManager.PlaySFX(audioManager.EnemyDie);
               }
           }
       }
       if (enablePixelShatter)
       {
           SpawnSquarePixelShatter();
       }
       if (spawnSpikesOnDeath && spikePrefab != null)
       {
           SpawnBouncingSpikes();
       }
       if (deathEffectPrefab != null)
       {
           Quaternion randomRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
           Instantiate(deathEffectPrefab, transform.position, randomRot);
       }
       Destroy(gameObject);
   }
   private void SpawnBouncingSpikes()
   {
       if (spikePrefab == null || spikeCount <= 0) return;
       float startAngle = 0f;
       if (useCurrentRotationAsBase) startAngle += transform.eulerAngles.z;
       if (randomizeAngle) startAngle += Random.Range(0f, 360f);
       float angleStep = 360f / spikeCount;
       for (int i = 0; i < spikeCount; i++)
       {
           float currentAngle = startAngle + (i * angleStep);
           float dirX = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
           float dirY = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
           Vector2 direction = new Vector2(dirX, dirY).normalized;
           Vector3 spawnPosition = transform.position + (Vector3)(direction * spikeSpawnOffset);
           GameObject spike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);
           if (spike.TryGetComponent(out BouncingSpikeTest spikeTest))
           {
               spikeTest.Setup(direction, spikeSpeed);
           }
       }
   }
   private void SpawnSquarePixelShatter()
   {
       Sprite squareSprite = GetOrCreateSquareSprite();
       Color baseCol = (customPixelColor != Color.clear) ? customPixelColor : originalColor;
       for (int i = 0; i < pixelCount; i++)
       {
           GameObject pixelObj = new GameObject("Pixel_Debris");
           pixelObj.transform.position = transform.position + (Vector3)(Random.insideUnitCircle * 0.25f);
           float sizeVariation = Random.Range(pixelSize * 0.7f, pixelSize * 1.35f);
           pixelObj.transform.localScale = Vector3.one * sizeVariation;
           SpriteRenderer sr = pixelObj.AddComponent<SpriteRenderer>();
           sr.sprite = squareSprite;
           float shade = Random.Range(0.85f, 1.15f);
           Color pColor = new Color(
               Mathf.Clamp01(baseCol.r * shade),
               Mathf.Clamp01(baseCol.g * shade),
               Mathf.Clamp01(baseCol.b * shade),
               1f
           );
           sr.color = pColor;
           if (spriteRenderer != null)
           {
               sr.sortingLayerID = spriteRenderer.sortingLayerID;
               sr.sortingOrder = spriteRenderer.sortingOrder + 2;
           }
           Vector2 randomDir = Random.insideUnitCircle.normalized;
           float randomSpeed = Random.Range(explosionSpeed * 0.6f, explosionSpeed * 1.3f);
           float randomSpin = Random.Range(-450f, 450f);
           PixelDebrisMotion motion = pixelObj.AddComponent<PixelDebrisMotion>();
           motion.Setup(randomDir * randomSpeed, randomSpin, pixelDuration);
       }
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
}
public class PixelDebrisMotion : MonoBehaviour
{
   private Vector2 velocity;
   private float angularVelocity;
   private float lifeTime;
   private float totalDuration;
   private Vector3 initialScale;
   private SpriteRenderer sr;
   public void Setup(Vector2 initialVelocity, float spinSpeed, float duration)
   {
       velocity = initialVelocity;
       angularVelocity = spinSpeed;
       totalDuration = duration;
       lifeTime = duration;
       initialScale = transform.localScale;
       sr = GetComponent<SpriteRenderer>();
   }
   private void Update()
   {
       transform.position += (Vector3)(velocity * Time.deltaTime);
       velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 3.5f);
       transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);
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