using UnityEngine;

public class ExplodingEnemy : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRange = 1.5f;   // ระยะที่จะเริ่มระเบิด
    public int explosionDamage = 25;      // ดาเมจระเบิดที่จะทำใส่ผู้เล่น
    public GameObject explosionEffect;    // (Optional) ใส่ Effect ระเบิดถ้ามี

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

        // ถ้าผู้เล่นเข้ามาอยู่ในระยะระเบิด
        if (distanceToPlayer <= explosionRange)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // เสก Effect ระเบิด (ถ้าใส่ไว้)
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // ส่งดาเมจไปตัดเลือด Player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(explosionDamage);
        }

        Debug.Log("ศัตรูระเบิดใส่ผู้เล่น!");
        Destroy(gameObject); // ทำลายตัวศัตรูทิ้งทันที
    }

    // วาดวงกลมสีแดงโชว์ระยะระเบิดในหน้า Scene ตอนตั้งค่า
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}