using System.Collections;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public enum EventType
    {
        WallEvent,
        MissileEvent
    }

    [Header("Global Timing & Rate (อัตราการเกิดอีเวนต์รวม)")]
    public float cooldownTime = 12f;
    [Range(0f, 100f)] public float globalEventChance = 75f;

    [Header("Event Selection Weight")]
    [Range(1, 100)] public int wallWeight = 50;
    [Range(1, 100)] public int missileWeight = 50;

    [Header("--- Wall Event Settings ---")]
    public Transform topWall;
    public Transform bottomWall;
    public float wallMoveSpeed = 6f;
    public float slideDistance = 25f;
    public float wallPreWarningTime = 2.5f;

    [Header("--- Spawner Control in Wall Event ---")]
    public bool spawnEnemyDuringWall = false;
    public bool spawnItemDuringWall = false;

    [Header("--- Missile Event Settings ---")]
    public GameObject warningPrefab;
    public GameObject missilePrefab;
    public int minMissiles = 1;
    public int maxMissiles = 3;
    public float delayBetweenMissiles = 0.8f;
    public float totalWarningDuration = 2.0f;
    public float trackingDuration = 1.3f;
    public float missileSpawnRightX = 9.5f;
    public float minY = -3.8f;
    public float maxY = 3.8f;
    public float missileRotationZ = 0f;

    [Header("--- Spawner Control in Missile Event ---")]
    public bool spawnEnemyDuringMissile = false;
    public bool spawnItemDuringMissile = false;

    [Header("Shared Spawners")]
    public GameObject enemySpawner;
    public GameObject itemSpawner;

    [Header("Player Reference")]
    public PlayerControllerTest playerController;

    [Header("Debug Settings")]
    public bool showDebugOnScreen = true;
    public bool showGizmos = true;

    private string currentStatus = "Initializing";
    private string currentEventDetail = "None";
    private float timerDisplay = 0f;
    private float lastRollResult = -1f;
    private Vector3 topStartPos;
    private Vector3 bottomStartPos;
    private AudioManger audioManager;

    private void Start()
    {
        if (topWall != null) topStartPos = topWall.position;
        if (bottomWall != null) bottomStartPos = bottomWall.position;

        if (playerController == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerController = p.GetComponentInParent<PlayerControllerTest>();
        }

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManger>();
        }

        StartCoroutine(MasterEventLoop());
    }

    private IEnumerator MasterEventLoop()
    {
        while (true)
        {
            currentStatus = "Cooldown";
            currentEventDetail = "Waiting for next cycle";
            timerDisplay = cooldownTime;

            while (timerDisplay > 0f)
            {
                if (playerController != null && playerController.CanShootAndControl)
                {
                    timerDisplay -= Time.deltaTime;
                }
                yield return null;
            }
            timerDisplay = 0f;

            currentStatus = "Rolling Event Chance";
            lastRollResult = Random.Range(0f, 100f);

            if (lastRollResult <= globalEventChance && playerController != null && playerController.CanShootAndControl)
            {
                yield return StartCoroutine(RunRandomEvent());
            }
        }
    }

    private IEnumerator RunRandomEvent()
    {
        int totalWeight = wallWeight + missileWeight;
        int choiceRoll = Random.Range(0, totalWeight);

        if (choiceRoll < wallWeight)
        {
            SetSpawnersActive(spawnEnemyDuringWall, spawnItemDuringWall);
            yield return StartCoroutine(WallEventRoutine());
        }
        else
        {
            SetSpawnersActive(spawnEnemyDuringMissile, spawnItemDuringMissile);
            yield return StartCoroutine(MissileEventRoutine());
        }

        SetSpawnersActive(true, true);
    }

    private void SetSpawnersActive(bool enemyActive, bool itemActive)
    {
        if (enemySpawner != null) enemySpawner.SetActive(enemyActive);
        if (itemSpawner != null) itemSpawner.SetActive(itemActive);
    }

    private IEnumerator WallEventRoutine()
    {
        currentStatus = "Wall Warning";
        currentEventDetail = "Spawners paused/running based on settings";
        timerDisplay = wallPreWarningTime;

        while (timerDisplay > 0f)
        {
            timerDisplay -= Time.deltaTime;
            yield return null;
        }
        timerDisplay = 0f;

        float patternRoll = Random.Range(0f, 100f);
        bool moveTop = patternRoll < 33.33f || patternRoll >= 66.66f;
        bool moveBottom = patternRoll >= 33.33f;

        string patternName = (moveTop && moveBottom) ? "Both Walls" : (moveTop ? "Top Wall" : "Bottom Wall");
        currentStatus = "Wall Sliding Active";
        currentEventDetail = $"{patternName} (Speed: {wallMoveSpeed})";

        Vector3 topTarget = moveTop ? topStartPos + (Vector3.left * slideDistance) : topStartPos;
        Vector3 bottomTarget = moveBottom ? bottomStartPos + (Vector3.left * slideDistance) : bottomStartPos;

        while (true)
        {
            bool topReached = true;
            bool bottomReached = true;

            if (moveTop && topWall != null)
            {
                topWall.position = Vector3.MoveTowards(topWall.position, topTarget, wallMoveSpeed * Time.deltaTime);
                if (Vector3.Distance(topWall.position, topTarget) > 0.01f) topReached = false;
            }

            if (moveBottom && bottomWall != null)
            {
                bottomWall.position = Vector3.MoveTowards(bottomWall.position, bottomTarget, wallMoveSpeed * Time.deltaTime);
                if (Vector3.Distance(bottomWall.position, bottomTarget) > 0.01f) bottomReached = false;
            }

            if (topReached && bottomReached) break;
            yield return null;
        }

        if (topWall != null) topWall.position = topStartPos;
        if (bottomWall != null) bottomWall.position = bottomStartPos;
    }

    private IEnumerator MissileEventRoutine()
    {
        int count = Random.Range(minMissiles, maxMissiles + 1);
        currentStatus = "Missile Warning";
        currentEventDetail = $"Incoming: {count} Missiles";

        for (int i = 0; i < count; i++)
        {
            if (playerController == null || !playerController.CanShootAndControl) break;

            StartCoroutine(SingleMissileSequence(i + 1, count));
            yield return new WaitForSeconds(delayBetweenMissiles);
        }

        timerDisplay = totalWarningDuration + 2.5f;
        currentStatus = "Missile Firing / Active";

        while (timerDisplay > 0f)
        {
            timerDisplay -= Time.deltaTime;
            yield return null;
        }
        timerDisplay = 0f;
    }

    private IEnumerator SingleMissileSequence(int missileIndex, int totalCount)
    {
        if (warningPrefab == null || missilePrefab == null) yield break;

        GameObject warningObj = Instantiate(warningPrefab);
        SpriteRenderer warningRenderer = warningObj.GetComponentInChildren<SpriteRenderer>();

        // ⭐ 1. เริ่มเล่นเสียง Alert ผ่านระบบคุมเสียง
        if (audioManager != null)
        {
            audioManager.PlayAlert();
        }

        float elapsed = 0f;
        float lockedY = (playerController != null) ? playerController.transform.position.y : 0f;

        while (elapsed < trackingDuration)
        {
            elapsed += Time.deltaTime;
            if (playerController != null)
            {
                lockedY = Mathf.Clamp(playerController.transform.position.y, minY, maxY);
            }

            warningObj.transform.position = new Vector3(missileSpawnRightX, lockedY, 0f);
            if (warningRenderer != null)
            {
                warningRenderer.enabled = (Mathf.PingPong(elapsed * 5f, 1f) > 0.4f);
            }
            yield return null;
        }

        float remainingTime = totalWarningDuration - trackingDuration;
        float flashTimer = 0f;

        while (flashTimer < remainingTime)
        {
            flashTimer += Time.deltaTime;
            warningObj.transform.position = new Vector3(missileSpawnRightX, lockedY, 0f);
            if (warningRenderer != null)
            {
                warningRenderer.enabled = (Mathf.PingPong(flashTimer * 12f, 1f) > 0.3f);
            }
            yield return null;
        }

        // ⭐ 2. เมื่อการแจ้งเตือนสิ้นสุด: สั่งตัดเสียง Alert ทันที และลบป้ายเตือน
        if (audioManager != null)
        {
            audioManager.StopAlert();
        }

        Destroy(warningObj);

        // ⭐ 3. เสกมิสไซล์ยิงออกไป
        Vector3 spawnPos = new Vector3(missileSpawnRightX + 1f, lockedY, 0f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, missileRotationZ);
        Instantiate(missilePrefab, spawnPos, spawnRotation);

        if (audioManager != null && audioManager.MidSide != null)
        {
            audioManager.PlaySFX(audioManager.MidSide);
        }
    }

    private void OnDisable()
    {
        // ป้องกันเสียงค้างหากปิดสคริปต์กลางคัน
        if (audioManager != null)
        {
            audioManager.ForceStopAlert();
        }
    }

    private void OnGUI()
    {
        if (!showDebugOnScreen) return;

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12
        };

        float startX = 20f;
        float startY = Screen.height - 140f;

        if (currentStatus.Contains("Warning"))
        {
            headerStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(startX, startY, 450, 22), $"[EVENT ALERT] {currentStatus} (in: {timerDisplay:F1}s)", headerStyle);
        }
        else if (currentStatus.Contains("Active") || currentStatus.Contains("Firing"))
        {
            headerStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(startX, startY, 450, 22), $"[EVENT RUNNING] {currentStatus}", headerStyle);
        }
        else
        {
            headerStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(startX, startY, 450, 22), $"[STATUS] {currentStatus} (Next check: {timerDisplay:F1}s)", headerStyle);
        }

        subStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(startX, startY + 24, 450, 20), $"Detail: {currentEventDetail}", subStyle);

        int totalWeight = Mathf.Max(1, wallWeight + missileWeight);
        float wallRate = (wallWeight / (float)totalWeight) * 100f;
        float missileRate = (missileWeight / (float)totalWeight) * 100f;
        string lastRollStr = lastRollResult >= 0 ? $"{lastRollResult:F1}%" : "-";

        subStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(startX, startY + 46, 500, 20), $"Global Chance: {globalEventChance}% | Last Roll: {lastRollStr}", subStyle);
        GUI.Label(new Rect(startX, startY + 66, 500, 20), $"Weights -> Wall: {wallRate:F0}% | Missile: {missileRate:F0}%", subStyle);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        if (topWall != null)
        {
            Vector3 origin = Application.isPlaying ? topStartPos : topWall.position;
            Vector3 target = origin + Vector3.left * slideDistance;
            Gizmos.DrawLine(origin, target);
            Gizmos.DrawWireCube(target, topWall.localScale);
        }
        if (bottomWall != null)
        {
            Vector3 origin = Application.isPlaying ? bottomStartPos : bottomWall.position;
            Vector3 target = origin + Vector3.left * slideDistance;
            Gizmos.DrawLine(origin, target);
            Gizmos.DrawWireCube(target, bottomWall.localScale);
        }

        Gizmos.color = Color.red;
        Vector3 topSpawnPoint = new Vector3(missileSpawnRightX, maxY, 0f);
        Vector3 bottomSpawnPoint = new Vector3(missileSpawnRightX, minY, 0f);
        Gizmos.DrawLine(topSpawnPoint, bottomSpawnPoint);
        Gizmos.DrawWireSphere(topSpawnPoint, 0.3f);
        Gizmos.DrawWireSphere(bottomSpawnPoint, 0.3f);
    }
}