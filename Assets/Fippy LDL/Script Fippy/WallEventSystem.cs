using System.Collections;
using UnityEngine;

public class WallEventSystem : MonoBehaviour
{
    [Header("Wall Transforms")]
    public Transform topWall;             // แถบสีเหลืองด้านบน
    public Transform bottomWall;          // แถบสีเหลืองด้านล่าง

    [Header("Movement Settings (ปรับการเลื่อน)")]
    [Tooltip("ความเร็วในการเลื่อน (หน่วย/วินาที) ค่ายิ่งมาก ยิ่งพุ่งเร็ว")]
    public float moveSpeed = 6f; // ⭐ ปรับความเร็วการเลื่อนได้ตรงนี้

    [Tooltip("ระยะทางที่เลื่อนไปทางซ้ายจนถึงจุดเป้าหมาย (ก่อนที่จะลบออก/รีเซ็ต)")]
    public float slideDistance = 25f;

    [Header("Event Timing Settings")]
    [Tooltip("เวลาปิด Spawner ล่วงหน้าก่อนกำแพงเริ่มเลื่อน (วินาที)")]
    public float preWarningTime = 2.5f;

    [Tooltip("คูลดาวน์หลังจบ Event ก่อนเริ่มสุ่มรอบใหม่ (วินาที)")]
    public float cooldownTime = 15f;

    [Range(0f, 100f)]
    [Tooltip("โอกาสเกิด Event (%)")]
    public float eventChance = 67f;

    [Header("Spawners (ตัวสร้างศัตรูและไอเทม)")]
    public GameObject enemySpawner;         // ลาก 'Spawn Enemy' มาใส่
    public GameObject itemSpawner;          // ลาก 'Spawn Item' มาใส่

    [Header("Debug Settings")]
    public bool showDebugOnScreen = true;

    private string currentStatus = "Initializing";
    private string currentPattern = "None";
    private float timerDisplay = 0f;
    private float lastRollResult = -1f;

    private Vector3 topStartPos;
    private Vector3 bottomStartPos;

    private void Start()
    {
        if (topWall != null) topStartPos = topWall.position;
        if (bottomWall != null) bottomStartPos = bottomWall.position;

        StartCoroutine(EventLoopRoutine());
    }

    private IEnumerator EventLoopRoutine()
    {
        while (true)
        {
            // 1. ช่วงคูลดาวน์
            currentStatus = "Cooldown";
            currentPattern = "None";
            timerDisplay = cooldownTime;

            while (timerDisplay > 0f)
            {
                timerDisplay -= Time.deltaTime;
                yield return null;
            }
            timerDisplay = 0f;

            // 2. สุ่มทอยโอกาสเกิด Event
            currentStatus = "Rolling Chance";
            lastRollResult = Random.Range(0f, 100f);

            if (lastRollResult <= eventChance)
            {
                Debug.Log($"<color=yellow>[Event] ทอยได้: {lastRollResult:F1}% กำลังเริ่ม Event!</color>");
                yield return StartCoroutine(TriggerEventRoutine());
            }
            else
            {
                Debug.Log($"<color=grey>[Event] ไม่เกิด Event (ทอยได้: {lastRollResult:F1}%) รอตรวจรอบถัดไป</color>");
            }
        }
    }

    private IEnumerator TriggerEventRoutine()
    {
        // ขั้นตอนที่ 1: ช่วงแจ้งเตือนล่วงหน้า (ปิด Spawner)
        currentStatus = "Warning (Spawners Disabled)";

        if (enemySpawner != null) enemySpawner.SetActive(false);
        if (itemSpawner != null) itemSpawner.SetActive(false);

        timerDisplay = preWarningTime;
        while (timerDisplay > 0f)
        {
            timerDisplay -= Time.deltaTime;
            yield return null;
        }
        timerDisplay = 0f;

        // ขั้นตอนที่ 2: สุ่มเลือกกำแพงที่จะเลื่อน (33.33% แต่ละแบบ)
        float patternRoll = Random.Range(0f, 100f);
        bool moveTop = false;
        bool moveBottom = false;

        if (patternRoll < 33.33f)
        {
            moveTop = true;
            currentPattern = "Top Only (33%)";
        }
        else if (patternRoll < 66.66f)
        {
            moveBottom = true;
            currentPattern = "Bottom Only (33%)";
        }
        else
        {
            moveTop = true;
            moveBottom = true;
            currentPattern = "Both Walls (33%)";
        }

        Debug.Log($"<color=orange>[Event Pattern] {currentPattern} กำแพงเริ่มเลื่อนด้วยความเร็ว {moveSpeed}!</color>");

        // คำนวณพิกัดจุดเป้าหมายทางซ้าย
        Vector3 topTarget = moveTop ? topStartPos + (Vector3.left * slideDistance) : topStartPos;
        Vector3 bottomTarget = moveBottom ? bottomStartPos + (Vector3.left * slideDistance) : bottomStartPos;

        // ขั้นตอนที่ 3: เลื่อนเข้าหาจุดเป้าหมายอย่างต่อเนื่องตามความเร็ว moveSpeed
        currentStatus = "Sliding";
        yield return StartCoroutine(MoveWallsConstantSpeed(topTarget, bottomTarget, moveTop, moveBottom));

        // ขั้นตอนที่ 4: เมื่อถึงจุดเป้าหมาย วาร์ปกลับจุดเริ่มต้นทันที (ไม่ค้าง)
        if (topWall != null) topWall.position = topStartPos;
        if (bottomWall != null) bottomWall.position = bottomStartPos;

        // ขั้นตอนที่ 5: เปิด Spawner ศัตรูและไอเทมกลับมาทำงานทันที
        if (enemySpawner != null) enemySpawner.SetActive(true);
        if (itemSpawner != null) itemSpawner.SetActive(true);

        Debug.Log("<color=green>[Event] กำแพงถึงเป้าหมายแล้ว รีเซ็ตกลับจุดเริ่มต้น และเริ่มนับคูลดาวน์รอบใหม่</color>");
    }

    // ฟังก์ชันเลื่อนกำแพงด้วยความเร็วคงที่
    private IEnumerator MoveWallsConstantSpeed(Vector3 topTarget, Vector3 bottomTarget, bool moveTop, bool moveBottom)
    {
        while (true)
        {
            bool topReached = true;
            bool bottomReached = true;

            if (moveTop && topWall != null)
            {
                topWall.position = Vector3.MoveTowards(topWall.position, topTarget, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(topWall.position, topTarget) > 0.01f) topReached = false;
            }

            if (moveBottom && bottomWall != null)
            {
                bottomWall.position = Vector3.MoveTowards(bottomWall.position, bottomTarget, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(bottomWall.position, bottomTarget) > 0.01f) bottomReached = false;
            }

            // ถ้ากำแพงที่เลือกเลื่อนถึงเป้าหมายแล้ว ให้ออกจากลูป
            if (topReached && bottomReached)
            {
                break;
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // แสดงเส้นทางและกล่องจำลองจุดเป้าหมายใน Scene View
        if (topWall != null)
        {
            Vector3 target = topWall.position + Vector3.left * slideDistance;
            Gizmos.DrawLine(topWall.position, target);
            Gizmos.DrawWireCube(target, topWall.localScale);
        }

        if (bottomWall != null)
        {
            Vector3 target = bottomWall.position + Vector3.left * slideDistance;
            Gizmos.DrawLine(bottomWall.position, target);
            Gizmos.DrawWireCube(target, bottomWall.localScale);
        }
    }

    private void OnGUI()
    {
        if (!showDebugOnScreen) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;

        float boxX = 20f;
        float boxY = Screen.height - 120f;

        if (currentStatus.Contains("Warning"))
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(boxX, boxY, 400, 25), $"[WARNING] Wall incoming in: {timerDisplay:F1}s", style);
        }
        else if (currentStatus.Contains("Sliding"))
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(boxX, boxY, 400, 25), $"[EVENT ACTIVE] {currentPattern} (Speed: {moveSpeed})", style);
        }
        else
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(boxX, boxY, 400, 25), $"[EVENT STATUS] {currentStatus}", style);

            style.normal.textColor = Color.white;
            GUI.Label(new Rect(boxX, boxY + 25, 400, 25), $"Next Event Check: {timerDisplay:F1}s", style);
        }

        string lastRollText = lastRollResult >= 0 ? $"{lastRollResult:F1}%" : "None";
        style.normal.textColor = Color.gray;
        GUI.Label(new Rect(boxX, boxY + 50, 400, 25), $"Chance: {eventChance}% | Last Roll: {lastRollText}", style);
    }
}