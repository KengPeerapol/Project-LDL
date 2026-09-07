    using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f; // ความเร็วของศัตรู (ปรับให้ช้ากว่า Player จะได้หนีพ้น)

    private Transform player;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // ค้นหาเป้าหมายในฉากที่มีป้ายชื่อว่า "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("หาผู้เล่นไม่เจอ! ลืมเปลี่ยน Tag ให้ Player หรือเปล่า?");
        }
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            // คำนวณทิศทางจากตำแหน่งศัตรู ชี้ไปยังตำแหน่งผู้เล่น
            Vector2 direction = (player.position - transform.position).normalized;

            // สั่งให้ศัตรูเดินเข้าไปหาด้วยความเร็วที่กำหนด
            rb.linearVelocity = direction * moveSpeed;
        }
    }
}