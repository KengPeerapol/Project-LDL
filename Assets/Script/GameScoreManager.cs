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

    // ⭐ อัปเดต Point Bar แบบ Event-Driven (เฉพาะเมื่อแต้มเปลี่ยน)
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore} / {targetScore}";
            scoreText.color = (currentScore >= targetScore) ? scoreCompletedColor : scorePendingColor;
        }

        float scoreProgress = (targetScore > 0) ? Mathf.Clamp01((float)currentScore / targetScore) : 0f;

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