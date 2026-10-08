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
        isGamePaused = false;
        isGameOver = false;

        // ⭐ 1. ปิดหน้าต่างเมนูและหน้าต่างจบเกมทั้งหมดตอนเริ่มฉาก
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // ⭐ 2. บังคับเปิดปุ่ม Pause บนหน้าจอให้แสดงผลเสมอตอนเริ่มเกม
        if (pauseButtonOnScreen != null)
        {
            pauseButtonOnScreen.SetActive(true);
        }
    }

    private void Update()
    {
        // หากเกมจบแล้ว (ชนะ/แพ้) หรือหน้าต่างผลจบเกมเปิดอยู่ ให้บล็อกและซ่อนปุ่ม Pause ทันที
        if (isGameOver || IsEndGameActive())
        {
            if (pauseButtonOnScreen != null && pauseButtonOnScreen.activeSelf)
            {
                pauseButtonOnScreen.SetActive(false);
            }
            return;
        }

        // ตรวจจับการกดปุ่ม Escape (Esc) บนคีย์บอร์ด
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
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButtonOnScreen != null) pauseButtonOnScreen.SetActive(true);

        Cursor.visible = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        isGameOver = false;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        isGameOver = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainMenu");
    }
}