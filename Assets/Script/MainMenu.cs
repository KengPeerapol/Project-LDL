using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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

    // 4. ฟังก์ชันออกจากเกม (สำหรับปุ่ม Quit)
    public void QuitGame()
    {
        Debug.Log("[Game] ปิดโปรแกรมและออกจากเกม");
        Application.Quit(); // สั่งปิดเกมเมื่อ Build เป็นไฟล์ .exe หรือแอปพลิเคชันจริง

#if UNITY_EDITOR
        // กรณีทดสอบในหน้า Unity Editor คำสั่งนี้จะหยุดโหมด Play ให้อัตโนมัติ
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}