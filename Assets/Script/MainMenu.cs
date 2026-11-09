using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // 1. ฟังก์ชันกดปุ่ม PLAY หน้า Main Menu เพื่อไปหน้า Level Select
    public void GoToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect"); // ระบุชื่อ Scene หน้าเลือกด่าน
    }

    // 2. ฟังก์ชันเลือกด่าน (ส่งชื่อ Scene ด่านเข้าผ่าน Button OnClick)
    public void LoadLevelByName(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    // 3. ฟังก์ชันย้อนกลับไปหน้า Main Menu (สำหรับปุ่ม Back)
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