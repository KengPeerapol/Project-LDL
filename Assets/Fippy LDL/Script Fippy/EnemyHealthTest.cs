using UnityEngine;

public class EnemyHealthTest : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Lifetime Settings")]
    [Tooltip("เวลาที่มีชีวิตอยู่ก่อนทำลายตัวเองทิ้ง (วินาที)")]
    public float lifeTime = 8f;

    [Header("Collision With Player")]
    public int damageToPlayerOnCrash = 20;

    [Header("Death Spikes (Optional)")]
    public GameObject spikePrefab;
    public int spikeCount = 6;
    public float spikeSpeed = 8f;

    private Collider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        // สั่งทำลายตัวเองล่วงหน้าตามเวลา Life Time ที่ตั้งไว้
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
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
        SpawnDeathSpikes();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (myCollider != null) myCollider.enabled = false;
            HandlePlayerCrash(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (myCollider != null) myCollider.enabled = false;
            HandlePlayerCrash(collision.gameObject);
        }
    }

    private void HandlePlayerCrash(GameObject target)
    {
        if (target.TryGetComponent(out PlayerHealthTest playerTest))
        {
            playerTest.TakeDamage(damageToPlayerOnCrash);
        }
        else if (target.TryGetComponent(out PlayerHealth player))
        {
            player.TakeDamage(damageToPlayerOnCrash);
        }

        Destroy(gameObject);
    }

    private void SpawnDeathSpikes()
    {
        if (spikePrefab == null || spikeCount <= 0) return;

        float angleStep = 360f / spikeCount;
        float currentAngle = 0f;

        for (int i = 0; i < spikeCount; i++)
        {
            float dirX = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector2 direction = new Vector2(dirX, dirY).normalized;

            GameObject spike = Instantiate(spikePrefab, transform.position + (Vector3)(direction * 0.5f), Quaternion.identity);
            BouncingSpike spikeScript = spike.GetComponent<BouncingSpike>();
            if (spikeScript != null)
            {
                spikeScript.Setup(direction, spikeSpeed);
            }

            currentAngle += angleStep;
        }
    }
}