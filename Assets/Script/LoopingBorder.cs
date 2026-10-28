using UnityEngine;

public class LoopingBorder : MonoBehaviour
{
    [Header("ชิ้นส่วนขอบฉาก (ใส่ 2 แผ่นเพื่อต่อวนลูป)")]
    public Transform border1;
    public Transform border2;

    [Header("การเลื่อน")]
    [Tooltip("ความเร็วในการเลื่อนไปทางซ้าย")]
    public float scrollSpeed = 3f;

    [Tooltip("ชดเชยรอยต่อ (หากเห็นเส้นช่องว่างระหว่าง 2 แผ่น ให้ปรับเป็น 0.02 - 0.05)")]
    public float seamOffset = 0f;

    private float spriteWidth;
    private float startPosX;

    private void Start()
    {
        if (border1 == null) border1 = transform;

        startPosX = border1.position.x;

        // คำนวณความกว้างจริงของ Sprite แผ่นแรกอัตโนมัติ
        SpriteRenderer sr = border1.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteWidth = sr.bounds.size.x;
        }
        else
        {
            spriteWidth = 20f;
        }

        // จัดแผ่นที่ 2 ให้ต่อท้ายแผ่นแรกทางขวาพอดี
        if (border2 != null)
        {
            border2.position = new Vector3(startPosX + spriteWidth - seamOffset, border1.position.y, border1.position.z);
        }
    }

    private void Update()
    {
        if (border1 == null || border2 == null) return;

        // เลื่อนทั้ง 2 แผ่นไปทางซ้ายพร้อมกัน
        float moveDistance = scrollSpeed * Time.deltaTime;
        border1.position += Vector3.left * moveDistance;
        border2.position += Vector3.left * moveDistance;

        // เมื่อแผ่นที่ 1 เลื่อนเลยพิกัดเดิมไป 1 ช่วงตัว ให้ย้ายไปต่อท้ายแผ่นที่ 2
        if (border1.position.x <= startPosX - spriteWidth)
        {
            border1.position = new Vector3(border2.position.x + spriteWidth - seamOffset, border1.position.y, border1.position.z);
        }

        // เมื่อแผ่นที่ 2 เลื่อนเลยพิกัดเดิมไป 1 ช่วงตัว ให้ย้ายไปต่อท้ายแผ่นที่ 1
        if (border2.position.x <= startPosX - spriteWidth)
        {
            border2.position = new Vector3(border1.position.x + spriteWidth - seamOffset, border2.position.y, border2.position.z);
        }
    }
}