using System.Collections;
using UnityEngine;

public class ItemSpawnerTest : MonoBehaviour
{
    [Header("Prefab ไอเทมที่จะสปอว์น")]
    [Tooltip("ลาก Prefab ของ Item (เช่น CooldownItem, HealthItem) มาใส่ในนี้")]
    public GameObject[] itemPrefabs;

    [Header("ตำแหน่งและขอบเขตการสปอว์น")]
    [Tooltip("ตำแหน่งแกน X ที่จะให้ไอเทมเริ่มเกิด (ทางขวาของจอ)")]
    public float spawnX = 10f;
    public float minY = -3.5f;
    public float maxY = 3.5f;

    [Header("ความถี่ในการเกิดไอเทม (วินาที)")]
    public float minSpawnInterval = 4f;
    public float maxSpawnInterval = 7f;

    [Header("ตรวจจับ Event (ตัวหยุดสปอว์น)")]
    [Tooltip("ลาก GameObject 'Event' จาก Hierarchy มาวางใส่ช่องนี้")]
    public GameObject eventObject;

    private bool isSpawningAllowed = true;
    private Coroutine spawnLoop;

    private void Start()
    {
        // เริ่มต้นลูปสปอว์นไอเทม
        spawnLoop = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // 1. เช็กว่าตัว Event กำลังเปิดทำงานอยู่หรือไม่
            bool isEventActive = (eventObject != null && eventObject.activeInHierarchy);

            // 2. ถ้าเปิดระบบ และไม่มี Event เกิดขึ้น ให้สร้างไอเทม
            if (isSpawningAllowed && !isEventActive)
            {
                SpawnRandomItem();
            }
        }
    }

    private void SpawnRandomItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject selectedItem = itemPrefabs[randomIndex];

        if (selectedItem != null)
        {
            float randomY = Random.Range(minY, maxY);
            Vector3 spawnPosition = new Vector3(spawnX, randomY, 0f);

            Instantiate(selectedItem, spawnPosition, Quaternion.identity);
        }
    }

    // ⭐ ฟังก์ชันสำหรับให้สคริปต์อื่นสั่งหยุดปล่อยไอเทม
    public void StopSpawning()
    {
        isSpawningAllowed = false;
    }

    // ⭐ ฟังก์ชันสั่งให้กลับมาปล่อยไอเทมใหม่
    public void ResumeSpawning()
    {
        isSpawningAllowed = true;
    }
}