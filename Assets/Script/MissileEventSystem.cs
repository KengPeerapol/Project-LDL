using System.Collections;
using UnityEngine;

public class MissileEventSystem : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject warningPrefab;
    public GameObject missilePrefab;

    [Header("Event Timing & Rate (อัตราการเกิด Event)")]
    public float cooldownTime = 12f;

    [Range(0f, 100f)]
    public float eventChance = 60f;

    [Header("Missile Count (ปรับจำนวนจรวดต่อรอบ)")]
    public int minMissiles = 1;
    public int maxMissiles = 3;
    public float delayBetweenMissiles = 0.8f;

    [Header("Warning Settings")]
    public float totalWarningDuration = 2.0f;
    public float trackingDuration = 1.3f;
    public float spawnRightX = 9.5f;

    [Header("Y Spawn Bounds")]
    public float minY = -3.8f;
    public float maxY = 3.8f;

    [Header("Spawners")]
    public GameObject enemySpawner;
    public GameObject itemSpawner;

    [Header("Player Reference")]
    public PlayerControllerTest playerController;

    private float timerDisplay = 0f;
    private AudioManger cachedAudioManager;
    private WaitForSeconds cachedDelayBetweenMissiles;
    private WaitForSeconds cachedPostWaveWait;

    private void Awake()
    {
        // แคช Yield Instruction เพื่อ Zero GC Alloc
        cachedDelayBetweenMissiles = new WaitForSeconds(delayBetweenMissiles);
        cachedPostWaveWait = new WaitForSeconds(totalWarningDuration + 2.5f);
    }

    private void Start()
    {
        if (playerController == null)
        {
            playerController = Object.FindAnyObjectByType<PlayerControllerTest>();
        }

        // แคช Audio Manager ครั้งเดียว ไม่เรียก Find ในลูป
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            cachedAudioManager = audioObj.GetComponent<AudioManger>();
        }

        StartCoroutine(MissileEventLoop());
    }

    private IEnumerator MissileEventLoop()
    {
        while (true)
        {
            timerDisplay = cooldownTime;

            while (timerDisplay > 0f)
            {
                if (playerController != null && playerController.CanShootAndControl)
                {
                    timerDisplay -= Time.deltaTime;
                }
                yield return null;
            }

            float roll = Random.Range(0f, 100f);
            if (roll <= eventChance && playerController != null && playerController.CanShootAndControl)
            {
                yield return StartCoroutine(TriggerMissileWaveRoutine());
            }
        }
    }

    private IEnumerator TriggerMissileWaveRoutine()
    {
        if (enemySpawner != null) enemySpawner.SetActive(false);
        if (itemSpawner != null) itemSpawner.SetActive(false);

        int missileCount = Random.Range(minMissiles, maxMissiles + 1);

        for (int i = 0; i < missileCount; i++)
        {
            if (playerController == null || !playerController.CanShootAndControl) break;

            StartCoroutine(SingleMissileSequenceRoutine());
            yield return cachedDelayBetweenMissiles;
        }

        yield return cachedPostWaveWait;

        if (enemySpawner != null) enemySpawner.SetActive(true);
        if (itemSpawner != null) itemSpawner.SetActive(true);
    }

    private IEnumerator SingleMissileSequenceRoutine()
    {
        if (warningPrefab == null || missilePrefab == null) yield break;

        GameObject warningObj = Instantiate(warningPrefab);
        SpriteRenderer warningRenderer = warningObj.GetComponentInChildren<SpriteRenderer>();

        // เล่นเสียงแจ้งเตือนผ่าน Reference ที่แคชไว้
        if (cachedAudioManager != null && cachedAudioManager.Alert != null)
        {
            cachedAudioManager.PlaySFX(cachedAudioManager.Alert);
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

            warningObj.transform.position = new Vector3(spawnRightX, lockedY, 0f);

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
            warningObj.transform.position = new Vector3(spawnRightX, lockedY, 0f);

            if (warningRenderer != null)
            {
                warningRenderer.enabled = (Mathf.PingPong(flashTimer * 12f, 1f) > 0.3f);
            }

            yield return null;
        }

        Destroy(warningObj);

        Vector3 spawnPos = new Vector3(spawnRightX + 1f, lockedY, 0f);
        Instantiate(missilePrefab, spawnPos, Quaternion.identity);

        if (cachedAudioManager != null && cachedAudioManager.MidSide != null)
        {
            cachedAudioManager.PlaySFX(cachedAudioManager.MidSide);
        }
    }
}