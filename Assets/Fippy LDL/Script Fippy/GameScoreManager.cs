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

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject gameOverPanel;

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
            FinishLine fl = Object.FindFirstObjectByType<FinishLine>();
            if (fl != null) finishLine = fl.transform;
        }

        if (player != null && finishLine != null)
        {
            initialDistance = Mathf.Abs(finishLine.position.x - player.position.x);
        }

        UpdateScoreUI();
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
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
            distanceText.text = $"Distance: {remainingDistance:F0}m";
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

    // ⭐ ตรวจสอบผลแพ้-ชนะเมื่อชนเส้นชัย
    public void CheckFinishLine(PlayerControllerTest playerController)
    {
        if (isGameEnded) return;
        isGameEnded = true;

        if (currentScore >= targetScore)
        {
            // ชนะ: คะแนนครบ -> บินพุ่งทะลุจอให้เสร็จก่อน ค่อยเปิดหน้าชนะ
            StartCoroutine(WinSequenceRoutine(playerController));
        }
        else
        {
            // แพ้: คะแนนไม่ครบ -> ให้ผู้เล่นตายก่อน รอ 1 วินาที ค่อยหยุดเกมและเปิดหน้าแพ้
            StartCoroutine(LoseSequenceRoutine(playerController));
        }
    }

    // ⭐ Coroutine กรณีชนะ: รอให้ Player บินทะลุขอบจอไปทางขวาก่อน
    private IEnumerator WinSequenceRoutine(PlayerControllerTest playerController)
    {
        if (playerController != null)
        {
            playerController.TriggerWinDash();
        }

        Camera cam = Camera.main;
        float maxTimeout = 3f;
        float timer = 0f;

        while (timer < maxTimeout)
        {
            timer += Time.deltaTime;

            if (playerController != null && cam != null)
            {
                Vector3 viewPos = cam.WorldToViewportPoint(playerController.transform.position);
                if (viewPos.x > 1.15f)
                {
                    break;
                }
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // ⭐ Coroutine กรณีแพ้: สั่งตาย -> รอ 1 วินาที -> เปิดหน้า Game Over และหยุดเกม
    private IEnumerator LoseSequenceRoutine(PlayerControllerTest playerController)
    {
        Debug.Log("<color=red>[Defeat] แต้มไม่ครบตามเป้าหมาย! กำลังเริ่มอนิเมชันการตาย...</color>");

        // 1. สั่งให้ผู้เล่นเข้าสู่สถานะตาย
        if (playerController != null)
        {
            // สั่งลดเลือดจนหมดเพื่อให้ PlayerHealthTest เล่นอนิเมชันสั่นตัวละคร
            if (playerController.TryGetComponent(out PlayerHealthTest healthTest))
            {
                healthTest.TakeDamage(healthTest.maxHealth);
            }
            else
            {
                playerController.TriggerDeath();
            }
        }

        // 2. หน่วงเวลา 1.0 วินาที ให้เห็นการตาย/สั่นตัวละครอย่างชัดเจน
        yield return new WaitForSeconds(1.0f);

        // 3. เปิดเคอร์เซอร์เมาส์ให้คลิกปุ่ม UI ได้
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 4. แสดงหน้าต่าง Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 5. หยุดเวลาเกมทั้งหมด
        Time.timeScale = 0f;
    }
}