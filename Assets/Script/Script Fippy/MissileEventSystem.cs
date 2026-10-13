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
    private string eventStatus = "Cooldown";

    private void Start()
    {
        if (playerController == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerController = p.GetComponentInParent<PlayerControllerTest>();
        }

        StartCoroutine(MissileEventLoop());
    }

    private IEnumerator MissileEventLoop()
    {
        while (true)
        {
            eventStatus = "Cooldown";
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
        eventStatus = "Missile Incoming!";

        if (enemySpawner != null) enemySpawner.SetActive(false);
        if (itemSpawner != null) itemSpawner.SetActive(false);

        int missileCount = Random.Range(minMissiles, maxMissiles + 1);

        for (int i = 0; i < missileCount; i++)
        {
            if (playerController == null || !playerController.CanShootAndControl) break;

            StartCoroutine(SingleMissileSequenceRoutine());
            yield return new WaitForSeconds(delayBetweenMissiles);
        }

        yield return new WaitForSeconds(totalWarningDuration + 2.5f);

        if (enemySpawner != null) enemySpawner.SetActive(true);
        if (itemSpawner != null) itemSpawner.SetActive(true);
    }

    private IEnumerator SingleMissileSequenceRoutine()
    {
        if (warningPrefab == null || missilePrefab == null) yield break;

        GameObject warningObj = Instantiate(warningPrefab);
        SpriteRenderer warningRenderer = warningObj.GetComponentInChildren<SpriteRenderer>();

        // ⭐ 1. เล่นเสียงเตือน Alert สำหรับจรวดลูกนี้
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            AudioManger audioManager = audioObj.GetComponent<AudioManger>();
            if (audioManager != null && audioManager.Alert != null)
            {
                audioManager.PlaySFX(audioManager.Alert);
            }
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

        // ปล่อยจรวดมิสไซล์
        Vector3 spawnPos = new Vector3(spawnRightX + 1f, lockedY, 0f);
        Instantiate(missilePrefab, spawnPos, Quaternion.identity);

        // ⭐ 2. เล่นเสียงพุ่ง MidSide สำหรับจรวดลูกนี้
        if (audioObj != null)
        {
            AudioManger audioManager = audioObj.GetComponent<AudioManger>();
            if (audioManager != null && audioManager.MidSide != null)
            {
                audioManager.PlaySFX(audioManager.MidSide);
            }
        }
    }
}