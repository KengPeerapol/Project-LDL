using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("หน้าต่าง Pause Panel ที่รวมปุ่ม Resume, Restart, MainMenu")]
    public GameObject pausePanel;

    [Tooltip("ปุ่มไอคอน Pause ที่อยู่บนจอระหว่างเล่น")]
    public GameObject pauseButtonOnScreen;

    [Header("End Game Panels (ป้องกันการกด Pause ตอนจบเกม)")]
    [Tooltip("ลาก WinPanel มาใส่ในช่องนี้")]
    public GameObject winPanel;

    [Tooltip("ลาก GameOverPanel มาใส่ในช่องนี้")]
    public GameObject gameOverPanel;

    public static bool isGamePaused = false;
    public static bool isGameOver = false;

    private void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isGamePaused = false;
        isGameOver = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (pauseButtonOnScreen != null)
        {
            pauseButtonOnScreen.SetActive(true);
        }
    }

    private void Update()
    {
        if (isGameOver || IsEndGameActive())
        {
            if (pauseButtonOnScreen != null && pauseButtonOnScreen.activeSelf)
            {
                pauseButtonOnScreen.SetActive(false);
            }
            return;
        }

        bool escPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        if (escPressed)
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private bool IsEndGameActive()
    {
        bool isWin = winPanel != null && winPanel.activeInHierarchy;
        bool isOver = gameOverPanel != null && gameOverPanel.activeInHierarchy;
        return isWin || isOver;
    }

    public void PauseGame()
    {
        if (isGameOver || IsEndGameActive()) return;

        isGamePaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButtonOnScreen != null) pauseButtonOnScreen.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButtonOnScreen != null) pauseButtonOnScreen.SetActive(true);

        Cursor.visible = false;
        AudioListener.pause = false;
    }

    // ==========================================
    // ปรับแก้ 2 ฟังก์ชันนี้ให้เรียกใช้ Coroutine หน่วงเวลา
    // ==========================================
    public void RestartGame()
    {
        StartCoroutine(DelayedRestart());
    }

    public void LoadMainMenu()
    {
        StartCoroutine(DelayedMainMenu());
    }

    // ==========================================
    // ชุดคำสั่งใหม่สำหรับหน่วงเวลาก่อนเปลี่ยนฉาก
    // ==========================================
    private IEnumerator DelayedRestart()
    {
        // 1. คืนค่าเวลาก่อน เพื่อให้ระบบแอนิเมชันหรือเสียงที่พึ่งพา TimeScale ทำงานได้ปกติ
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        AudioListener.pause = false; // ต้องปลดล็อก AudioListener ด้วย เสียงจะได้ดัง

        // 2. หน่วงเวลา 0.3 วินาทีให้เสียงคลิกดังจบ
        yield return new WaitForSecondsRealtime(0.3f);

        // 3. รีเซ็ตสถานะและเปลี่ยนฉาก
        isGamePaused = false;
        isGameOver = false;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator DelayedMainMenu()
    {
        // 1. คืนค่าเวลาและปลดล็อกเสียงก่อน
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        AudioListener.pause = false;

        // 2. หน่วงเวลา 0.3 วินาทีให้เสียงคลิกดังจบ
        yield return new WaitForSecondsRealtime(0.3f);

        // 3. รีเซ็ตสถานะและไปหน้าเมนู
        isGamePaused = false;
        isGameOver = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainMenu");
    }
}