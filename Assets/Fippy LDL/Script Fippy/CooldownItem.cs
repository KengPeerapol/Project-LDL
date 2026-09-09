using UnityEngine;

public class CooldownItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("เวลาคูลดาวน์ที่จะลดลง (วินาที)")]
    public float cooldownReduction = 5.0f;

    [Tooltip("อายุของไอเทมก่อนหายไปเอง (วินาที)")]
    public float lifeTime = 10.0f;

    private void Start()
    {
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollection(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollection(collision.gameObject);
    }

    private void HandleCollection(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            if (target.TryGetComponent(out PlayerAttackTest playerAttack))
            {
                playerAttack.ReduceChargeCooldown(cooldownReduction);
                Destroy(gameObject);
            }
        }
    }
}