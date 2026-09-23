using UnityEngine;

public class ExplodingEnemyTest : MonoBehaviour
{
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
    public bool randomizeAngle = true; // ⭐ สุ่มมุมให้ไม่ซ้ำรอยเดิม

    [Tooltip("ใช้องศาการหมุนของตัวศัตรูในขณะนั้นเป็นฐาน (ถ้าศัตรูกำลังหมุนควงสว่านอยู่)")]
    public bool useCurrentRotationAsBase = true; // ⭐ ถ้ามอนหมุนอยู่ หนามจะสะบัดตามมุมที่มันกำลังหัน

    private Transform player;
    private bool hasExploded = false;

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

    private void Explode()
    {
        hasExploded = true;

        // 1. เสกเอฟเฟกต์ระเบิด (สุ่มมุมหมุนเอฟเฟกต์ด้วย เพื่อให้หน้าตาไม่ซ้ำกัน)
        if (explosionEffect != null)
        {
            Quaternion randomEffectRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            Instantiate(explosionEffect, transform.position, randomEffectRot);
        }

        // 2. เสกหนามกระจายออกแบบมีมุมหมุนผันแปร
        SpawnSpikesAround();

        Debug.Log("<color=yellow>[ExplodingEnemy] ระเบิดหนามกระจายแบบสุ่มมุมใส่ผู้เล่นแล้ว!</color>");

        // 3. ทำลายตัวศัตรูทิ้ง
        Destroy(gameObject);
    }

    private void SpawnSpikesAround()
    {
        if (spikePrefab == null || spikeCount <= 0) return;

        // คำนวณมุมเริ่มต้นที่ไม่ใช่ 0 องศาตายตัว
        float startAngle = 0f;

        if (useCurrentRotationAsBase)
        {
            startAngle += transform.eulerAngles.z; // ดึงมุมปัจจุบันของมอนสเตอร์มาใช้
        }

        if (randomizeAngle)
        {
            startAngle += Random.Range(0f, 360f); // สุ่มองศาหมุน 0-360 องศา
        }

        float angleStep = 360f / spikeCount;

        for (int i = 0; i < spikeCount; i++)
        {
            float currentAngle = startAngle + (i * angleStep);

            // คำนวณเวกเตอร์ทิศทางรอบวงกลมจากมุมที่คำนวณใหม่
            float dirX = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(dirX, dirY).normalized;

            Vector3 spawnPosition = transform.position + (Vector3)(direction * spikeSpawnOffset);
            GameObject spike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);

            if (spike.TryGetComponent(out BouncingSpikeTest spikeTest))
            {
                spikeTest.Setup(direction, spikeSpeed);
            }
            else if (spike.TryGetComponent(out BouncingSpike spikeNormal))
            {
                spikeNormal.Setup(direction, spikeSpeed);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}