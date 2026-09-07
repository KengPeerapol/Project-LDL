using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public float firePointDistance = 0.6f; // ระยะห่างกระสุนจากตัวละคร

    private float nextFireTime = 0f;
    private InputAction attackAction;

    private void Awake()
    {
        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Keyboard>/space");

        attackAction.performed += context => Shoot();
    }

    private void OnEnable() => attackAction.Enable();
    private void OnDisable() => attackAction.Disable();

    private void Update()
    {
        // บังคับล็อกให้จุดปล่อยกระสุนอยู่ "ด้านขวา" ของตัวละครเสมอ
        if (firePoint != null)
        {
            firePoint.localPosition = Vector2.right * firePointDistance;
        }
    }

    private void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            if (bulletPrefab == null || firePoint == null) return;

            nextFireTime = Time.time + fireRate;

            // เสกกระสุน
            GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                // บังคับให้กระสุนพุ่งไปทาง "ขวา (Vector2.right)" เสมอ
                bulletScript.Setup(Vector2.right);
            }
        }
    }
}