using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization; // ช่วยจำค่า Reference เดิมใน Inspector
using TMPro;

public class PatientSystemManager : MonoBehaviour
{
    // ⭐ คลาสข้อมูลปุ่มหัวข้อของแต่ละคน
    [System.Serializable]
    public class PatientTopic
    {
        [Tooltip("ชื่อบนปุ่ม (เช่น การกิน, ออกกำลังกาย, สูบบุหรี่)")]
        public string buttonTitle = "หัวข้อ";

        [TextArea(4, 8)]
        [Tooltip("เนื้อหาที่จะแสดงในกล่องข้อความเมื่อคลิกปุ่มนี้")]
        public string detailContent;
    }

    // ⭐ คลาสข้อมูลผู้ป่วย
    [System.Serializable]
    public class PatientData
    {
        [Tooltip("ชื่อ-นามสกุลของผู้ป่วย")]
        public string patientName;

        [Tooltip("รูปภาพประจำตัวผู้ป่วย")]
        public Sprite patientAvatar;

        [Tooltip("ชื่อ Scene ด่านที่จะเล่นสำหรับคนนี้")]
        public string targetSceneName = "FippyLDL";

        [TextArea(3, 6)]
        [Tooltip("บทพูดแนะนำตัวของผู้ป่วย (จะแสดงขึ้นมาก่อนกดปุ่มหัวข้อ)")]
        public string introductionSpeech;

        [TextArea(3, 6)]
        [Tooltip("ประวัติทั่วไปในกล่องบนขวา (อายุ วันเกิด อาชีพ ฯลฯ)")]
        public string generalProfile;

        [Tooltip("รายการปุ่มหัวข้อ (คนแรกใส่ 3 ปุ่ม, คนที่สองใส่ 2 ปุ่ม)")]
        public PatientTopic[] topics;
    }

    [Header("UI Displays (หน้าจอแสดงผล)")]
    [Tooltip("รูปภาพผู้ป่วย")]
    public Image avatarDisplayImage;

    [Tooltip("Text สำหรับแสดงชื่อผู้ป่วย")]
    public TextMeshProUGUI patientNameText;

    [Tooltip("Text กล่องบนขวา (แสดงประวัติ/อายุ/อาชีพ)")]
    public TextMeshProUGUI profileBoxText;

    [FormerlySerializedAs("detailBoxText")]
    [FormerlySerializedAs("speechBoxText")]
    [Tooltip("กล่องข้อความหลัก (แสดงบทแนะนำตัวก่อน พอคลิกปุ่มหัวข้อจะเปลี่ยนเป็นข้อมูล)")]
    public TextMeshProUGUI contentBoxText; // ⭐ รวมเป็นช่องเดียว

    [Header("Topic Buttons (สล็อตปุ่มในฉาก)")]
    [Tooltip("ลากปุ่มที่มีในฉากมาใส่ที่นี่ตามลำดับ (เช่น ปุ่ม 1, 2, 3)")]
    public Button[] topicButtons;

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

        // ----------------- ผู้ป่วยคนที่ 1 (มี 3 ปุ่ม) -----------------
        patients[0] = new PatientData
        {
            patientName = "นายสมชาย ชาญวิทย์",
            targetSceneName = "FippyLDL",
            introductionSpeech = "สวัสดีครับคุณหมอ ผมสมชายครับ ช่วงนี้รู้สึกเหนื่อยง่าย แน่นหน้าอกบ่อยๆ โดยเฉพาะเวลาขับรถส่งของทางไกล เลยแวะมาตรวจดูครับ",
            generalProfile = "<b>ข้อมูลผู้ป่วย:</b>\n" +
                             "• ชื่อ: นายสมชาย ชาญวิทย์ (อายุ 54 ปี)\n" +
                             "• อาชีพ: พนักงานขับรถบรรทุกขนส่ง\n" +
                             "• ความดันโลหิต: 145/92 mmHg (ระดับ 1 Hypertension)",
            topics = new PatientTopic[]
            {
                new PatientTopic
                {
                    buttonTitle = "การกิน",
                    detailContent = "<b>[พฤติกรรมการรับประทานอาหาร]:</b>\n" +
                                    "• ชอบทานของทอด แกงกะทิ ข้าวขาหมูเป็นประจำ\n" +
                                    "• ดื่มกาแฟกระป๋องรสหวานวันละ 2-3 กระป๋องเพื่อแก้ง่วง\n" +
                                    "• ผลตรวจคอเลสเตอรอลรวมสูงถึง 260 mg/dL (ไขมันเลว LDL สูง)"
                },
                new PatientTopic
                {
                    buttonTitle = "ออกกำลังกาย",
                    detailContent = "<b>[พฤติกรรมการออกกำลังกาย]:</b>\n" +
                                    "• แทบไม่ได้ออกกำลังกายเลยเนื่องจากต้องขับรถทางไกล\n" +
                                    "• นั่งอยู่กับที่ติดต่อกันมากกว่า 10-12 ชั่วโมงต่อวัน\n" +
                                    "• มีอาการปวดหลังและเหนื่อยง่ายเวลาเดินขึ้นบันได"
                },
                new PatientTopic
                {
                    buttonTitle = "สูบบุหรี่",
                    detailContent = "<b>[พฤติกรรมการสูบบุหรี่]:</b>\n" +
                                    "• สูบบุหรี่เป็นประจำเฉลี่ยวันละ 1 ซอง (20 มวน)\n" +
                                    "• สูบต่อเนื่องมานานกว่า 25 ปี\n" +
                                    "• ส่งผลให้หลอดเลือดแข็งตัวและมีความเสี่ยงต่อภาวะกล้ามเนื้อหัวใจขาดเลือดสูง"
                }
            }
        };

        // ----------------- ผู้ป่วยคนที่ 2 (มี 2 ปุ่ม) -----------------
        patients[1] = new PatientData
        {
            patientName = "นางวรรณา สว่างจิต",
            targetSceneName = "TEST",
            introductionSpeech = "สวัสดีค่ะคุณหมอ ดิฉันวรรณาค่ะ ช่วงนี้ตื่นนอนมาแล้วมีอาการมึนหัว เวียนศีรษะบ่อยๆ เลยอยากมาตรวจสุขภาพดูค่ะ",
            generalProfile = "<b>ข้อมูลผู้ป่วย:</b>\n" +
                             "• ชื่อ: นางวรรณา สว่างจิต (อายุ 48 ปี)\n" +
                             "• อาชีพ: เจ้าของร้านเบเกอรี่\n" +
                             "• ความดันโลหิต: 130/85 mmHg",
            topics = new PatientTopic[]
            {
                new PatientTopic
                {
                    buttonTitle = "การกิน",
                    detailContent = "<b>[พฤติกรรมการรับประทานอาหาร]:</b>\n" +
                                    "• ชิมเค้กเนยสด ขนมปังหวาน และดื่มชานมไข่มุกทุกวัน\n" +
                                    "• บริโภคน้ำตาลและไขมันทรานส์สะสมในปริมาณสูง\n" +
                                    "• ผลตรวจไตรกลีเซอไรด์สูงถึง 230 mg/dL เสี่ยงต่อไขมันพอกตับ"
                },
                new PatientTopic
                {
                    buttonTitle = "ออกกำลังกาย",
                    detailContent = "<b>[พฤติกรรมการออกกำลังกาย]:</b>\n" +
                                    "• ยืนทำขนมหน้าเตาเกือบทั้งวัน แต่ไม่ได้ออกกำลังกายแบบคาร์ดิโอ\n" +
                                    "• พักผ่อนน้อย นอนดึกตื่นเช้าเพื่อเตรียมวัตถุดิบ\n" +
                                    "• การเผาผลาญไขมันในร่างกายทำงานได้ช้าลงตามวัย"
                }
            }
        };
    }

    private void UpdatePatientDisplay()
    {
        if (patients == null || patients.Length == 0) return;

        PatientData current = patients[currentIndex];

        // 1. เปลี่ยนรูปภาพผู้ป่วย
        if (avatarDisplayImage != null && current.patientAvatar != null)
        {
            avatarDisplayImage.sprite = current.patientAvatar;
        }

        // 2. แสดงชื่อผู้ป่วย
        if (patientNameText != null)
        {
            patientNameText.text = current.patientName;
        }

        // 3. แสดงประวัติในกล่องบนขวา
        if (profileBoxText != null)
        {
            profileBoxText.text = current.generalProfile;
        }

        // 4. ⭐ แสดงบทพูดแนะนำตัวก่อนเสมอในกล่องข้อความ
        ShowIntroduction();

        // 5. จัดการปุ่มหัวข้อ (เปิด/ปิด และผูกคำสั่งคลิก)
        if (topicButtons != null)
        {
            for (int i = 0; i < topicButtons.Length; i++)
            {
                if (topicButtons[i] == null) continue;

                if (i < current.topics.Length)
                {
                    topicButtons[i].gameObject.SetActive(true);

                    TextMeshProUGUI btnText = topicButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        btnText.text = current.topics[i].buttonTitle;
                    }

                    int topicIndex = i;
                    topicButtons[i].onClick.RemoveAllListeners();
                    topicButtons[i].onClick.AddListener(() =>
                    {
                        ShowDetail(current.topics[topicIndex].detailContent);
                    });
                }
                else
                {
                    topicButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // ⭐ แสดงบทพูดแนะนำตัวในกล่องหลัก
    public void ShowIntroduction()
    {
        if (patients == null || patients.Length == 0) return;
        PatientData current = patients[currentIndex];

        if (contentBoxText != null)
        {
            contentBoxText.text = $"<b>{current.patientName}:</b>\n\"{current.introductionSpeech}\"";
        }
    }

    // ⭐ แสดงรายละเอียดหัวข้อเมื่อกดปุ่ม (การกิน / ออกกำลังกาย / สูบบุหรี่)
    public void ShowDetail(string content)
    {
        if (contentBoxText != null)
        {
            contentBoxText.text = content;
        }
    }

    // ⭐ ปุ่ม PLAY
    public void PlaySelectedPatientScene()
    {
        if (patients == null || patients.Length == 0) return;

        string sceneToLoad = patients[currentIndex].targetSceneName;
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // ⭐ ปุ่มลูกศรขวา (คนถัดไป)
    public void NextPatient()
    {
        if (patients == null || patients.Length == 0) return;

        currentIndex = (currentIndex + 1) % patients.Length;
        UpdatePatientDisplay();
    }

    // ⭐ ปุ่มลูกศรซ้าย (คนก่อนหน้า)
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