using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ⭐ จำเป็นสำหรับการสั่งโหลดเปลี่ยน Scene
using TMPro;

public class PatientSystemManager : MonoBehaviour
{
    [System.Serializable]
    public class PatientData
    {
        [Tooltip("ชื่อ-นามสกุลของผู้ป่วย")]
        public string patientName;

        [Tooltip("รูปร่าง/ใบหน้าของผู้ป่วย (ลากไฟล์รูปภาพ Sprite มาใส่)")]
        public Sprite patientAvatar;

        [Tooltip("ชื่อ Scene ด่านที่จะเล่นสำหรับคนนี้ (พิมพ์ชื่อให้ตรงกับในโปรเจกต์)")]
        public string targetSceneName = "FippyLDL"; // ⭐ ชื่อ Scene ด่านประจำตัว

        [TextArea(4, 8)]
        [Tooltip("ประวัติทั่วไปในกล่องสีฟ้าขวา (อายุ วันเกิด อาชีพ ฯลฯ)")]
        public string generalProfile;

        [TextArea(3, 6)]
        [Tooltip("บทพูดแนะนำตัวเมื่อกดปุ่มชื่อ (แสดงในกล่องเขียวซ้าย)")]
        public string introductionSpeech;

        [TextArea(3, 6)]
        [Tooltip("ข้อมูลวิเคราะห์ความเสี่ยงด้านไขมันและการกิน (แสดงในกล่องเขียวซ้าย)")]
        public string lipidRiskSpeech;
    }

    [Header("UI Displays (หน้าจอแสดงผล)")]
    [Tooltip("Text ในกล่องสีเขียวทางซ้าย")]
    public TextMeshProUGUI speechBoxText;

    [Tooltip("Text ในกล่องสีฟ้าทางขวา")]
    public TextMeshProUGUI profileBoxText;

    [Tooltip("Text บนปุ่มชื่อตรงกลางล่าง")]
    public TextMeshProUGUI nameButtonText;

    [Tooltip("คอมโพเนนต์ Image ตรงกลางสำหรับแสดงรูปคน")]
    public Image avatarDisplayImage;

    [Header("Patient Database")]
    public PatientData[] patients;

    private int currentIndex = 0;

    private void Start()
    {
        if (patients == null || patients.Length == 0)
        {
            SetupDefaultPatients();
        }

        UpdatePatientDisplay();
    }

    private void SetupDefaultPatients()
    {
        patients = new PatientData[2];

        // ผู้ป่วยคนที่ 1 -> ด่าน FippyLDL
        patients[0] = new PatientData
        {
            patientName = "นายสมชาย ชาญวิทย์",
            targetSceneName = "FippyLDL", // ⭐ เชื่อมกับด่าน FippyLDL
            generalProfile = "<b>ประวัติผู้ป่วย:</b>\n" +
                             "• ชื่อ: นายสมชาย ชาญวิทย์\n" +
                             "• อายุ: 54 ปี\n" +
                             "• วันเกิด: 14 ก.พ. 2515\n" +
                             "• อาชีพ: พนักงานขับรถบรรทุก\n" +
                             "• น้ำหนัก/ส่วนสูง: 86 กก. / 168 ซม.\n" +
                             "• ความดันโลหิต: 145/92 mmHg",
            introductionSpeech = "สวัสดีครับคุณหมอ ช่วงนี้ผมรู้สึกเหนื่อยง่าย แน่นหน้าอกบ่อยๆ โดยเฉพาะเวลาทำงานขับรถดึกๆ ครับ",
            lipidRiskSpeech = "<b>[วิเคราะห์ความเสี่ยงด้านไขมัน]:</b>\n" +
                              "• ผลตรวจคอเลสเตอรอลรวมสูง 260 mg/dL (LDL สูง)\n" +
                              "• พฤติกรรมเสี่ยง: ชอบทานของทอด แกงกะทิ และอาหารฟาสต์ฟู้ดระหว่างขับรถ\n" +
                              "• ความเสี่ยง: มีภาวะไขมันในเลือดสูง เสี่ยงต่อหลอดเลือดหัวใจตีบเฉียบพลัน"
        };

        // ผู้ป่วยคนที่ 2 -> ด่าน TEST
        patients[1] = new PatientData
        {
            patientName = "นางวรรณา สว่างจิต",
            targetSceneName = "TEST", // ⭐ เชื่อมกับด่าน TEST
            generalProfile = "<b>ประวัติผู้ป่วย:</b>\n" +
                             "• ชื่อ: นางวรรณา สว่างจิต\n" +
                             "• อายุ: 48 ปี\n" +
                             "• วันเกิด: 22 ก.ย. 2521\n" +
                             "• อาชีพ: เจ้าของร้านเบเกอรี่\n" +
                             "• น้ำหนัก/ส่วนสูง: 68 กก. / 155 ซม.\n" +
                             "• ความดันโลหิต: 130/85 mmHg",
            introductionSpeech = "สวัสดีค่ะคุณหมอ พักนี้มีอาการมึนหัว เวียนศีรษะตอนตื่นนอนบ่อยๆ เลยอยากมาตรวจเช็กร่างกายค่ะ",
            lipidRiskSpeech = "<b>[วิเคราะห์ความเสี่ยงด้านไขมัน]:</b>\n" +
                              "• ระดับไตรกลีเซอไรด์สูงถึง 230 mg/dL\n" +
                              "• พฤติกรรมเสี่ยง: ชิมขนมหวาน เค้กเนยสด และดื่มชานมไข่มุกเป็นประจำทุกวัน\n" +
                              "• ความเสี่ยง: เสี่ยงต่อภาวะไขมันพอกตับ และภาวะหลอดเลือดสมองอุดตัน"
        };
    }

    private void UpdatePatientDisplay()
    {
        if (patients == null || patients.Length == 0) return;

        PatientData current = patients[currentIndex];

        if (avatarDisplayImage != null && current.patientAvatar != null)
        {
            avatarDisplayImage.sprite = current.patientAvatar;
        }

        if (profileBoxText != null)
        {
            profileBoxText.text = current.generalProfile;
        }

        if (nameButtonText != null)
        {
            nameButtonText.text = current.patientName;
        }

        if (speechBoxText != null)
        {
            speechBoxText.text = $"<i>\"{current.introductionSpeech}\"</i>";
        }
    }

    // ⭐ ฟังก์ชันสำหรับปุ่ม PLAY (โหลด Scene ตามคนที่กำลังเลือกอยู่)
    public void PlaySelectedPatientScene()
    {
        if (patients == null || patients.Length == 0) return;

        string sceneToLoad = patients[currentIndex].targetSceneName;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"<color=green>[Scene] กำลังเริ่มด่านของผู้ป่วย: {patients[currentIndex].patientName} -> Scene: {sceneToLoad}</color>");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning($"ยังไม่ได้กำหนดชื่อ Scene สำหรับ {patients[currentIndex].patientName}!");
        }
    }

    public void OnNameButtonClicked()
    {
        if (patients == null || patients.Length == 0) return;

        if (speechBoxText != null)
        {
            speechBoxText.text = $"<b>{patients[currentIndex].patientName}:</b>\n\"{patients[currentIndex].introductionSpeech}\"";
        }
    }

    public void OnRiskInfoButtonClicked()
    {
        if (patients == null || patients.Length == 0) return;

        if (speechBoxText != null)
        {
            speechBoxText.text = patients[currentIndex].lipidRiskSpeech;
        }
    }

    public void NextPatient()
    {
        if (patients == null || patients.Length == 0) return;

        currentIndex = (currentIndex + 1) % patients.Length;
        UpdatePatientDisplay();
    }

    public void PreviousPatient()
    {
        if (patients == null || patients.Length == 0) return;

        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = patients.Length - 1;
        }
        UpdatePatientDisplay();
    }
}