using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameUIController : MonoBehaviour
{
    private void Start()
    {
        // ⭐ ซ่อนเมาส์ทันทีเมื่อเริ่มเกม / โหลดฉากใหม่ เพื่อให้ใช้เป้าเล็ง Crosshair ในเกม
        HideCursor();
    }

    private void OnEnable()
    {
        // ⭐ เปิดเมาส์ทันทีเมื่อวัตถุนี้ถูกเปิดขึ้นมา (กรณีนำสคริปต์ไปแปะไว้ที่ Panel)
        ShowCursor();
    }

    // ฟังก์ชันเปิดการแสดงผลเมาส์
    public static void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // ฟังก์ชันซ่อนเมาส์
    public static void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    // ⭐ สำหรับปุ่ม Retry / Play Again (เริ่มเล่นใหม่)
    public void RestartGame()
    {
        Debug.Log("<color=cyan>[UI] เริ่มด่านใหม่: ซ่อนเมาส์และคืนค่าเวลา</color>");

        HideCursor(); // 1. ซ่อนเมาส์ทันทีที่กดปุ่ม
        Time.timeScale = 1f; // 2. ปลดล็อกเวลาเกม
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 3. โหลดซีนใหม่
    }

    // ⭐ สำหรับปุ่มกลับหน้าเมนูหลัก
    public void GoToMainMenu(string sceneName = "MainMenu")
    {
        ShowCursor(); // เปิดเมาส์ไว้สำหรับหน้าเลือกเมนู
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ⭐ สำหรับปุ่มออกจากเกม
    public void QuitGame()
    {
        Application.Quit();
    }
}