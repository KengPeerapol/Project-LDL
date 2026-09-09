using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealthTest : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Lifetime Settings")]
    public float lifeTime = 8f;

    [Header("Collision With Player")]
    public int damageToPlayerOnCrash = 20;

    [Header("Death Spikes Settings")]
    public GameObject spikePrefab;
    public int spikeCount = 6;
    public float spikeSpeed = 8f;
    public float spawnOffset = 0.5f;

    private Collider2D myCollider;
    private bool isDead = false;
    private bool hasCrashed = false;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        SpawnDeathSpikes();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCrash(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandlePlayerCrash(collision.gameObject);
    }

    private void HandlePlayerCrash(GameObject target)
    {
        if (isDead || hasCrashed) return;

        if (target.CompareTag("Player"))
        {
            hasCrashed = true;

            if (myCollider != null) myCollider.enabled = false;

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

            Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffset);
            GameObject spike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);

            // รองรับทั้งชื่อคลาส BouncingSpikeTest และ BouncingSpike
            if (spike.TryGetComponent(out BouncingSpikeTest spikeTest))
            {
                spikeTest.Setup(direction, spikeSpeed);
            }
            else if (spike.TryGetComponent(out BouncingSpike spikeNormal))
            {
                spikeNormal.Setup(direction, spikeSpeed);
            }

            currentAngle += angleStep;
        }
    }
}