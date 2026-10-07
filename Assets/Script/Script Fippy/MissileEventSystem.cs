using System.Collections;
using UnityEngine;

public class MissileEventSystem : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab สัญลักษณ์แจ้งเตือน (!)")]
    public GameObject warningPrefab;

    [Tooltip("Prefab ตัวจรวดมิสไซล์")]
    public GameObject missilePrefab;

    [Header("Event Timing & Rate (อัตราการเกิด Event)")]
    [Tooltip("ระยะเวลาคูลดาวน์ก่อนสุ่มตรวจรอบใหม่ (วินาที)")]
    public float cooldownTime = 12f;

    [Range(0f, 100f)]
    [Tooltip("โอกาสเกิด Event (%)")]
    public float eventChance = 60f;

    [Header("Missile Count (ปรับจำนวนจรวดต่อรอบ)")]
    [Tooltip("จำนวนจรวดขั้นต่ำต่อ 1 Event")]
    public int minMissiles = 1;

    [Tooltip("จำนวนจรวดสูงสุดต่อ 1 Event")]
    public int maxMissiles = 3;

    [Tooltip("ระยะเวลาเว้นช่วงระหว่างการเริ่มเตือนจรวดแต่ละลูก (วินาที)")]
    public float delayBetweenMissiles = 0.8f;

    [Header("Warning Settings (ตั้งค่าการเตือนแบบ Jetpack Joyride)")]
    [Tooltip("เวลารวมทั้งหมดของการเตือนก่อนจรวดพุ่ง (วินาที)")]
    public float totalWarningDuration = 2.0f;

    [Tooltip("เวลาที่ไอคอนเตือนจะเลื่อนตามระดับแกน Y ของ Player (วินาที) ก่อนจะหยุดล็อกเป้า")]
    public float trackingDuration = 1.3f;

    [Tooltip("พิกัดแกน X ฝั่งขวาของจอสำหรับแสดงป้ายเตือนและจุดปล่อยจรวด")]
    public float spawnRightX = 9.5f;

    [Header("Y Spawn Bounds (ขอบเขตความสูงบน-ล่าง)")]
    public float minY = -3.8f;
    public float maxY = 3.8f;

    [Header("Spawners (ปิดตัวสร้างมอนสเตอร์ชั่วคราวตอนมิสไซล์มา)")]
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
            // 1. ช่วงคูลดาวน์
            eventStatus = "Cooldown";
            timerDisplay = cooldownTime;

            while (timerDisplay > 0f)
            {
                // ตรวจสอบว่า Player พร้อมเล่นหรือไม่ ถ้าตาย/ชนะ ให้หยุดรอ
                if (playerController != null && playerController.CanShootAndControl)
                {
                    timerDisplay -= Time.deltaTime;
                }
                yield return null;
            }

            // 2. สุ่มทอยโอกาสเกิด Event
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

        // ปิด Spawner ศัตรูปกติชั่วคราวเพื่อไม่ให้ขวางมิสไซล์
        if (enemySpawner != null) enemySpawner.SetActive(false);
        if (itemSpawner != null) itemSpawner.SetActive(false);

        // สุ่มจำนวนจรวดในรอบนี้
        int missileCount = Random.Range(minMissiles, maxMissiles + 1);

        for (int i = 0; i < missileCount; i++)
        {
            if (playerController == null || !playerController.CanShootAndControl) break;

            StartCoroutine(SingleMissileSequenceRoutine());
            yield return new WaitForSeconds(delayBetweenMissiles);
        }

        // รอจนกระทั่งมิสไซล์ลูกสุดท้ายพุ่งพ้นจอ
        yield return new WaitForSeconds(totalWarningDuration + 2.5f);

        // เปิด Spawner กลับมาทำงานปกติ
        if (enemySpawner != null) enemySpawner.SetActive(true);
        if (itemSpawner != null) itemSpawner.SetActive(true);
    }

    private IEnumerator SingleMissileSequenceRoutine()
    {
        if (warningPrefab == null || missilePrefab == null) yield break;

        GameObject warningObj = Instantiate(warningPrefab);
        SpriteRenderer warningRenderer = warningObj.GetComponentInChildren<SpriteRenderer>();

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
    }
}