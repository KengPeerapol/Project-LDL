using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StarResultManager : MonoBehaviour
{
    private static StarResultManager _instance;

    public static StarResultManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<StarResultManager>(FindObjectsInactive.Include);
            }
            return _instance;
        }
    }

    [Header("Result Panel")]
    [Tooltip("หน้าต่างสรุปผลจบเกมทั้งหมด")]
    public GameObject resultPanel;

    [Tooltip("ข้อความหัวข้อด้านบน (เช่น STAGE CLEAR, GAME OVER)")]
    public TextMeshProUGUI titleText;

    [Header("Star UI References (รูปดาว 3 ดวง)")]
    public Image star1Image;
    public Image star2Image;
    public Image star3Image;

    [Header("Star Sprites & Colors (ภาพ/สีของดาว)")]
    public Sprite starFilledSprite;
    public Sprite starEmptySprite;
    public Color starActiveColor = Color.white;
    public Color starInactiveColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);

    [Header("Star Animation Settings (ตั้งค่าแอนิเมชันดาวเด้ง)")]
    [Tooltip("เปิดใช้งานแอนิเมชันดาวเด้งทีละดวงหรือไม่")]
    public bool enableStarAnimation = true;

    [Tooltip("เสียง Sound Effect ตอนดาวเด้งขึ้นมาทีละดวง (Optional)")]
    public AudioClip starPopSFX;

    [Tooltip("ระยะเวลาหน่วงก่อนดาวดวงถัดไปจะเด้ง (วินาที)")]
    public float delayBetweenStars = 0.35f;

    [Tooltip("ความพองตัวสูงสุดตอนเด้ง (เช่น 1.4 = ขยาย 140% แล้วหดกลับมาปกติ)")]
    public float popOvershoot = 1.35f;

    [Tooltip("ระยะเวลาอนิเมชันเด้งของดาวแต่ละดวง (วินาที)")]
    public float popDuration = 0.25f;

    [Header("Summary Details")]
    public TextMeshProUGUI scoreSummaryText;
    public TextMeshProUGUI escapeSummaryText;

    [Header("Text Status Colors (สีบอกสถานะ ผ่าน/ไม่ครบ)")]
    public Color textPassColor = new Color(0.4f, 1f, 0.4f, 1f);
    public Color textFailColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Audio (Optional)")]
    public AudioClip winSFX;
    public AudioClip loseSFX;

    private bool hasShownResult = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        if (!hasShownResult)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void ShowResult(bool reachedFinishLine)
    {
        if (hasShownResult) return;
        hasShownResult = true;

        PauseMenuManager.isGameOver = true;

        // ------------------ 1. ตรวจสอบเงื่อนไขแต่ละข้อ ------------------
        bool condFinish = reachedFinishLine;

        bool condScore = false;
        int currentScore = 0;
        int targetScore = 0;
        if (GameScoreManager.Instance != null)
        {
            currentScore = GameScoreManager.Instance.currentScore;
            targetScore = GameScoreManager.Instance.targetScore;
            condScore = (currentScore >= targetScore);
        }

        bool condEscape = false;
        int escapedCount = 0;
        int maxEscapes = 0;
        if (EnemyEscapeManager.Instance != null)
        {
            escapedCount = EnemyEscapeManager.Instance.CurrentEscapes;
            maxEscapes = EnemyEscapeManager.Instance.maxEscapesAllowed;
            condEscape = (escapedCount <= maxEscapes);
        }

        // ------------------ 2. นับจำนวนดาวรวม ------------------
        int totalStarsEarned = 0;
        if (condFinish) totalStarsEarned++;
        if (condScore) totalStarsEarned++;
        if (condEscape) totalStarsEarned++;

        // ------------------ 3. อัปเดตข้อความและสีสถานะ ------------------
        if (titleText != null)
        {
            titleText.text = condFinish ? "STAGE CLEAR" : "GAME OVER";
            titleText.color = condFinish ? textPassColor : textFailColor;
        }

        if (scoreSummaryText != null)
        {
            string statusIcon = condScore ? "Successful" : "Fail";
            scoreSummaryText.text = $"Score: {currentScore} / {targetScore}  ({statusIcon})";
            scoreSummaryText.color = condScore ? textPassColor : textFailColor;
        }

        if (escapeSummaryText != null)
        {
            string statusIcon = condEscape ? "Successful" : "";
            escapeSummaryText.text = $"Escaped: {escapedCount} / {maxEscapes}  ({statusIcon})";
            escapeSummaryText.color = condEscape ? textPassColor : textFailColor;
        }

        // ------------------ 4. เปิดหน้าต่าง UI ------------------
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        PlayEndSFX(condFinish);

        // หยุดเวลาเกม
        Time.timeScale = 0f;

        // ------------------ 5. สั่งเล่นแอนิเมชันดาว ------------------
        if (enableStarAnimation)
        {
            StartCoroutine(AnimateStarsSequence(totalStarsEarned));
        }
        else
        {
            SetStarDisplay(star1Image, totalStarsEarned >= 1);
            SetStarDisplay(star2Image, totalStarsEarned >= 2);
            SetStarDisplay(star3Image, totalStarsEarned >= 3);
        }
    }

    // ⭐ Coroutine แอนิเมชันดาวค่อยๆ เด้งทีละดวง
    private IEnumerator AnimateStarsSequence(int totalStarsEarned)
    {
        Image[] starImages = new Image[] { star1Image, star2Image, star3Image };

        // เซ็ตขนาดดาวทุกดวงเป็น 0 ก่อนเริ่มเด้ง
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                SetStarDisplay(starImages[i], false);
                starImages[i].transform.localScale = Vector3.zero;
            }
        }

        // รอให้หน้าต่างการ์ดแสดงตัวชัดเจนก่อนเริ่มเด้งดาว
        yield return new WaitForSecondsRealtime(0.2f);

        // ค่อยๆ เล่นแอนิเมชันทีละดวง
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            bool isEarned = (i < totalStarsEarned);
            SetStarDisplay(starImages[i], isEarned);

            // เล่นเสียง Sound Effect ประจำดาว (ถ้ามี)
            if (isEarned && starPopSFX != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(starPopSFX, Camera.main.transform.position);
            }

            // แอนิเมชันสเกลเด้งขยายใหญ่แล้วยุบกลับ (Bounce Pop)
            yield return StartCoroutine(PopScaleRoutine(starImages[i].transform));

            // หน่วงเวลาก่อนดวงถัดไปจะเด้ง
            yield return new WaitForSecondsRealtime(delayBetweenStars);
        }
    }

    private IEnumerator PopScaleRoutine(Transform targetTransform)
    {
        float elapsed = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 overshootScale = Vector3.one * popOvershoot;
        Vector3 normalScale = Vector3.one;

        // ช่วงที่ 1: พุ่งจาก 0 ไปขนาดใหญ่กว่าปกติ (Overshoot)
        float phase1Duration = popDuration * 0.65f;
        while (elapsed < phase1Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / phase1Duration);
            // ใช้ SmoothStep เพื่อให้การเคลื่อนที่นุ่มนวล
            targetTransform.localScale = Vector3.Lerp(initialScale, overshootScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // ช่วงที่ 2: เด้งยุบกลับมาที่ขนาดปกติ 1.0
        elapsed = 0f;
        float phase2Duration = popDuration * 0.35f;
        while (elapsed < phase2Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / phase2Duration);
            targetTransform.localScale = Vector3.Lerp(overshootScale, normalScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        targetTransform.localScale = normalScale;
    }

    private void SetStarDisplay(Image starImg, bool earned)
    {
        if (starImg == null) return;

        if (starFilledSprite != null && starEmptySprite != null && starFilledSprite != starEmptySprite)
        {
            starImg.sprite = earned ? starFilledSprite : starEmptySprite;
            starImg.color = earned ? starActiveColor : starInactiveColor;
        }
        else
        {
            if (starFilledSprite != null) starImg.sprite = starFilledSprite;
            starImg.color = earned ? starActiveColor : starInactiveColor;
        }
    }

    private void PlayEndSFX(bool won)
    {
        AudioClip clipToPlay = won ? winSFX : loseSFX;
        if (clipToPlay != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, Camera.main.transform.position);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        PauseMenuManager.isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu(string sceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        PauseMenuManager.isGameOver = false;
        SceneManager.LoadScene(sceneName);
    }
}