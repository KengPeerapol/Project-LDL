using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;           // ค่าความเร็วการยิงปกติ
    public float firePointDistance = 0.6f;

    private float defaultFireRate;          // บันทึกค่าเดิมไว้สำหรับคืนค่า
    private Coroutine buffCoroutine;        // ตัวนับเวลา Buff
    private float nextFireTime = 0f;
    private InputAction attackAction;

    private void Awake()
    {
        defaultFireRate = fireRate;

        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Keyboard>/space");

        attackAction.performed += context => Shoot();
    }

    private void OnEnable() => attackAction.Enable();
    private void OnDisable() => attackAction.Disable();

    private void Update()
    {
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

            GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Setup(Vector2.right);
            }
        }
    }

    // ฟังก์ชันรับ Buff ความเร็วยิง
    public void ApplyFireRateBuff(float boostedRate, float duration)
    {
        if (buffCoroutine != null)
        {
            StopCoroutine(buffCoroutine); // หากเก็บซ้ำ ให้ยกเลิกตัวนับเดิม
        }
        buffCoroutine = StartCoroutine(FireRateBuffRoutine(boostedRate, duration));
    }

    private IEnumerator FireRateBuffRoutine(float boostedRate, float duration)
    {
        fireRate = boostedRate; // ปรับความเร็วเป็นค่า Buff (0.2)
        Debug.Log("Fire Rate Buff Active! FireRate: " + fireRate);

        yield return new WaitForSeconds(duration); // รอเวลา 10 วินาที

        fireRate = defaultFireRate; // คืนค่าความเร็วปกติ
        buffCoroutine = null;
        Debug.Log("Fire Rate Buff Ended! Reverted to: " + fireRate);
    }
}