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

    [Header("UI Score (แต้มคะแนน)")]
    public TextMeshProUGUI scoreText;
    public Color scorePendingColor = Color.white;
    public Color scoreCompletedColor = Color.green;

    [Header("UI Distance (ระยะทาง)")]
    public TextMeshProUGUI distanceText;
    public Slider distanceSlider;

    private float initialDistance = 0f;
    private bool isGameEnded = false;

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

    private void UpdateDistanceUI()
    {
        if (player == null || finishLine == null) return;

        float remainingDistance = finishLine.position.x - player.position.x;
        if (remainingDistance < 0f) remainingDistance = 0f;

        if (distanceText != null)
        {
            // ปัดเศษลง (Mathf.Floor) เพื่อให้ตัวเลขลดลงอย่างเป็นธรรมชาติ
            distanceText.text = $"Distance: {Mathf.FloorToInt(remainingDistance)}m";
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
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore} / {targetScore}";
            scoreText.color = (currentScore >= targetScore) ? scoreCompletedColor : scorePendingColor;
        }
    }

    // ⭐ เมื่อผู้เล่นชนเส้นชัย
    public void CheckFinishLine(PlayerControllerTest playerController)
    {
        if (isGameEnded) return;
        isGameEnded = true;

        // ⭐ บังคับเซ็ตระยะทางให้เป็น 0m และหลอดเต็ม 100% ทันทีที่ชนเส้นชัย
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