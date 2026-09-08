using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthItem : MonoBehaviour
{
    [Header("Health Settings")]
    public int healAmount = 25;       // จำนวนเลือดที่จะฟื้นฟู
    public bool onlyHealWhenDamaged = true; // เก็บได้เฉพาะตอนเลือดไม่เต็มหรือไม่

    [Header("Item Life Settings")]
    public float itemLifetime = 15f;  // หายไปเองหากไม่มีคนเก็บใน 15 วิ

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
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // ถ้าตั้งให้เก็บได้เฉพาะตอนเลือดลด และเลือดปัจจุบันเต็มอยู่ จะยังไม่ให้เก็บ
                if (onlyHealWhenDamaged && playerHealth.currentHealth >= playerHealth.maxHealth)
                {
                    return;
                }

                playerHealth.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}