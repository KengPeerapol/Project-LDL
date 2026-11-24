using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("ลาก GameObject ของ Panel Tutorial ใน Hierarchy มาใส่ช่องนี้")]
    public GameObject tutorialPanel;

    public void GoToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadLevelByName(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ==================== Tutorial Panel Control ====================

    // ⭐ ฟังก์ชันกดเปิด Panel Tutorial (สำหรับปุ่ม How to play / Tutorial)
    public void OpenTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    // ⭐ ฟังก์ชันกดปิด Panel Tutorial (สำหรับปุ่ม X หรือปุ่ม Close)
    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    // ⭐ ฟังก์ชันเปิด/ปิด แบบรับค่า GameObject โดยตรง (ยืดหยุ่นใช้กับ Panel อื่นๆ ได้ด้วย)
    public void OpenPanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(false);
    }

    // ================================================================

    // ฟังก์ชันออกจากเกม (สำหรับปุ่ม Quit)
    public void QuitGame()
    {
        Debug.Log("[Game] ปิดโปรแกรมและออกจากเกม");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}