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

    private Transform player;
    private bool hasExploded = false;

    private void Start()
    {
        // ค้นหาตำแหน่งผู้เล่นจาก Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null || hasExploded) return;

        // คำนวณระยะห่างระหว่างศัตรูกับผู้เล่น
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // เมื่อผู้เล่นเข้ามาอยู่ในระยะ ให้เริ่มระเบิดหนามทันที
        if (distanceToPlayer <= explosionRange)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // 1. เสกเอฟเฟกต์ระเบิด (ถ้ามี)
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 2. เสกหนามกระจายออกรอบทิศ 360 องศา
        SpawnSpikesAround();

        Debug.Log("<color=yellow>[ExplodingEnemy] ระเบิดหนามกระจายใส่ผู้เล่นแล้ว!</color>");

        // 3. ทำลายตัวศัตรูทิ้งทันที
        Destroy(gameObject);
    }

    private void SpawnSpikesAround()
    {
        if (spikePrefab == null || spikeCount <= 0) return;

        float angleStep = 360f / spikeCount;
        float currentAngle = 0f;

        for (int i = 0; i < spikeCount; i++)
        {
            // คำนวณเวกเตอร์ทิศทางรอบวงกลม
            float dirX = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(dirX, dirY).normalized;

            Vector3 spawnPosition = transform.position + (Vector3)(direction * spikeSpawnOffset);
            GameObject spike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);

            // รองรับทั้ง BouncingSpikeTest และ BouncingSpike
            if (spike.TryGetComponent(out BouncingSpikeTest spikeTest))
            {
                spikeTest.Setup(direction, spikeSpeed);
            }
            else if (spike.TryGetComponent(out BouncingSpike spikeNormal))
            {
                spikeNormal.Setup(direction, spikeSpeed);
            }

            currentAngle += angleStep;
        }
    }

    // วาดวงกลมสีแดงแสดงระยะจุดชนวนระเบิดในหน้า Scene ตอนตั้งค่า
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}