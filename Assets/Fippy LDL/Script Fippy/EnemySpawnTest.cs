using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [Tooltip("ลาก Prefab ศัตรูที่ต้องการทดสอบมาใส่ที่นี่")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("ระยะเวลาเกิดแต่ละตัว (วินาที)")]
    public float spawnInterval = 1.5f;
    [Tooltip("เกิดตัวแรกทันทีหลังจาก Player บินเข้าจอเสร็จ")]
    public bool spawnImmediately = true;

    [Header("Player Reference")]
    [Tooltip("ลาก Player มาใส่ หรือปล่อยว่างไว้ให้ค้นหาอัตโนมัติได้")]
    public PlayerControllerTest playerController;

    [Header("Gizmos Visual (การแสดงขอบเขตในหน้า Scene)")]
    [Tooltip("ให้แสดงกรอบพื้นที่เกิดตลอดเวลา แม้ไม่ได้คลิกเลือกตัว Spawner")]
    public bool alwaysShowGizmo = true;
    public Color gizmoColor = new Color(0f, 1f, 0.5f, 0.8f); // สีเขียวนีออน
    [Tooltip("ความกว้างของแถบแสดงจุดเกิดในหน้า Scene")]
    public float gizmoAreaWidth = 1f;

    private float timer;
    private bool hasStartedSpawning = false;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("ยังไม่ได้ใส่ Enemy Prefab ใน EnemySpawnTest!", this);
            enabled = false;
            return;
        }

        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponentInParent<PlayerControllerTest>();
            }
        }
    }

    private void Update()
    {
        // รอจนกว่า Player จะบิน Intro เข้าจอเสร็จ และหยุดเกิดเมื่อ Player ตาย/ชนะ
        if (playerController != null && !playerController.CanShootAndControl)
        {
            return;
        }

        if (!hasStartedSpawning)
        {
            hasStartedSpawning = true;
            timer = spawnImmediately ? 0f : spawnInterval;
        }

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

        // คำนวณความสูงต่ำสุด-สูงสุด
        GetSpawnBounds(out float minY, out float maxY);

        // สุ่มพิกัดความสูงแกน Y
        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(transform.position.x, randomY, 0f);

        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }

    // ⭐ ฟังก์ชันคำนวณขอบเขตแกน Y กลาง (ใช้ร่วมกันทั้งตอนเกิดและตอนวาดเส้น Gizmos)
    private void GetSpawnBounds(out float minY, out float maxY)
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (col != null)
        {
            minY = col.bounds.min.y;
            maxY = col.bounds.max.y;
        }
        else if (sr != null)
        {
            minY = sr.bounds.min.y;
            maxY = sr.bounds.max.y;
        }
        else
        {
            float halfHeight = transform.localScale.y / 2f;
            minY = transform.position.y - halfHeight;
            maxY = transform.position.y + halfHeight;
        }
    }

    // ⭐ วาดขอบเขตใน Scene View
    private void DrawSpawnGizmos()
    {
        GetSpawnBounds(out float minY, out float maxY);

        float height = maxY - minY;
        float centerY = (minY + maxY) / 2f;
        Vector3 centerPos = new Vector3(transform.position.x, centerY, 0f);
        Vector3 boxSize = new Vector3(gizmoAreaWidth, height, 0.1f);

        // 1. วาดกรอบสี่เหลี่ยมโปร่งใสแสดงพื้นที่สุ่มเกิด
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.15f);
        Gizmos.DrawCube(centerPos, boxSize);

        // 2. วาดเส้นกรอบนอก
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(centerPos, boxSize);

        // 3. วาดเส้นขีดจุดต่ำสุดและจุดสูงสุด
        float halfW = gizmoAreaWidth * 0.75f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(transform.position.x - halfW, maxY, 0f), new Vector3(transform.position.x + halfW, maxY, 0f)); // เพดานบน
        Gizmos.DrawLine(new Vector3(transform.position.x - halfW, minY, 0f), new Vector3(transform.position.x + halfW, minY, 0f)); // พื้นล่าง
    }

    private void OnDrawGizmos()
    {
        if (alwaysShowGizmo)
        {
            DrawSpawnGizmos();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!alwaysShowGizmo)
        {
            DrawSpawnGizmos();
        }
    }
}