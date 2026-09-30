using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FinishLine : MonoBehaviour
{
    [Header("Finish Line Movement")]
    [Tooltip("ให้เส้นชัยเลื่อนมาทางซ้ายตามความเร็วของฉากหรือไม่")]
    public bool moveLeftWithScene = true;

    [Tooltip("ความเร็วในการเลื่อน (ตั้งให้เท่ากับ scrollSpeed ของพื้นหลัง)")]
    public float scrollSpeed = 3f;

    private BoxCollider2D boxCollider;
    private bool hasTriggered = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true; // บังคับให้เป็น Trigger เพื่อไม่ให้ชนกระแทกผู้เล่น
    }

    private void Update()
    {
        // เลื่อนเส้นชัยเข้าหาผู้เล่นจากทางขวา
        if (moveLeftWithScene && !hasTriggered)
        {
            transform.position += Vector3.left * (scrollSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;

            PlayerControllerTest player = collision.GetComponentInParent<PlayerControllerTest>();
            if (GameScoreManager.Instance != null)
            {
                GameScoreManager.Instance.CheckFinishLine(player);
            }
        }
    }

    // วาดเส้นสีเขียวใน Scene View เพื่อให้เห็นตำแหน่งเส้นชัยล่องหนเวลาพัฒนาเกม
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);
        }
        else
        {
            Gizmos.DrawLine(transform.position + Vector3.up * 6f, transform.position + Vector3.down * 6f);
        }
    }
}