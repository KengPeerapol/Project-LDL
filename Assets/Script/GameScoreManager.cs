using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance { get; private set; }

    [Header("Score Settings (ตั้งค่าคะแนน)")]
    public int targetScore = 100;
    public int currentScore = 0;

    [Header("Point Bar Stepping Settings (การเพิ่มทีละขั้น)")]
    [Tooltip("⭐ ติ๊กถูก = ให้หลอดคะแนนขยับทีละสเต็ป (เช่น ทีละ 20% ให้ตรงกับ 5 ช่องของ ldl-bar)")]
    public bool snapToSteps = true;

    [Range(1, 50)]
    [Tooltip("ขนาดของแต่ละสเต็ปเป็นเปอร์เซ็นต์ (ค่าเริ่มต้น 20% = 5 ช่อง)")]
    public int stepPercent = 20;

    [Header("References (ตัวละครและเส้นชัย)")]
    public Transform player;
    public Transform finishLine;

    [Header("--- UI Point Bar (หลอดคะแนนใต้ตัว Player) ---")]
    [Tooltip("ข้อความกำกับใต้หลอดคะแนน (เช่น point bar)")]
    public TextMeshProUGUI scoreText;
    public Color scorePendingColor = Color.white;
    public Color scoreCompletedColor = Color.green;

    [Tooltip("หลอดคะแนนแบบ Slider")]
    public Slider scoreSlider;

    [Tooltip("หลอดคะแนนแบบ Image Filled")]
    public Image scoreBarFill;

    [Tooltip("SpriteRenderer ของ ldl-bar ใต้ตัว Player (ถ้ามี)")]
    public SpriteRenderer playerLdlBarSprite;

    [Header("--- UI Distance Bar (หลอดระยะทางบนกลางจอ) ---")]
    public TextMeshProUGUI distanceText;
    public Slider distanceSlider;

    private float initialDistance = 0f;
    private bool isGameEnded = false;
    private bool hasPlayedTargetSound = false;
    private AudioManger audioManager;

    // Optimized Dirty-Check Cache
    private int lastDisplayedDistance = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManger>();
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (finishLine == null)
        {
            FinishLine fl = Object.FindAnyObjectByType<FinishLine>();
            if (fl != null) finishLine = fl.transform;
        }

        if (player != null && finishLine != null)
        {
            initialDistance = Mathf.Abs(finishLine.position.x - player.position.x);
        }

        UpdateScoreUI();
    }

    private void Update()
    {
        if (isGameEnded) return;
        UpdateDistanceUI();
    }

    // ⭐ อัปเดตข้อความระยะทางเฉพาะเมื่อตัวเลขเปลี่ยน ป้องกันการเกิด GC Alloc ทุกเฟรม
    private void UpdateDistanceUI()
    {
        if (player == null || finishLine == null) return;

        float remainingDistance = finishLine.position.x - player.position.x;
        if (remainingDistance < 0f) remainingDistance = 0f;

        int currentDistanceMeter = Mathf.FloorToInt(remainingDistance);

        if (distanceText != null && currentDistanceMeter != lastDisplayedDistance)
        {
            lastDisplayedDistance = currentDistanceMeter;
            distanceText.text = $"Distance: {currentDistanceMeter}m";
        }

        if (distanceSlider != null && initialDistance > 0f)
        {
            float progress = 1f - (remainingDistance / initialDistance);
            distanceSlider.value = Mathf.Clamp01(progress);
        }
    }

    public void AddScore(int amount)
    {
        if (isGameEnded) return;
        currentScore += amount;
        UpdateScoreUI();

        if (currentScore >= targetScore && !hasPlayedTargetSound)
        {
            hasPlayedTargetSound = true;
            if (audioManager != null && audioManager.ScoreReached != null)
            {
                audioManager.PlaySFX(audioManager.ScoreReached);
            }
            Debug.Log("<color=yellow>[Score] แต้มถึงเกณฑ์แล้ว! เล่นเสียงเตือน!</color>");
        }
    }

    // ⭐ คำนวณแบบสเต็ป 20% (Event-Driven ทำงานเฉพาะตอนคะแนนเปลี่ยน)
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore} / {targetScore}";
            scoreText.color = (currentScore >= targetScore) ? scoreCompletedColor : scorePendingColor;
        }

        // 1. คำนวณเปอร์เซ็นต์คะแนนจริง
        int percent = (targetScore > 0) ? (currentScore * 100) / targetScore : 0;

        // 2. ปัดสเต็ปทีละ 20% ด้วย Integer Math (ป้องกันบั๊กทศนิยมเพี้ยน)
        if (snapToSteps && stepPercent > 0)
        {
            percent = (percent / stepPercent) * stepPercent;
        }

        // 3. แปลงค่าเป็นช่วง 0.0f - 1.0f สำหรับ Slider และ Image Fill
        float scoreProgress = Mathf.Clamp01(percent / 100f);

        if (scoreSlider != null)
        {
            scoreSlider.value = scoreProgress;
        }

        if (scoreBarFill != null)
        {
            scoreBarFill.fillAmount = scoreProgress;
        }
    }

    public void CheckFinishLine(PlayerControllerTest playerController)
    {
        if (isGameEnded) return;
        isGameEnded = true;

        if (distanceText != null)
        {
            distanceText.text = "Distance: 0m";
        }
        if (distanceSlider != null)
        {
            distanceSlider.value = 1f;
        }

        StartCoroutine(FinishSequenceRoutine(playerController));
    }

    private IEnumerator FinishSequenceRoutine(PlayerControllerTest playerController)
    {
        if (playerController != null)
        {
            playerController.TriggerWinDash();
        }

        yield return new WaitForSeconds(0.8f);

        if (StarResultManager.Instance != null)
        {
            StarResultManager.Instance.ShowResult(reachedFinishLine: true);
        }
    }
}