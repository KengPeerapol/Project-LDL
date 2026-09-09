using UnityEngine;

public class EnemyDamageTest : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("ดาเมจที่ทำใส่ผู้เล่น")]
    public int damage = 20;

    [Header("Destroy Settings")]
    [Tooltip("ทำลายตัวเองทันทีหลังพุ่งชนผู้เล่นหรือไม่")]
    public bool destroyOnHit = true;

    // ชนแบบฟิสิกส์ปกติ (รองรับกับ MovementTest ที่ต้องเด้งกำแพง)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision.gameObject);
    }

    // ชนแบบ Trigger (กรณีติ๊ก Is Trigger ไว้)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision.gameObject);
    }

    private void TryDealDamage(GameObject target)
    {
        // ตรวจสอบทั้ง PlayerHealthTest และ PlayerHealth เดิม
        if (target.TryGetComponent(out PlayerHealthTest playerTest))
        {
            playerTest.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
        }
        else if (target.TryGetComponent(out PlayerHealth player))
        {
            player.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}