using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [Tooltip("ลาก Prefab ศัตรูที่ต้องการทดสอบมาใส่ที่นี่")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("ระยะเวลาเกิดแต่ละตัว (วินาที)")]
    public float spawnInterval = 1.5f;
    public bool spawnImmediately = true;

    private float timer;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // ตรวจสอบว่ามี Prefab ศัตรูหรือไม่
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("ยังไม่ได้ใส่ Enemy Prefab ใน EnemySpawnTest!", this);
            enabled = false;
            return;
        }

        timer = spawnImmediately ? 0f : spawnInterval;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedPrefab = enemyPrefabs[randomIndex];
        if (selectedPrefab == null) return;

        // คำนวณหาจุดต่ำสุดและสูงสุดของแถบแกน Y อัตโนมัติจากขนาดของ Sprite หรือ Collider
        float minY, maxY;

        if (boxCollider != null)
        {
            minY = boxCollider.bounds.min.y;
            maxY = boxCollider.bounds.max.y;
        }
        else if (spriteRenderer != null)
        {
            minY = spriteRenderer.bounds.min.y;
            maxY = spriteRenderer.bounds.max.y;
        }
        else
        {
            float halfHeight = transform.localScale.y / 2f;
            minY = transform.position.y - halfHeight;
            maxY = transform.position.y + halfHeight;
        }

        // สุ่มพิกัดความสูงแกน Y
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(transform.position.x, randomY, 0f);

        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }

    // วาดเส้นสีเขียวบอกตำแหน่งขอบเขตการเกิดในหน้า Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}