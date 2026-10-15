using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // ฟังก์ชันสำหรับปุ่ม Play
    public void PlayGame()
    {
        // โหลด Scene เกมหลัก (ต้องพิมพ์ชื่อ Scene เกมของคุณให้ตรงเป๊ะ เช่น "SampleScene")
        SceneManager.LoadScene("FippyLDL");
    }

    [Header("UI Panels")]
    public GameObject targetPanel; // ลาก UI Panel มาใส่ใน Inspector

    // 1. ฟังก์ชันสำหรับปุ่ม Icon (เปิด Panel)
    public void OpenPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    // ฟังก์ชันเสริม: สำหรับปิด Panel (ถ้ามีปุ่ม X หรือ Cancel)
    public void ClosePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }

    // ฟังก์ชันสำหรับปุ่ม Quit
    public void QuitGame()
    {
        Debug.Log("ออกจากเกม!");
        Application.Quit(); // คำสั่งนี้จะทำงานตอน Build เกมเป็นไฟล์ .exe แล้วเท่านั้น
    }
}