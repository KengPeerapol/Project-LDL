using System.Collections;
using UnityEngine;

public class WallEventSystem : MonoBehaviour
{
    [Header("Wall Transforms")]
    public Transform topWall;
    public Transform bottomWall;
    public float squeezeDistance = 1.8f;
    public float slideDuration = 1.5f;

    [Header("Event Timing Settings")]
    public float eventDuration = 10f;
    public float cooldownTime = 20f;
    [Range(0f, 100f)]
    public float eventChance = 67f;

    [Header("Enemy Spawner")]
    public GameObject enemySpawner;

    [Header("Debug Settings")]
    public bool showDebugOnScreen = true;

    // สถานะสำหรับแสดงผล Debug
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
            // 1. ช่วงคูลดาวน์ 20 วิ
            currentStatus = "Cooldown";
            currentPattern = "None";
            timerDisplay = cooldownTime;

            while (timerDisplay > 0f)
            {
                timerDisplay -= Time.deltaTime;
                yield return null;
            }
            timerDisplay = 0f;

            // 2. สุ่มโอกาสเกิด Event (67%)
            currentStatus = "Rolling Chance";
            lastRollResult = Random.Range(0f, 100f);

            if (lastRollResult <= eventChance)
            {
                Debug.Log($"<color=yellow>[Event Debug] เกิด Event! ทอยได้: {lastRollResult:F1}% (ต้องการ <= {eventChance}%)</color>");
                yield return StartCoroutine(TriggerEventRoutine());
            }
            else
            {
                Debug.Log($"<color=grey>[Event Debug] ไม่เกิด Event! ทอยได้: {lastRollResult:F1}% (ต้องการ <= {eventChance}%) รอตรวจรอบถัดไป</color>");
            }
        }
    }

    private IEnumerator TriggerEventRoutine()
    {
        currentStatus = "Event Active";

        // ปิดการทำงานของ Spawner ศัตรู
        if (enemySpawner != null)
        {
            enemySpawner.SetActive(false);
            Debug.Log("<color=red>[Event Debug] ปิด Enemy Spawner ชั่วคราว</color>");
        }

        // สุ่มรูปแบบ 3 แบบ (แบบละ 33.33%)
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

        Debug.Log($"<color=orange>[Event Debug] รูปแบบที่เลือก: {currentPattern} (ทอยได้: {patternRoll:F1}%)</color>");

        // คำนวณพิกัดเป้าหมาย
        Vector3 topTarget = moveTop ? topStartPos + Vector3.down * squeezeDistance : topStartPos;
        Vector3 bottomTarget = moveBottom ? bottomStartPos + Vector3.up * squeezeDistance : bottomStartPos;

        // เลื่อนกำแพงเข้ามา
        yield return StartCoroutine(MoveWalls(topTarget, bottomTarget, slideDuration));

        // นับเวลาค้างของ Event
        float holdTime = Mathf.Max(0f, eventDuration - (slideDuration * 2f));
        timerDisplay = holdTime;

        while (timerDisplay > 0f)
        {
            timerDisplay -= Time.deltaTime;
            yield return null;
        }
        timerDisplay = 0f;

        // เลื่อนกำแพงกลับ
        currentStatus = "Returning Walls";
        yield return StartCoroutine(MoveWalls(topStartPos, bottomStartPos, slideDuration));

        // เปิด Enemy Spawner ให้กลับมาทำงาน
        if (enemySpawner != null)
        {
            enemySpawner.SetActive(true);
            Debug.Log("<color=green>[Event Debug] เปิด Enemy Spawner กลับมาทำงานตามปกติ</color>");
        }
    }

    private IEnumerator MoveWalls(Vector3 targetTop, Vector3 targetBottom, float duration)
    {
        float elapsed = 0f;
        Vector3 currentTopStart = topWall != null ? topWall.position : Vector3.zero;
        Vector3 currentBottomStart = bottomWall != null ? bottomWall.position : Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (topWall != null) topWall.position = Vector3.Lerp(currentTopStart, targetTop, t);
            if (bottomWall != null) bottomWall.position = Vector3.Lerp(currentBottomStart, targetBottom, t);

            yield return null;
        }

        if (topWall != null) topWall.position = targetTop;
        if (bottomWall != null) bottomWall.position = targetBottom;
    }

    // วาดเส้นแนวระยะบีบของกำแพงในหน้า Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        if (topWall != null)
        {
            Vector3 target = topWall.position + Vector3.down * squeezeDistance;
            Gizmos.DrawWireCube(target, topWall.localScale);
            Gizmos.DrawLine(topWall.position, target);
        }

        if (bottomWall != null)
        {
            Vector3 target = bottomWall.position + Vector3.up * squeezeDistance;
            Gizmos.DrawWireCube(target, bottomWall.localScale);
            Gizmos.DrawLine(bottomWall.position, target);
        }
    }

    // แสดงข้อมูล Debug บนหน้าจอ Game View แบบเรียลไทม์
    private void OnGUI()
    {
        if (!showDebugOnScreen) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;

        // กล่องพื้นหลังข้อความ Debug มุมซ้ายล่าง
        float boxX = 20f;
        float boxY = Screen.height - 120f;

        if (currentStatus == "Event Active")
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(boxX, boxY, 400, 25), $"[EVENT ACTIVE] Mode: {currentPattern}", style);

            style.normal.textColor = Color.white;
            GUI.Label(new Rect(boxX, boxY + 25, 400, 25), $"Event Duration Left: {timerDisplay:F1}s", style);
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