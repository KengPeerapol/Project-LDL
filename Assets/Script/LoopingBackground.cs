using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    [Header("Background Sprites (ใส่ทั้ง 4 แผ่น)")]
    [Tooltip("ลาก Background ทั้ง 4 ชิ้นมาใส่ในนี้ตามลำดับ")]
    public Transform[] backgrounds;

    [Header("Scroll Settings")]
    [Tooltip("ความเร็วในการเลื่อนไปทางซ้าย")]
    public float scrollSpeed = 3f;

    [Tooltip("ระยะเหลื่อมชดเชยรอยต่อ (0.01 - 0.05 หากเห็นเส้นรอยต่อกระพริบ)")]
    public float seamOffset = 0f;

    [Header("Reset Settings")]
    [Tooltip("จุดแกน X ฝั่งซ้ายที่ถ้าหลุดเกินจุดนี้ไป จะถือว่าพ้นจอแล้วย้ายไปต่อท้ายสุด")]
    public float resetThresholdX = -20f;
    public bool autoCalculateThreshold = true;

    private float bgWidth;

    private void Start()
    {
        if (backgrounds == null || backgrounds.Length == 0) return;

        // คำนวณความกว้างจริงของ Sprite จากแผ่นแรก
        SpriteRenderer sr = backgrounds[0].GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            bgWidth = sr.bounds.size.x;
        }
        else
        {
            bgWidth = 20f;
        }

        // คำนวณจุดหลุดขอบจอซ้ายอัตโนมัติจากขนาดของภาพ
        if (autoCalculateThreshold)
        {
            resetThresholdX = -bgWidth;
        }

        // จัดเรียงตำแหน่งแผ่นที่เหลือให้ต่อท้ายกันไปทางขวาอัตโนมัติตั้งแต่เริ่มเกม
        for (int i = 1; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] != null && backgrounds[i - 1] != null)
            {
                backgrounds[i].position = new Vector3(
                    backgrounds[i - 1].position.x + bgWidth - seamOffset,
                    backgrounds[0].position.y,
                    backgrounds[0].position.z
                );
            }
        }
    }

    private void Update()
    {
        if (backgrounds == null || backgrounds.Length == 0) return;

        Vector3 movement = Vector3.left * (scrollSpeed * Time.deltaTime);

        // 1. เลื่อนทุกแผ่นไปทางซ้ายพร้อมกัน
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] != null)
            {
                backgrounds[i].position += movement;
            }
        }

        // 2. ตรวจสอบแผ่นที่หลุดขอบซ้าย แล้วย้ายไปต่อท้ายแผ่นที่อยู่ขวาสุด
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] != null && backgrounds[i].position.x <= resetThresholdX)
            {
                Transform rightmost = GetRightmostBackground();

                // ย้ายไปต่อท้ายแผ่นที่อยู่ขวาสุด
                backgrounds[i].position = new Vector3(
                    rightmost.position.x + bgWidth - seamOffset,
                    backgrounds[i].position.y,
                    backgrounds[i].position.z
                );
            }
        }
    }

    // ฟังก์ชันค้นหาแผ่นที่อยู่ตำแหน่งขวาสุดในปัจจุบัน
    private Transform GetRightmostBackground()
    {
        Transform rightmost = backgrounds[0];
        for (int i = 1; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] != null && backgrounds[i].position.x > rightmost.position.x)
            {
                rightmost = backgrounds[i];
            }
        }
        return rightmost;
    }
}