using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] enemyPrefabs; // เปลี่ยนเป็น Array เพื่อใส่ได้หลายตัว
    public float spawnRate = 2f;
    public float randomYRange = 3f;

    private float nextSpawnTime;

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    private void SpawnEnemy()
    {
        // เช็คว่ามี Prefab ในช่องหรือเปล่า
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("ยังไม่ได้ใส่ Enemy Prefabs ใน Spawner!");
            return;
        }

        // สุ่มเลือกลิสต์ศัตรูจาก Array
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedEnemy = enemyPrefabs[randomIndex];

        // สุ่มตำแหน่งแกน Y
        float randomY = Random.Range(-randomYRange, randomYRange);
        Vector3 spawnPosition = transform.position + new Vector3(0f, randomY, 0f);

        // เสกศัตรูตัวที่ถูกสุ่มเลือก
        Instantiate(selectedEnemy, spawnPosition, Quaternion.identity);
    }
}