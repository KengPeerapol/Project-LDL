using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] itemPrefabs;    // ใส่ Prefab ไอเทมทั้งหมดลงในช่องนี้ (ทั้งยิงเร็วและเพิ่มเลือด)
    public float spawnRate = 10f;
    public int maxItemsOnScreen = 3;

    [Header("Spawn Bounds")]
    public Vector2 minBounds = new Vector2(-6f, -3f);
    public Vector2 maxBounds = new Vector2(6f, 3f);

    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = Time.time + 2f;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomItem();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    private void SpawnRandomItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        // นับไอเทมทั้งสองประเภทรวมกันในฉาก
        int currentItems = FindObjectsByType<FireRateItem>(FindObjectsSortMode.None).Length +
                           FindObjectsByType<HealthItem>(FindObjectsSortMode.None).Length;

        if (currentItems >= maxItemsOnScreen) return;

        // สุ่มไอเทมจากรายการ
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject selectedPrefab = itemPrefabs[randomIndex];

        // สุ่มพิกัดเกิด
        float randomX = Random.Range(minBounds.x, maxBounds.x);
        float randomY = Random.Range(minBounds.y, maxBounds.y);
        Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

        Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
        Vector3 size = new Vector3(Mathf.Abs(maxBounds.x - minBounds.x), Mathf.Abs(maxBounds.y - minBounds.y), 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}