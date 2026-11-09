using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PatientInfoPanel : MonoBehaviour
{
    [System.Serializable]
    public class TabData
    {
        public string tabName = "Normal Mode";
        [Tooltip("รูปผู้ป่วย (ฝั่งซ้าย)")]
        public Sprite patientSprite;
        [Tooltip("ชื่อผู้ป่วย")]
        public string patientName = "John Doe";
        [TextArea(4, 10)]
        [Tooltip("รายละเอียดข้อมูลผู้ป่วย/อาการ")]
        public string patientDescription;
        [Tooltip("ชื่อ Scene ที่จะโหลดเมื่อกด Play (เช่น Level 1)")]
        public string sceneToLoad = "Level 1";
    }

    [Header("Window References")]
    [Tooltip("ตัวหน้าต่างหลัก (ใช้เปิด/ปิด)")]
    public GameObject windowRoot;

    [Header("Top Tabs")]
    public List<Button> tabButtons = new List<Button>();
    public List<TabData> tabDataList = new List<TabData>();

    [Header("Left Side (ฝั่งซ้าย)")]
    public Image patientImage;
    public TextMeshProUGUI patientNameText;

    [Header("Right Side (ฝั่งขวา)")]
    public TextMeshProUGUI patientInfoText;

    [Header("Buttons")]
    public Button playButton;
    public Button closeButton;

    [Header("Tab Colors")]
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = new Color(0.75f, 0.75f, 0.75f, 0.7f);

    private int currentTabIndex = 0;

    private void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseWindow);

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            if (tabButtons[i] != null)
            {
                tabButtons[i].onClick.AddListener(() => SwitchTab(index));
            }
        }
    }

    private void Start()
    {
        // ซ่อนหน้าต่างไว้ตอนเริ่มฉาก
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }
    }

    public void OpenWindow(int defaultTab = 0)
    {
        if (windowRoot != null) windowRoot.SetActive(true);
        SwitchTab(defaultTab);
    }

    public void CloseWindow()
    {
        if (windowRoot != null) windowRoot.SetActive(false);
    }

    public void SwitchTab(int tabIndex)
    {
        if (tabDataList == null || tabDataList.Count == 0) return;

        currentTabIndex = Mathf.Clamp(tabIndex, 0, tabDataList.Count - 1);
        TabData data = tabDataList[currentTabIndex];

        // 1. อัปเดตรูปผู้ป่วย
        if (patientImage != null)
        {
            patientImage.sprite = data.patientSprite;
            patientImage.enabled = (data.patientSprite != null);
        }

        // 2. อัปเดตชื่อผู้ป่วย
        if (patientNameText != null)
        {
            patientNameText.text = data.patientName;
        }

        // 3. อัปเดตข้อความข้อมูล
        if (patientInfoText != null)
        {
            patientInfoText.text = data.patientDescription;
        }

        // 4. สลับสีแท็บที่กดเลือก
        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (tabButtons[i] != null)
            {
                Image btnImg = tabButtons[i].GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = (i == currentTabIndex) ? activeTabColor : inactiveTabColor;
                }
            }
        }
    }

    private void OnPlayClicked()
    {
        if (currentTabIndex < tabDataList.Count)
        {
            string sceneName = tabDataList[currentTabIndex].sceneToLoad;
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("[PatientInfoPanel] ยังไม่ได้ใส่ชื่อ Scene ในช่อง Scene To Load!");
            }
        }
    }
}