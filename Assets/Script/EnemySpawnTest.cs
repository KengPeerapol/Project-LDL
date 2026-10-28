using UnityEngine;

public class EnemySpawnTest : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("ชื่อสำหรับระบุประเภท (ตั้งไว้ดูเองใน Inspector)")]
        public string enemyName = "Normal Enemy";

        [Tooltip("Prefab ของศัตรู")]
        public GameObject prefab;

        [Range(1, 100)]
        [Tooltip("ค่าน้ำหนักโอกาสเกิด ยิ่งใส่เยอะยิ่งเกิดบ่อย (เช่น ตัวธรรมดา 70, ตัวหายาก 15)")]
        public int spawnWeight = 50;
    }

    [Header("Enemy Prefabs & Spawn Rate (ตั้งค่าศัตรูและโอกาสเกิด)")]
    [Tooltip("รายการศัตรูที่ต้องการสุ่มเกิด พร้อมค่าน้ำหนักโอกาสเกิด")]
    public EnemySpawnEntry[] enemies;

    [Header("Spawn Interval Settings (ความถี่ในการเกิด)")]
    [Tooltip("เปิดใช้งานการสุ่มช่วงเวลาเกิด (ถ้าปิดจะใช้เวลาคงที่)")]
    public bool useRandomInterval = true;

    [Tooltip("ระยะเวลาเกิดขั้นต่ำ (วินาที)")]
    public float minSpawnInterval = 1.0f;

    [Tooltip("ระยะเวลาเกิดสูงสุด (วินาที)")]
    public float maxSpawnInterval = 2.5f;

    [Tooltip("เกิดตัวแรกทันทีหลังจาก Player บินเข้าจอเสร็จ")]
    public bool spawnImmediately = true;

    [Header("Player Reference")]
    [Tooltip("ลาก Player มาใส่ หรือปล่อยว่างไว้ให้ค้นหาอัตโนมัติได้")]
    public PlayerControllerTest playerController;

    [Header("Gizmos Visual (การแสดงขอบเขตในหน้า Scene)")]
    [Tooltip("ให้แสดงกรอบพื้นที่เกิดตลอดเวลา แม้ไม่ได้คลิกเลือกตัว Spawner")]
    public bool alwaysShowGizmo = true;
    public Color gizmoColor = new Color(0f, 1f, 0.5f, 0.8f);
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
        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogWarning("ยังไม่ได้ตั้งค่ารายการศัตรูใน EnemySpawnTest!", this);
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
            timer = spawnImmediately ? 0f : GetNextInterval();
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = GetNextInterval(); // คำนวณเวลารอตัวถัดไป
        }
    }

    // ⭐ สุ่มเลือกศัตรูตามค่าน้ำหนักโอกาสเกิด (Weighted Random)
    private void SpawnEnemy()
    {
        GameObject selectedPrefab = GetRandomWeightedEnemy();
        if (selectedPrefab == null) return;

        GetSpawnBounds(out float minY, out float maxY);

        float randomY = Random.Range(minY, maxY);
        Vector3 spawnPosition = new Vector3(transform.position.x, randomY, 0f);

        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }

    private GameObject GetRandomWeightedEnemy()
    {
        // 1. รวมผลรวมน้ำหนักทั้งหมด
        int totalWeight = 0;
        foreach (var entry in enemies)
        {
            if (entry != null && entry.prefab != null)
            {
                totalWeight += entry.spawnWeight;
            }
        }

        if (totalWeight <= 0) return null;

        // 2. สุ่มเลขตั้งแต่ 0 ถึง totalWeight
        int randomRoll = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        // 3. ตรวจสอบว่าตกอยู่ในช่วงของศัตรูตัวไหน
        foreach (var entry in enemies)
        {
            if (entry != null && entry.prefab != null)
            {
                currentWeightSum += entry.spawnWeight;
                if (randomRoll < currentWeightSum)
                {
                    return entry.prefab;
                }
            }
        }

        return enemies[0].prefab;
    }

    private float GetNextInterval()
    {
        if (useRandomInterval)
        {
            return Random.Range(minSpawnInterval, maxSpawnInterval);
        }
        return minSpawnInterval;
    }

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

    private void DrawSpawnGizmos()
    {
        GetSpawnBounds(out float minY, out float maxY);

        float height = maxY - minY;
        float centerY = (minY + maxY) / 2f;
        Vector3 centerPos = new Vector3(transform.position.x, centerY, 0f);
        Vector3 boxSize = new Vector3(gizmoAreaWidth, height, 0.1f);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.15f);
        Gizmos.DrawCube(centerPos, boxSize);

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(centerPos, boxSize);

        float halfW = gizmoAreaWidth * 0.75f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(transform.position.x - halfW, maxY, 0f), new Vector3(transform.position.x + halfW, maxY, 0f));
        Gizmos.DrawLine(new Vector3(transform.position.x - halfW, minY, 0f), new Vector3(transform.position.x + halfW, minY, 0f));
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