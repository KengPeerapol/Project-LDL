using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyEscapeManager : MonoBehaviour
{
    public static EnemyEscapeManager Instance { get; private set; }

    [Header("Escape Limit Settings (โควตาศัตรูที่หลุดรอด)")]
    public int maxEscapesAllowed = 5;
    public int currentEscapes = 0;

    public int CurrentEscapes => currentEscapes;
    public int maxEscapes => maxEscapesAllowed;

    [Header("--- UI Escape Bar Settings ---")]
    public Image escapeBarFill;
    public Slider escapeSlider;
    public TextMeshProUGUI escapeText;
    public Image decorativeIcon;

    [Header("--- Bar Juice Effect (การสั่นและพิกเซล) ---")]
    public UIBarPixelEffect barEffect;

    [Header("Bar Direction Settings")]
    public bool fillAsEscapesIncrease = true;
    public bool snapToSteps = true;

    [Header("Color Warning (เขียว -> ส้ม -> แดง)")]
    public Color safeColor = Color.green;
    public Color warningColor = new Color(1f, 0.55f, 0f);
    public Color dangerColor = Color.red;

    [Header("Danger Thresholds")]
    [Range(0f, 1f)] public float warningThreshold = 0.35f;
    [Range(0f, 1f)] public float dangerThreshold = 0.70f;

    private Image cachedSliderFillImage;
    private AudioManger audioManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (escapeSlider != null && escapeSlider.fillRect != null)
        {
            cachedSliderFillImage = escapeSlider.fillRect.GetComponent<Image>();
        }

        if (barEffect == null)
        {
            if (escapeBarFill != null) barEffect = escapeBarFill.GetComponent<UIBarPixelEffect>();
            if (barEffect == null) barEffect = GetComponentInChildren<UIBarPixelEffect>();
        }
    }

    private void Start()
    {
        currentEscapes = 0;

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManger>();
        }

        if (escapeText != null)
        {
            escapeText.text = "escape bar";
        }

        UpdateEscapeBarUI(triggerEffect: false);
    }

    public void OnEnemyEscaped(string enemyName = "")
    {
        currentEscapes++;
        UpdateEscapeBarUI(triggerEffect: true);

        Debug.Log($"<color=orange>[Enemy Escaped] {enemyName} หลุดรอด: {currentEscapes}/{maxEscapesAllowed}</color>");

        // ⭐ ดึงเสียง EnemyEscape จาก AudioManger โดยตรง
        if (audioManager != null && audioManager.EnemyEscape != null)
        {
            audioManager.PlaySFX(audioManager.EnemyEscape);
        }
    }

    private void UpdateEscapeBarUI(bool triggerEffect = false)
    {
        if (maxEscapesAllowed <= 0) return;

        float dangerRatio = Mathf.Clamp01((float)currentEscapes / maxEscapesAllowed);

        float fillProgress = dangerRatio;
        if (snapToSteps)
        {
            float step = 1f / maxEscapesAllowed;
            fillProgress = Mathf.Round(fillProgress / step) * step;
        }

        float displayValue = fillAsEscapesIncrease ? fillProgress : (1f - fillProgress);

        Color targetBarColor;
        if (dangerRatio < warningThreshold) targetBarColor = safeColor;
        else if (dangerRatio < dangerThreshold) targetBarColor = warningColor;
        else targetBarColor = dangerColor;

        if (escapeBarFill != null)
        {
            escapeBarFill.fillAmount = displayValue;
            escapeBarFill.color = targetBarColor;
        }

        if (escapeSlider != null)
        {
            escapeSlider.value = displayValue;
            if (cachedSliderFillImage != null)
            {
                cachedSliderFillImage.color = targetBarColor;
            }
        }

        if (triggerEffect && barEffect != null)
        {
            barEffect.PlayEffect(targetBarColor);
        }
    }

    public void ResetEscapes()
    {
        currentEscapes = 0;
        UpdateEscapeBarUI(triggerEffect: false);
    }
}