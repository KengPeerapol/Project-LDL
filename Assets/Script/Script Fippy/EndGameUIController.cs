using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameUIController : MonoBehaviour
{
    private void Start()
    {
        HideCursor();
    }

    private void OnEnable()
    {
        ShowCursor();

        // ⭐ ตั้งสถานะว่าจบเกมแล้ว และสั่งปิดหน้าต่าง/ปุ่ม Pause ทันทีหากเปิดค้างอยู่
        PauseMenuManager.isGameOver = true;

        PauseMenuManager pauseManager = Object.FindFirstObjectByType<PauseMenuManager>();
        if (pauseManager != null)
        {
            if (pauseManager.pausePanel != null) pauseManager.pausePanel.SetActive(false);
            if (pauseManager.pauseButtonOnScreen != null) pauseManager.pauseButtonOnScreen.SetActive(false);
        }
    }

    public static void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame()
    {
        Debug.Log("<color=cyan>[UI] เริ่มด่านใหม่: ซ่อนเมาส์และคืนค่าเวลา</color>");

        PauseMenuManager.isGameOver = false;
        HideCursor();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu(string sceneName = "MainMenu")
    {
        PauseMenuManager.isGameOver = false;
        ShowCursor();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}