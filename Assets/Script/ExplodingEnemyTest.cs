using System.Collections;

using UnityEngine;

public class ExplodingEnemyTest : MonoBehaviour

{

    [Header("Health & Hit Settings (พลังชีวิตและการโดนยิง)")]

    [Tooltip("พลังชีวิตสูงสุดของศัตรูระเบิด")]

    public int maxHealth = 60;

    public int currentHealth;

    [Tooltip("ติ๊กถูก = เมื่อโดนยิงจนเลือดหมด ให้จุดชนวนระเบิดหนามทันที | ติ๊กออก = ตายธรรมดาไม่ปล่อยหนาม")]

    public bool explodeOnDeath = true;

    [Header("Hit Flash Settings (เอฟเฟกต์กระพริบเมื่อโดนยิง)")]

    [Tooltip("SpriteRenderer ของศัตรู (ถ้าเว้นว่างไว้ สคริปต์จะค้นหาให้อัตโนมัติ)")]

    public SpriteRenderer spriteRenderer;

    [Tooltip("สีที่จะกระพริบตอนโดนยิง (แนะนำสีแดง Color.red หรือสีส้ม เพื่อให้เห็นชัดบน Sprite)")]

    public Color flashColor = Color.red;

    [Tooltip("ระยะเวลาการกระพริบแฟลช (วินาที)")]

    public float flashDuration = 0.1f;

    [Tooltip("Material สำหรับทำ Flash สีขาวล้วน (Optional - ลาก Flash Material มาใส่ถ้ามี)")]

    public Material flashMaterial;

    [Header("Explosion Trigger")]

    [Tooltip("ระยะห่างจาก Player ที่จะเริ่มจุดชนวนระเบิด")]

    public float explosionRange = 2.5f;

    [Tooltip("Effect ระเบิดตอนแตกตัว (Optional)")]

    public GameObject explosionEffect;

    [Header("Spike Explosion Settings (ระเบิดหนาม)")]

    [Tooltip("Prefab ของกระสุนหนาม")]

    public GameObject spikePrefab;

    [Tooltip("จำนวนหนามที่จะกระจายออกมารอบตัว")]

    public int spikeCount = 8;

    [Tooltip("ความเร็วของหนาม")]

    public float spikeSpeed = 8f;

    [Tooltip("ระยะห่างจุดเกิดหนามจากจุดกึ่งกลางตัว")]

    public float spikeSpawnOffset = 0.5f;

    [Header("Spike Rotation Variation (การหมุนทิศทางหนาม)")]

    [Tooltip("สุ่มมุมเริ่มต้นทุกครั้งที่ระเบิด เพื่อไม่ให้พุ่งออกมุมเดิมซ้ำๆ")]

    public bool randomizeAngle = true;

    [Tooltip("ใช้องศาการหมุนของตัวศัตรูในขณะนั้นเป็นฐาน (ถ้าศัตรูกำลังหมุนควงสว่านอยู่)")]

    public bool useCurrentRotationAsBase = true;

    private Transform player;

    private bool hasExploded = false;

    // ตัวแปรสำหรับระบบ Flash

    private Color originalColor = Color.white;

    private Material originalMaterial;

    private Coroutine flashCoroutine;

    private void Awake()

    {

        currentHealth = maxHealth;

        // ค้นหา SpriteRenderer อัตโนมัติหากไม่ได้ลากใส่ใน Inspector

        if (spriteRenderer == null)

        {

            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)

            {

                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            }

        }

        // บันทึกสีและ Material เดิมไว้คืนค่าหลัง Flash

        if (spriteRenderer != null)

        {

            originalColor = spriteRenderer.color;

            originalMaterial = spriteRenderer.material;

        }

    }

    private void Start()

    {

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)

        {

            player = playerObj.transform;

        }

    }

    private void Update()

    {

        if (player == null || hasExploded) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= explosionRange)

        {

            Explode();

        }

    }

    // ⭐ ฟังก์ชันรับดาเมจเมื่อโดนยิง (เรียกใช้โดยกระสุนหรือเลเซอร์)

    public void TakeDamage(int damage)

    {

        if (hasExploded) return;

        currentHealth -= damage;

        // สั่งเล่นเอฟเฟกต์ Flash กระพริบทันที

        PlayHitFlash();

        Debug.Log($"<color=orange>[ExplodingEnemy] โดนโจมตี! ดาเมจ: {damage} | เลือดคงเหลือ: {currentHealth}/{maxHealth}</color>");

        if (currentHealth <= 0)

        {

            Die();

        }

    }

    // ⭐ ระบบสั่งกระพริบ Flash Hit

    public void PlayHitFlash()

    {

        if (spriteRenderer == null) return;

        if (flashCoroutine != null)

        {

            StopCoroutine(flashCoroutine);

        }

        flashCoroutine = StartCoroutine(FlashRoutine());

    }

    private IEnumerator FlashRoutine()

    {

        // แบบที่ 1: ถ้ามีการใส่ Flash Material (แฟลชขาวล้วน)

        if (flashMaterial != null)

        {

            spriteRenderer.material = flashMaterial;

            yield return new WaitForSeconds(flashDuration);

            if (spriteRenderer != null)

            {

                spriteRenderer.material = originalMaterial;

            }

        }

        // แบบที่ 2: ถ้าไม่ได้ใส่ Material จะเปลี่ยนสี Sprite เป็น flashColor ชั่วคราว (เช่น สีแดง)

        else

        {

            spriteRenderer.color = flashColor;

            yield return new WaitForSeconds(flashDuration);

            if (spriteRenderer != null)

            {

                spriteRenderer.color = originalColor;

            }

        }

        flashCoroutine = null;

    }

    private void Die()

    {

        if (hasExploded) return;

        if (explodeOnDeath)

        {

            Explode();

        }

        else

        {

            hasExploded = true;

            if (explosionEffect != null)

            {

                Quaternion randomEffectRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

                Instantiate(explosionEffect, transform.position, randomEffectRot);

            }

            Destroy(gameObject);

        }

    }

    private void Explode()

    {

        hasExploded = true;

        // 1. เสกเอฟเฟกต์ระเบิด

        if (explosionEffect != null)

        {

            Quaternion randomEffectRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            Instantiate(explosionEffect, transform.position, randomEffectRot);

        }

        // 2. เสกหนามกระจายรอบตัว

        SpawnSpikesAround();

        Debug.Log("<color=yellow>[ExplodingEnemy] ระเบิดหนามกระจายใส่ผู้เล่นแล้ว!</color>");

        // 3. ทำลายตัวศัตรูทิ้ง

        Destroy(gameObject);

    }

    private void SpawnSpikesAround()

    {

        if (spikePrefab == null || spikeCount <= 0) return;

        float startAngle = 0f;

        if (useCurrentRotationAsBase)

        {

            startAngle += transform.eulerAngles.z;

        }

        if (randomizeAngle)

        {

            startAngle += Random.Range(0f, 360f);

        }

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

            else if (spike.TryGetComponent(out BouncingSpikeTest spikeNormal))

            {

                spikeNormal.Setup(direction, spikeSpeed);

            }

        }

    }

    // ตรวจจับเมื่อชนกับกระสุนของผู้เล่น (Tag: Bullet)

    private void OnTriggerEnter2D(Collider2D collision)

    {

        if (hasExploded) return;

        if (collision.CompareTag("Bullet"))

        {

            // ดึงสคริปต์กระสุนมารับค่าดาเมจ (หรือคิดดาเมจพื้นฐาน 25 หากไม่ได้ส่งค่ามา)

            TakeDamage(25);

        }

    }

    private void OnDisable()

    {

        if (flashCoroutine != null)

        {

            StopCoroutine(flashCoroutine);

            flashCoroutine = null;

        }

        // คืนค่าสีและ Material ให้สะอาดเรียบร้อยป้องกันค้าง

        if (spriteRenderer != null)

        {

            if (flashMaterial != null && originalMaterial != null)

            {

                spriteRenderer.material = originalMaterial;

            }

            spriteRenderer.color = originalColor;

        }

    }

    private void OnDrawGizmosSelected()

    {

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, explosionRange);

    }

}
 