using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public int maxHealth = 30;
    public int currentHealth;

    [Header("Score Settings")]
    public int scoreValue = 10; // จำนวนคะแนนที่ผู้เล่นจะได้เมื่อกำจัดศัตรูตัวนี้

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // ส่งคะแนนไปบวกที่ ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}