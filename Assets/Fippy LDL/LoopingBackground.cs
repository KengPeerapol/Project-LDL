using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    [Header("Background Sprites")]
    public Transform bg1;
    public Transform bg2;

    [Header("Scroll Settings")]
    [Tooltip("ความเร็วในการเลื่อนไปทางซ้าย")]
    public float scrollSpeed = 3f;

    private float bgWidth;

    private void Start()
    {
        // คำนวณความกว้างจริงของ Sprite แผ่นแรก
        SpriteRenderer sr = bg1.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bgWidth = sr.bounds.size.x;
        }
        else
        {
            bgWidth = 20f;
        }

        // จัดตำแหน่งแผ่นที่ 2 ให้ต่อท้ายแผ่นแรกไปทางขวาพอดี
        bg2.position = new Vector3(bg1.position.x + bgWidth, bg1.position.y, bg1.position.z);
    }

    private void Update()
    {
        // เลื่อนทั้ง 2 แผ่นไปทางซ้ายตามเวลา
        Vector3 movement = Vector3.left * (scrollSpeed * Time.deltaTime);
        bg1.position += movement;
        bg2.position += movement;

        // เมื่อแผ่นที่ 1 เลื่อนหลุดขอบจอซ้าย ให้ย้ายไปต่อท้ายแผ่นที่ 2
        if (bg1.position.x <= -bgWidth)
        {
            bg1.position = new Vector3(bg2.position.x + bgWidth, bg1.position.y, bg1.position.z);
        }

        // เมื่อแผ่นที่ 2 เลื่อนหลุดขอบจอซ้าย ให้ย้ายไปต่อท้ายแผ่นที่ 1
        if (bg2.position.x <= -bgWidth)
        {
            bg2.position = new Vector3(bg1.position.x + bgWidth, bg2.position.y, bg2.position.z);
        }
    }
}