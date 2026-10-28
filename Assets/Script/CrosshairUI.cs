using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairUI : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Cursor.visible = false;
    }

    private void Update()
    {
        // ตรวจสอบว่าเกมหยุดเวลา (Pause) หรืออยู่ในหน้าจบเกมหรือไม่
        bool isPaused = Time.timeScale <= 0f || PauseMenuManager.isGamePaused || PauseMenuManager.isGameOver;

        if (isPaused)
        {
            // แสดงลูกศรเมาส์ปกติของระบบเพื่อใช้คลิกปุ่ม
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // ข้ามการอัปเดตพิกัด เพื่อให้ภาพเป้า Crosshair ล็อกค้างอยู่ที่เดิมตอนกด Pause
            return;
        }

        // เมื่อเล่นเกมตามปกติ: ซ่อนลูกศรเมาส์ และให้ Crosshair ขยับตามพิกัดเมาส์
        Cursor.visible = false;

        if (Mouse.current != null)
        {
            rectTransform.position = Mouse.current.position.ReadValue();
        }
    }

    private void OnDisable()
    {
        // คืนค่าให้เห็นเมาส์ปกติเมื่อปิดเกมหรือสลับฉาก
        Cursor.visible = true;
    }
}