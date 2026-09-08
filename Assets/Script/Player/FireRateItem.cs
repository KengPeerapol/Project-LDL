using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FireRateItem : MonoBehaviour
{
    [Header("Buff Settings")]
    public float newFireRate = 0.2f;    // ค่า Fire Rate ขณะรับบัฟ
    public float buffDuration = 10f;    // ระยะเวลาคงอยู่ 10 วินาที

    [Header("Item Life Settings")]
    public float itemLifetime = 15f;    // เวลาที่ไอเทมจะหายไปเองถ้าไม่ถูกเก็บ (ใส่ 0 ถ้าไม่อยากให้หาย)

    private void Start()
    {
        if (itemLifetime > 0f)
        {
            Destroy(gameObject, itemLifetime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerAttack playerAttack = collision.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.ApplyFireRateBuff(newFireRate, buffDuration);
            }

            // ทำลายไอเทมหลังเก็บ
            Destroy(gameObject);
        }
    }
}