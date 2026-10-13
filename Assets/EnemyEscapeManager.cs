using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyEscapeManager : MonoBehaviour
{
    public static EnemyEscapeManager Instance { get; private set; }

    [Header("Escape Limit Settings (โควตาดาวดวงที่ 3)")]
    [Tooltip("จำนวนศัตรูสูงสุดที่ยอมให้หลุดได้เพื่อรับดาวดวงที่ 3 (หลุดเกินจะไม่แพ้ แต่จะชวดดาวข้อนี้)")]
    public int maxEscapesAllowed = 5;

    [Tooltip("รูปแบบการแสดงผล: ติ๊กถูก = แสดงจำนวนที่หลุดไปแล้ว (0/5) | ติ๊กออก = แสดงจำนวนที่เหลือ (5)")]
    public bool showAsCountUp = true;

    [Header("UI Displays")]
    public TextMeshProUGUI escapeText;
    public Image decorativeIcon;
    public Sprite iconSprite;

    [Header("Color Warning (สีเตือนสถานะดาว)")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red; // สีแดงเมื่อหลุดเกินโควตาดาว

    private int currentEscapes = 0;

    // Property สำหรับให้ StarResultManager ดึงค่าไปคำนวณดาว
    public int CurrentEscapes => currentEscapes;

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
        currentEscapes = 0;

        if (decorativeIcon != null && iconSprite != null)
        {
            decorativeIcon.sprite = iconSprite;
        }

        UpdateUI();
    }

    // ⭐ ทำงานเมื่อมี Enemy หลุดขอบจอด้านซ้าย
    public void OnEnemyEscaped(string enemyName = "")
    {
        currentEscapes++;
        UpdateUI();

        Debug.Log($"<color=orange>[Enemy Escaped] ศัตรูหลุดรอด: {currentEscapes} ตัว (โควตาดาว: {maxEscapesAllowed})</color>");

        // ⭐ ตัดคำสั่ง TriggerGameOver() ออกแล้ว เกมจะไม่หยุด/ไม่แพ้ เล่นต่อได้ตามปกติ
    }

    private void UpdateUI()
    {
        if (escapeText == null) return;

        int remaining = Mathf.Max(0, maxEscapesAllowed - currentEscapes);

        if (showAsCountUp)
        {
            escapeText.text = $"Escaped: {currentEscapes} / {maxEscapesAllowed}";
        }
        else
        {
            escapeText.text = $"Remaining: {remaining}";
        }

        // แจ้งเตือนสีตัวอักษร
        if (currentEscapes > maxEscapesAllowed)
        {
            escapeText.color = dangerColor; // หลุดเกินโควตาแล้ว (สีแดง - ชวดดาวข้อนี้)
        }
        else if (remaining <= 1)
        {
            escapeText.color = warningColor; // ใกล้หลุดเกินโควตา
        }
        else
        {
            escapeText.color = normalColor;
        }
    }
}