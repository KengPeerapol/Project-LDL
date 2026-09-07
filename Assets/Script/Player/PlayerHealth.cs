using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Settings")]
    public Slider healthBar; // ลาก Slider หลอดเลือดมาใส่ช่องนี้

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ตรวจสอบ Tag ของสิ่งที่ชน
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(20); // โดนศัตรูลด 20
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            TakeDamage(10); // ชนกำแพงลด 10
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // ป้องกันเลือดติดลบ

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");

        // เรียกคำสั่ง GameOver จาก GameManager
    if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        // เปลี่ยนจาก Destroy เป็น SetActive(false) เพื่อซ่อนตัวผู้เล่นแทน
        // (ช่วยป้องกัน Error กรณีที่ศัตรูพยายามวิ่งหา Player ที่โดนลบทิ้งไปแล้ว)
        gameObject.SetActive(false);
    }
}