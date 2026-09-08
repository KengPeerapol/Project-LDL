using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public int maxHealth = 30;
    public int currentHealth;

    [Header("Score Settings")]
    public int scoreValue = 10; // จำนวนคะแนนที่ผู้เล่นจะได้เมื่อกำจัดศัตรูตัวนี้

    [Header("Death Spike Settings")]
    public GameObject spikePrefab;      // Prefab หนามที่จะเสกออกมาตอนตาย
    public int spikeCount = 6;          // จำนวนหนามที่จะกระจายรอบตัว (เช่น 4, 6, 8 หรือ 12 ดอก)
    public float spikeSpeed = 8f;       // ความเร็วในการพุ่งออกของหนาม
    public float spawnOffset = 0.5f;     // ระยะห่างจากจุดกึ่งกลาง เพื่อดันให้หนามพุ่งออกจากนอก Collider ไม่ติดกันเอง

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 1. เสกหนามกระจายพุ่งออกรอบทิศทาง
        SpawnSpikes();

        // 2. ส่งคะแนนไปบวกที่ ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        // 3. ทำลายตัวศัตรูทิ้งทันที
        Destroy(gameObject);
    }

    private void SpawnSpikes()
    {
        // ตรวจสอบว่าได้ลาก Prefab หนามมาใส่แล้วหรือไม่
        if (spikePrefab == null || spikeCount <= 0) return;

        // คำนวณมุมองศาที่ต้องแบ่งเท่า ๆ กันตามจำนวนหนาม
        float angleStep = 360f / spikeCount;
        float currentAngle = 0f;

        for (int i = 0; i < spikeCount; i++)
        {
            // แปลงมุมองศาเป็นทิศทาง Vector2 (แกน X, Y)
            float dirX = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(dirX, dirY).normalized;

            // ดันจุดเกิดให้อยู่ห่างจากศัตรูตามทิศทางพุ่ง เพื่อไม่ให้ติด Collider ซ้อนทับกัน
            Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffset);

            // เสกหนามออกมา
            GameObject spike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);

            // ส่งทิศทางและค่าความเร็วให้หนามพุ่งตัวออกไปทันที
            BouncingSpike spikeScript = spike.GetComponent<BouncingSpike>();
            if (spikeScript != null)
            {
                spikeScript.Setup(direction, spikeSpeed);
            }

            // ขยับมุมไปทิศถัดไป
            currentAngle += angleStep;
        }
    }
}