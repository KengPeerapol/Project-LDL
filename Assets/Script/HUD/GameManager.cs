using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นต้องมีสำหรับใช้คำสั่งโหลด Scene

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // เริ่มเกมมาให้เวลาเดินปกติ และซ่อนหน้า Game Over
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void GameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // โชว์หน้า Game Over
        }
        Time.timeScale = 0f; // หยุดเวลาในเกม (ศัตรูจะหยุดเดิน)
    }

    // ฟังก์ชันนี้จะเอาไปผูกกับปุ่ม Restart
    public void RestartGame()
    {
        Time.timeScale = 1f; // ให้เวลาเดินปกติก่อนรีสตาร์ท
        // โหลด Scene ปัจจุบันใหม่ทั้งหมด
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}