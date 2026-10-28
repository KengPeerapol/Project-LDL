using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // เรียกฟังก์ชันนี้เมื่อผู้เล่นเล่นชนะในด่านนั้นๆ
    public void WinLevel(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

        // ถ้าด่านถัดไปสูงกว่าด่านสูงสุดที่เคยบันทึกไว้ ให้ทำการอัปเดตค่า
        if (nextLevel > reachedLevel)
        {
            PlayerPrefs.SetInt("ReachedLevel", nextLevel);
            PlayerPrefs.Save(); // บันทึกข้อมูลลงเครื่อง
        }

        // โหลดกลับไปหน้าเลือกด่าน หรือไปด่านถัดไป
        SceneManager.LoadScene("LevelSelect");
    }
}