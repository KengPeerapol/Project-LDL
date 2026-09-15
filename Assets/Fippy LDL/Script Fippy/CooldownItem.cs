using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CooldownItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("เวลาคูลดาวน์ที่จะลดลง (วินาที)")]
    public float cooldownReduction = 5.0f;

    [Tooltip("อายุของไอเทมก่อนหายไปเอง (วินาที)")]
    public float lifeTime = 10.0f;

    private Collider2D myCollider;
    private bool isCollected = false;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollection(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollection(collision.gameObject);
    }

    private void HandleCollection(GameObject target)
    {
        if (isCollected) return;

        if (target.CompareTag("Player"))
        {
            // ค้นหา PlayerAttackTest ทั้งบนตัวที่ชนและบนตัวแม่
            PlayerAttackTest playerAttack = target.GetComponentInParent<PlayerAttackTest>();

            if (playerAttack != null)
            {
                isCollected = true;

                // ⭐ ปิด Collider ทันที เพื่อตัดแรงกระแทกฟิสิกส์ ไม่ให้ Player ชะงัก
                if (myCollider != null) myCollider.enabled = false;

                playerAttack.ReduceChargeCooldown(cooldownReduction);
                Destroy(gameObject);
            }
        }
    }
}