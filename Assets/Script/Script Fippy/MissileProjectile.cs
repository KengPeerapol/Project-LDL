using UnityEngine;

public class MissileProjectile : MonoBehaviour
{
    [Header("Missile Movement")]
    [Tooltip("ความเร็วในการพุ่งไปทางซ้าย (หน่วย/วินาที)")]
    public float flySpeed = 16f;

    [Tooltip("พิกัด X นอกจอฝั่งซ้าย ที่จะลบจรวดทิ้งอัตโนมัติ")]
    public float despawnLeftX = -12f;

    [Header("Damage")]
    public int damage = 20;

    private void Update()
    {
        // พุ่งตรงไปทางซ้ายด้วยความเร็วคงที่
        transform.position += Vector3.left * flySpeed * Time.deltaTime;

        if (transform.position.x <= despawnLeftX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckHit(collision.gameObject);
    }

    private void CheckHit(GameObject hitObj)
    {
        if (hitObj.CompareTag("Player"))
        {
            if (hitObj.TryGetComponent(out PlayerHealthTest healthTest))
            {
                healthTest.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}