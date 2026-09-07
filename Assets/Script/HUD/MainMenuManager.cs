using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // ฟังก์ชันสำหรับปุ่ม Play
    public void PlayGame()
    {
        // โหลด Scene เกมหลัก (ต้องพิมพ์ชื่อ Scene เกมของคุณให้ตรงเป๊ะ เช่น "SampleScene")
        SceneManager.LoadScene("SampleScene");
    }

    // ฟังก์ชันสำหรับปุ่ม Quit
    public void QuitGame()
    {
        Debug.Log("ออกจากเกม!");
        Application.Quit(); // คำสั่งนี้จะทำงานตอน Build เกมเป็นไฟล์ .exe แล้วเท่านั้น
    }
}