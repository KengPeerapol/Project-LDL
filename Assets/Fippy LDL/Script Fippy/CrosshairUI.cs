using UnityEngine;
using UnityEngine.InputSystem; // ใช้งาน New Input System

public class CrosshairUI : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // ซ่อนลูกศรเมาส์จริงของ Windows
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Mouse.current != null)
        {
            // ให้ตำแหน่งของภาพ UI ขยับตามพิกัดเมาส์บนหน้าจอแบบ 1:1
            rectTransform.position = Mouse.current.position.ReadValue();
        }
    }

    private void OnDisable()
    {
        // คืนค่าให้เห็นเมาส์ปกติเมื่อปิดเกมหรือสคริปต์หยุดทำงาน
        Cursor.visible = true;
    }
}