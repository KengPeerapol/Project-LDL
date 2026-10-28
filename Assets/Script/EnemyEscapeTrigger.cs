using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyEscapeTrigger : MonoBehaviour
{
    [Header("Detection Tag")]
    public string enemyTag = "Enemy";

    private void Awake()
    {
        // บังคับให้ Collider เป็น Trigger เสมอ
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจจับเฉพาะศัตรู (ข้าม Player, Bullet และ Point)
        if (other.CompareTag(enemyTag) || other.transform.root.CompareTag(enemyTag))
        {
            // ดึง Root GameObject ของศัตรู
            GameObject enemyObj = other.transform.root.gameObject;

            // ส่งข้อมูลแจ้งเตือนระบบ
            if (EnemyEscapeManager.Instance != null)
            {
                EnemyEscapeManager.Instance.OnEnemyEscaped(enemyObj.name);
            }

            // ทำลายศัตรูทิ้งทันทีเมื่อหลุดออกนอกจอ
            Destroy(enemyObj);
        }
    }
}