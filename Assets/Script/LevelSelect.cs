using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;

    [Header("Patient Info Window")]
    public PatientInfoPanel patientInfoPanel;

    private void Start()
    {
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 1);

        if (level2Button != null) level2Button.interactable = (reachedLevel >= 2);
        if (level3Button != null) level3Button.interactable = (reachedLevel >= 3);

        // ผูก Event ให้ปุ่มเลข 1 สั่งเปิดหน้าต่างข้อมูลผู้ป่วย
        if (level1Button != null)
        {
            level1Button.onClick.AddListener(OnLevel1Clicked);
        }
    }

    public void OnLevel1Clicked()
    {
        if (patientInfoPanel != null)
        {
            patientInfoPanel.OpenWindow(0);
        }
        else
        {
            LoadLevel("Level 1");
        }
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    // เพิ่มฟังก์ชันนี้ลงในคลาส LevelSelect
    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("ReachedLevel");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}