using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RotateSelf : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("ความเร็วในการหมุน (องศา/วินาที)")]
    public float rotationSpeed = 200f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // ปลดล็อกการหมุนแกน Z เพื่อให้หมุนตัวได้
        rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
        rb.angularVelocity = rotationSpeed;
    }

    private void FixedUpdate()
    {
        // บังคับความเร็วในการหมุนให้คงที่ตลอดเวลา แม้จะชนกำแพง
        rb.angularVelocity = rotationSpeed;
    }
}