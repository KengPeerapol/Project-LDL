using System.Collections;
using UnityEngine;

public class WallEventSystem : MonoBehaviour
{
    [Header("Wall Transforms")]
    public Transform topWall;             // แถบสีเหลืองด้านบน
    public Transform bottomWall;          // แถบสีเหลืองด้านล่าง

    [Header("Horizontal Movement Settings")]
    [Tooltip("ระยะที่เลื่อนเข้าซ้ายมาอยู่ในจอเกม")]
    public float slideDistance = 14f;
    [Tooltip("ระยะที่เลื่อนไปซ้ายต่อเพื่อให้ออกพ้นขอบจอซ้าย")]
    public float exitDistance = 15f;
    [Tooltip("เวลาที่ใช้เลื่อนเข้ามาในจอ (วินาที)")]
    public float slideInDuration = 1.5f;
    [Tooltip("เวลาที่ใช้เลื่อนออกพ้นจอ (วินาที)")]
    public float slideOutDuration = 1.5f;

    [Header("Event Timing Settings")]
    [Tooltip("เวลาปิด Spawner ล่วงหน้าก่อนกำแพงเริ่มเลื่อน (วินาที)")]
    public float preWarningTime = 3f;

    [Tooltip("ระยะเวลาที่กำแพงจะหยุดค้างบีบทางอยู่ในจอ (วินาที)")]
    public float wallHoldDuration = 10f;  // ⭐ กำหนดเวลาค้างได้โดยตรงที่นี่

    [Tooltip("คูลดาวน์หลังจบ Event ก่อนเริ่มสุ่มรอบใหม่ (วินาที)")]
    public float cooldownTime = 20f;

    [Range(0f, 100f)]
    [Tooltip("โอกาสเกิด Event (67%)")]
    public float eventChance = 67f;

    [Header("Enemy Spawner")]
    public GameObject enemySpawner;         // ลาก 'Spawn Enemy' มาใส่

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
            // 1. คูลดาวน์
            currentStatus = "Cooldown";
            currentPattern = "None";
            timerDisplay = cooldownTime;

            while (timerDisplay > 0f)
            {
                timerDisplay -= Time.deltaTime;
                yield return null;
            }
            timerDisplay = 0f;

            // 2. สุ่มโอกาสเกิด Event
            currentStatus = "Rolling Chance";
            lastRollResult = Random.Range(0f, 100f);

            if (lastRollResult <= eventChance)
            {
                Debug.Log($"<color=yellow>[Event] ทอยได้: {lastRollResult:F1}% เตรียมเริ่ม Event!</color>");
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
        // ขั้นตอนที่ 1: ปิด Enemy Spawner ล่วงหน้า
        currentStatus = "Warning (Spawner Disabled)";
        if (enemySpawner != null)
        {
            enemySpawner.SetActive(false);
            Debug.Log("<color=red>[Event] สั่งปิด Enemy Spawner ล่วงหน้า!</color>");
        }

        timerDisplay = preWarningTime;
        while (timerDisplay > 0f)
        {
            timerDisplay -= Time.deltaTime;
            yield return null;
        }
        timerDisplay = 0f;

        // ขั้นตอนที่ 2: สุ่มรูปแบบ 3 แบบ (แบบละ 33.33%)
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

        Debug.Log($"<color=orange>[Event Pattern] {currentPattern} กำแพงเริ่มเลื่อนเข้ามา!</color>");

        Vector3 topInArena = moveTop ? topStartPos + (Vector3.left * slideDistance) : topStartPos;
        Vector3 bottomInArena = moveBottom ? bottomStartPos + (Vector3.left * slideDistance) : bottomStartPos;

        // ขั้นตอนที่ 3: เลื่อนเข้ามาในจอ
        currentStatus = "Sliding In";
        yield return StartCoroutine(MoveWalls(topStartPos, topInArena, bottomStartPos, bottomInArena, slideInDuration));

        // ขั้นตอนที่ 4: หยุดค้างบีบทางไว้ตามเวลาที่กำหนด (wallHoldDuration)
        currentStatus = "Holding in Arena";
        timerDisplay = wallHoldDuration;

        while (timerDisplay > 0f)
        {
            timerDisplay -= Time.deltaTime;
            yield return null;
        }
        timerDisplay = 0f;

        // ขั้นตอนที่ 5: เลื่อนทะลุออกซ้ายจนพ้นจอ
        Vector3 topExit = moveTop ? topInArena + (Vector3.left * exitDistance) : topStartPos;
        Vector3 bottomExit = moveBottom ? bottomInArena + (Vector3.left * exitDistance) : bottomStartPos;

        currentStatus = "Exiting Left";
        yield return StartCoroutine(MoveWalls(topInArena, topExit, bottomInArena, bottomExit, slideOutDuration));

        // ขั้นตอนที่ 6: วาร์ปกลับจุดเริ่มต้น
        if (topWall != null) topWall.position = topStartPos;
        if (bottomWall != null) bottomWall.position = bottomStartPos;

        // ขั้นตอนที่ 7: เปิด Spawner กลับมาทำงาน
        if (enemySpawner != null)
        {
            enemySpawner.SetActive(true);
            Debug.Log("<color=green>[Event] เปิด Enemy Spawner ทำงานตามปกติ</color>");
        }
    }

    private IEnumerator MoveWalls(Vector3 topFrom, Vector3 topTo, Vector3 bottomFrom, Vector3 bottomTo, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (topWall != null) topWall.position = Vector3.Lerp(topFrom, topTo, t);
            if (bottomWall != null) bottomWall.position = Vector3.Lerp(bottomFrom, bottomTo, t);

            yield return null;
        }

        if (topWall != null) topWall.position = topTo;
        if (bottomWall != null) bottomWall.position = bottomTo;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (topWall != null)
        {
            Vector3 inArena = topWall.position + Vector3.left * slideDistance;
            Vector3 outLeft = inArena + Vector3.left * exitDistance;
            Gizmos.DrawLine(topWall.position, inArena);
            Gizmos.DrawWireCube(inArena, topWall.localScale);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(inArena, outLeft);
        }

        Gizmos.color = Color.yellow;
        if (bottomWall != null)
        {
            Vector3 inArena = bottomWall.position + Vector3.left * slideDistance;
            Vector3 outLeft = inArena + Vector3.left * exitDistance;
            Gizmos.DrawLine(bottomWall.position, inArena);
            Gizmos.DrawWireCube(inArena, bottomWall.localScale);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(inArena, outLeft);
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
        else if (currentStatus.Contains("Sliding") || currentStatus.Contains("Holding") || currentStatus.Contains("Exiting"))
        {
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(boxX, boxY, 400, 25), $"[EVENT ACTIVE] {currentPattern} ({currentStatus})", style);

            style.normal.textColor = Color.white;
            GUI.Label(new Rect(boxX, boxY + 25, 400, 25), $"Hold Time Remaining: {timerDisplay:F1}s", style);
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