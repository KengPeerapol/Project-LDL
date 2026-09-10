using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerTest : MonoBehaviour
{
    [Header("ตั้งค่าการบิน (คลิกซ้ายค้าง หรือ Spacebar)")]
    public float flyForce = 5f;

    [Header("ตั้งค่าการเอียงตัว")]
    public float baseRotationZ = -90f;
    public float tiltUpwardOffset = 20f;
    public float tiltDownwardOffset = -20f;
    public float rotationSpeed = 15f;

    [Header("ตั้งค่าการชนกำแพง (Wall Collision)")]
    public int wallDamage = 10;
    public float wallBounceForce = 7f;
    public float wallStunDuration = 0.15f;
    public float damageCooldown = 0.5f;

    private Rigidbody2D rb;
    private bool canControl = true;
    private bool isWinning = false;
    private bool isDead = false;
    private bool isFlapping = false;
    private float lastDamageTime = -999f;

    private bool IsActive => !isWinning && !isDead;
    private bool CanFly => IsActive && canControl;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    private void Update()
    {
        bool mousePress = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool spacePress = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        isFlapping = mousePress || spacePress;

        if (IsActive)
        {
            HandleRotationSmooth();
        }
    }

    private void FixedUpdate()
    {
        if (isFlapping && CanFly)
        {
            FlyUp();
        }
    }

    private void FlyUp()
    {
        rb.linearVelocity = new Vector2(0f, flyForce);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleWallCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleWallCollision(collision);
    }

    private void HandleWallCollision(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                lastDamageTime = Time.time;

                if (TryGetComponent(out PlayerHealthTest healthTest))
                {
                    healthTest.TakeDamage(wallDamage);
                }
                else if (TryGetComponent(out PlayerHealth health))
                {
                    health.TakeDamage(wallDamage);
                }
            }

            float wallCenterY = collision.collider.bounds.center.y;
            float pushDirectionY = (transform.position.y >= wallCenterY) ? 1f : -1f;

            rb.linearVelocity = new Vector2(0f, pushDirectionY * wallBounceForce);
            transform.position += new Vector3(0f, pushDirectionY * 0.15f, 0f);

            ApplyStun(wallStunDuration);
        }
    }

    public void ApplyStun(float duration)
    {
        if (isDead) return;
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        canControl = false;
        yield return new WaitForSeconds(duration);
        canControl = true;
    }

    private void HandleRotationSmooth()
    {
        float targetAngle;

        if (rb.linearVelocity.y > 0.1f) targetAngle = baseRotationZ + tiltUpwardOffset;
        else if (rb.linearVelocity.y < -0.1f) targetAngle = baseRotationZ + tiltDownwardOffset;
        else return;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void TriggerWinDash()
    {
        isWinning = true;
        canControl = false;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.Euler(0f, 0f, baseRotationZ);
        rb.linearVelocity = new Vector2(5f, 0f);
    }

    public void TriggerDeath()
    {
        isDead = true;
        canControl = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }
}