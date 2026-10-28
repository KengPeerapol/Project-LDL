using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button level2Button;
    public Button level3Button;

    void Start()
    {
        // อ่านค่าระดับด่านสูงสุดที่ผ่านแล้ว (ถ้าเพิ่งเล่นครั้งแรก ค่าเริ่มต้นจะเป็น 1)
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

        // ด่าน 1 กดได้เสมอ
        // ด่าน 2 จะกดได้ก็ต่อเมื่อ reachedLevel >= 2
        if (level2Button != null)
        {
            level2Button.interactable = (reachedLevel >= 2);
        }

        // ด่าน 3 จะกดได้ก็ต่อเมื่อ reachedLevel >= 3
        if (level3Button != null)
        {
            level3Button.interactable = (reachedLevel >= 3);
        }
    }

    // ฟังก์ชันสำหรับเรียกโหลด Scene ด่าน
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    // (ออฟชันเสริม) ปุ่ม Reset ความคืบหน้าสำหรับทดสอบระบบ
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("ReachedLevel");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
