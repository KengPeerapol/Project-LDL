using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ItemMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; // ความเร็วในการพุ่งเป็นเส้นตรง

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // ล็อกไม่ให้ไอเทมหมุนเคว้ง
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // ค้นหาตำแหน่งผู้เล่น
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector2 targetDirection;

        if (playerObj != null)
        {
            // คำนวณทิศทางเล็งตรงไปยังจุดที่ผู้เล่นยืนอยู่ ณ ขณะนั้นครั้งเดียว
            targetDirection = (playerObj.transform.position - transform.position).normalized;
        }
        else
        {
            // กรณีหาผู้เล่นไม่เจอ ให้พุ่งไปทางซ้ายเป็นค่าเริ่มต้น
            targetDirection = Vector2.left;
        }

        // ปล่อยความเร็วพุ่งตรงไปข้างหน้าทันที (ไม่เลี้ยวตามผู้เล่น)
        rb.linearVelocity = targetDirection * moveSpeed;
    }
}