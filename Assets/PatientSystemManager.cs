using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PatientSystemManager : MonoBehaviour
{
    // ⭐ ข้อมูลหัวข้อพฤติกรรมของแต่ละคนไข้
    [System.Serializable]
    public class PatientTopic
    {
        [Tooltip("ชื่อบนปุ่ม (เช่น การกิน, สูบบุหรี่)")]
        public string buttonTitle = "หัวข้อ";

        [TextArea(4, 8)]
        [Tooltip("เนื้อหาพฤติกรรมที่จะแสดงในกล่องเขียวเมื่อกดปุ่มนี้")]
        public string detailContent;

        [Tooltip("ลาก Sprite คนสีเทาท่าทางเฉพาะของหัวข้อนี้มาใส่")]
        public Sprite topicNpcSprite;
    }

    // ⭐ ข้อมูลผู้ป่วยแต่ละคน (สามารถกรอกและลากรูปใส่ใน Inspector ได้เลย)
    [System.Serializable]
    public class PatientData
    {
        [Tooltip("ชื่อ-นามสกุลของผู้ป่วย")]
        public string patientName;

        [Tooltip("รูปภาพประจำตัวผู้ป่วย (กล่องซ้ายบน)")]
        public Sprite patientAvatar;

        [Tooltip("ชื่อ Scene ด่านที่จะเล่นสำหรับคนนี้")]
        public string targetSceneName = "FippyLDL";

        [TextArea(3, 6)]
        [Tooltip("บทพูดแนะนำตัวของผู้ป่วย (จะขึ้นในกล่องเขียวก่อนกดปุ่ม)")]
        public string introductionSpeech;

        [Tooltip("รูปคนสีเทาตอนแนะนำตัว (ถ้าไม่มีจะใช้ Default Npc Sprite)")]
        public Sprite patientIntroNpcSprite;

        [TextArea(3, 6)]
        [Tooltip("ประวัติทั่วไปในกล่องฟ้า")]
        public string generalProfile;

        [Tooltip("รายการปุ่มหัวข้อของคนนี้")]
        public PatientTopic[] topics;
    }

    [Header("UI Displays (หน้าจอคนไข้)")]
    [Tooltip("รูปภาพคนไข้มุมซ้ายบน")]
    public Image avatarDisplayImage; // ⭐ Profile (Image)

    [Tooltip("Text ป้ายชื่อคนไข้ (มุมขวาบน)")]
    public TextMeshProUGUI patientNameText; // ⭐ Name Button Text

    [Tooltip("Text ในกล่องฟ้า (แสดงประวัติทั่วไป)")]
    public TextMeshProUGUI profileDetailText; // ⭐ Profile Box Text

    [Header("NPC & Speech (คนสีเทาและกล่องเขียวด้านขวา)")]
    [Tooltip("คอมโพเนนต์ Image ของตัวละครคนสีเทา")]
    public Image npcImage; // ⭐ NPC (Image)

    [Tooltip("รูป Sprite เริ่มต้นของคนสีเทา (ท่ายิ้มชี้มือเดิม)")]
    public Sprite defaultNpcSprite; // ⭐ Npc_0

    [Tooltip("Text ในกล่องสีเขียวทางขวามือ (แสดงบทพูดและพฤติกรรม)")]
    public TextMeshProUGUI speechBoxText; // ⭐ Speech Box Text

    [TextArea(3, 5)]
    [Tooltip("ข้อความเริ่มต้นของกล่องเขียวเมื่อปิดหน้าต่าง Panel")]
    public string defaultSpeechText = "ยินดีต้อนรับเข้าสู่ ArterySavior กรุณาเลือกเปิดเคสผู้ป่วยเพื่อทำการวินิจฉัย";

    [Header("Topic Buttons (สล็อตปุ่มในกล่องฟ้า)")]
    [Tooltip("ลากปุ่มหัวข้อ (Eat, Smoking ฯลฯ) มาใส่ตามลำดับ")]
    public Button[] topicButtons;

    [Header("Patient Database")]
    public PatientData[] patients;

    private int currentIndex = 0;

    private void Start()
    {
        // สร้างข้อมูลเริ่มต้นให้อัตโนมัติ เฉพาะกรณีที่ใน Inspector ยังไม่ได้เพิ่มข้อมูลไว้
        if (patients == null || patients.Length == 0)
        {
            SetupDefaultPatients();
        }
    }

    private void OnEnable()
    {
        UpdatePatientDisplay();
    }

    private void OnDisable()
    {
        ResetToDefaultMenuState();
    }

    private void SetupDefaultPatients()
    {
        patients = new PatientData[2];

        // ผู้ป่วยคนที่ 1
        patients[0] = new PatientData
        {
            patientName = "เก่ง ขยี้หนม",
            targetSceneName = "FippyLDL",
            introductionSpeech = "สวัสดีครับคุณหมอ ผมเก่ง ขยี้หนมครับ ช่วงนี้รู้สึกเหนื่อยง่าย แน่นหน้าอกบ่อยๆ โดยเฉพาะเวลาขับรถส่งของทางไกล เลยแวะมาตรวจดูครับ",
            generalProfile = "<b>ข้อมูลผู้ป่วย:</b>\n" +
                             "• ชื่อ: นายสมชาย ชาญวิทย์ (อายุ 54 ปี)\n" +
                             "• อาชีพ: พนักงานขับรถบรรทุกขนส่ง\n" +
                             "• ความดันโลหิต: 145/92 mmHg (ระดับ 1 Hypertension)",
            topics = new PatientTopic[]
            {
                new PatientTopic
                {
                    buttonTitle = "การกิน",
                    detailContent = "<b>[พฤติกรรมการรับประทานอาหาร]:</b>\n• ชอบทานของทอด แกงกะทิ ข้าวขาหมูเป็นประจำ\n• ดื่มกาแฟหวานจัดวันละหลายกระป๋องเพื่อแก้ง่วง\n• คอเลสเตอรอลรวมสูง 260 mg/dL"
                },
                new PatientTopic
                {
                    buttonTitle = "สูบบุหรี่",
                    detailContent = "<b>[พฤติกรรมการสูบบุหรี่]:</b>\n• สูบบุหรี่เป็นประจำเฉลี่ยวันละ 1 ซอง (20 มวน)\n• สูบต่อเนื่องมานานกว่า 25 ปี\n• หลอดเลือดแดงแข็งตัวและเสี่ยงต่อภาวะกล้ามเนื้อหัวใจขาดเลือดสูง"
                }
            }
        };

        // ผู้ป่วยคนที่ 2
        patients[1] = new PatientData
        {
            patientName = "วรรณา เบเกอรี่",
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
                    detailContent = "<b>[พฤติกรรมการรับประทานอาหาร]:</b>\n• ชิมเค้กเนยสด ขนมปังหวาน และดื่มชานมไข่มุกทุกวัน\n• ไตรกลีเซอไรด์สูงถึง 230 mg/dL เสี่ยงต่อไขมันพอกตับ"
                },
                new PatientTopic
                {
                    buttonTitle = "ออกกำลังกาย",
                    detailContent = "<b>[พฤติกรรมการออกกำลังกาย]:</b>\n• ยืนทำขนมหน้าเตาเกือบทั้งวัน แทบไม่ได้ออกกำลังกายแบบคาร์ดิโอ\n• ร่างกายเผาผลาญไขมันช้าลง"
                }
            }
        };
    }

    public void UpdatePatientDisplay()
    {
        if (patients == null || patients.Length == 0) return;
        PatientData current = patients[currentIndex];

        // 1. เปลี่ยนรูปคนไข้ซ้ายบน
        if (avatarDisplayImage != null && current.patientAvatar != null)
            avatarDisplayImage.sprite = current.patientAvatar;

        // 2. แสดงชื่อคนไข้
        if (patientNameText != null)
            patientNameText.text = current.patientName;

        // 3. แสดงประวัติทั่วไปในกล่องฟ้า
        if (profileDetailText != null)
            profileDetailText.text = current.generalProfile;

        // 4. แสดงบทพูดแนะนำตัวก่อนในกล่องเขียว
        if (speechBoxText != null)
            speechBoxText.text = $"<b>{current.patientName}:</b>\n\"{current.introductionSpeech}\"";

        // 5. เปลี่ยนรูปคนสีเทาเริ่มต้น
        if (npcImage != null)
        {
            if (current.patientIntroNpcSprite != null)
                npcImage.sprite = current.patientIntroNpcSprite;
            else if (defaultNpcSprite != null)
                npcImage.sprite = defaultNpcSprite;
        }

        // 6. ผูกปุ่มหัวข้อ
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
                        OnTopicButtonClicked(current.topics[topicIndex]);
                    });
                }
                else
                {
                    topicButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // ⭐ เมื่อกดปุ่มหัวข้อ (การกิน / สูบบุหรี่)
    public void OnTopicButtonClicked(PatientTopic topic)
    {
        // 1. ส่งข้อความพฤติกรรมไปแสดงในกล่องเขียว
        if (speechBoxText != null)
        {
            speechBoxText.text = topic.detailContent;
        }

        // 2. เปลี่ยนรูปคนสีเทาตามหัวข้อที่คลิก (ถ้ามีใส่รูปไว้)
        if (npcImage != null && topic.topicNpcSprite != null)
        {
            npcImage.sprite = topic.topicNpcSprite;
        }
    }

    // คืนค่าหน้าต่างเมื่อปิด Panel
    public void ResetToDefaultMenuState()
    {
        if (speechBoxText != null)
            speechBoxText.text = defaultSpeechText;

        if (npcImage != null && defaultNpcSprite != null)
            npcImage.sprite = defaultNpcSprite;
    }

    public void PlaySelectedPatientScene()
    {
        if (patients == null || patients.Length == 0) return;
        string sceneToLoad = patients[currentIndex].targetSceneName;
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
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
        if (currentIndex < 0) currentIndex = patients.Length - 1;
        UpdatePatientDisplay();
    }
}