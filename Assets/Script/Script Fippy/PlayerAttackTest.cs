using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerAttackTest : MonoBehaviour
{
    public enum ChargeMode
    {
        Projectile, // แบบที่ 1: กระสุนเด้งกำแพง
        LaserBeam   // แบบที่ 2: ลำแสงเลเซอร์ (เด้งกำแพงได้)
    }

    private struct LaserSegment
    {
        public Vector2 start;
        public Vector2 end;
        public float length;
        public Vector2 direction;
    }

    [Header("Gun & Aiming Settings")]
    public Transform gunPivot;
    public float gunRotationOffset = -90f;

    [Header("Shooting Animation")]
    public Animator gunAnimator;
    public string fireAnimationName = "fire";

    [Header("Shooting Settings (ยิงปกติ)")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.25f;
    public float firePointDistance = 0.7f;

    [Header("Aim Angle Clamp (จำกัดมุมชี้ด้านหน้า)")]
    [Range(0f, 360f)]
    public float maxAimAngle = 80f;

    [Header("Charge Attack Settings (ระบบชาร์จยิง)")]
    public bool enableChargeAttack = true;

    [Tooltip("โหมดชาร์จยิงปัจจุบัน (สลับได้ผ่านปุ่ม UI หรือคีย์ลัด)")]
    public ChargeMode chargeMode = ChargeMode.Projectile;

    public float chargeTimeRequired = 1.0f;
    public float chargeCooldown = 5.0f;
    public int normalDamage = 25;
    public int chargedDamage = 100;

    [Header("--- UI Mode Switch Settings (สลับโหมดการยิง) ---")]
    public Key toggleModeKey = Key.Q;
    public Button switchModeButton;
    public TextMeshProUGUI modeDisplayText;
    public bool showOnScreenToggleButton = true;

    [Header("--- Mode 1: Projectile Settings (ชาร์จยิงแบบกระสุน) ---")]
    public GameObject chargedBulletPrefab;
    public float chargedBulletScale = 1.0f;
    public int chargedBulletBounces = 3;

    [Header("--- Mode 2: Laser Beam Settings (ชาร์จยิงแบบลำแสงเลเซอร์) ---")]
    [Tooltip("ระยะยิงรวมไกลสุดของลำแสงเลเซอร์")]
    public float laserRange = 35f;

    [Tooltip("ความหนาของลำแสงเลเซอร์")]
    public float laserBeamWidth = 0.45f;

    [Tooltip("ระยะเวลาที่ลำแสงเลเซอร์อยู่ค้างบนหน้าจอ (วินาที)")]
    public float laserDuration = 0.6f;

    public Color laserColor = new Color(1f, 0.2f, 0.2f, 0.95f);

    [Tooltip("จำนวนครั้งที่ลำแสงเลเซอร์สามารถเด้งสะท้อนกำแพงได้")]
    public int laserBounces = 2;

    [Header("Gun Scale Settings (เอฟเฟกต์ปืนใหญ่ขึ้นตอนชาร์จ 100%)")]
    public bool enableGunScale = true;
    public Transform gunVisual;
    public float chargedGunScale = 1.5f;
    public float gunScaleSmoothSpeed = 14f;

    [Header("Slow Motion Settings (สโลว์เกมขณะชาร์จ)")]
    public bool enableSlowMotion = true;
    [Range(0.05f, 1f)]
    public float slowMotionTimeScale = 0.25f;
    public float slowMotionDelay = 0.3f;

    [Header("Bounce Settings (การเด้งกำแพงของกระสุนธรรมดา)")]
    public int normalBulletBounces = 0;

    [Header("Trajectory Preview Settings (เส้นเล็งนำวิถี)")]
    public bool enableTrajectory = true;
    public bool showOnlyAtFullCharge = true;
    public LineRenderer trajectoryLine;
    public LayerMask wallLayer = ~0;
    public float maxTrajectoryDistance = 35f;
    public float trajectoryWidth = 0.08f;

    [Tooltip("⭐ สีของเส้นเล็งเมื่ออยู่ในโหมดกระสุน (Bullet)")]
    public Color bulletTrajectoryColor = new Color(0f, 1f, 1f, 0.85f); // สีฟ้า Cyan

    [Tooltip("⭐ สีของเส้นเล็งเมื่ออยู่ในโหมดเลเซอร์ (Laser)")]
    public Color laserTrajectoryColor = new Color(1f, 0.35f, 0.05f, 0.9f); // สีส้ม/แดงนีออน

    private float nextFireTime = 0f;
    private float currentChargeTimer = 0f;
    private float chargeCooldownTimer = 0f;
    private bool isCharging = false;
    private bool hasLoggedFullCharge = false;
    private bool isSlowMoActive = false;
    private Vector3 originalGunScale = Vector3.one;

    private Camera mainCamera;
    private Vector2 currentAimDirection = Vector2.right;

    private PlayerHealthTest playerHealthTest;
    private PlayerControllerTest playerController;
    private LineRenderer laserLineRenderer;
    private Coroutine activeLaserCoroutine;

    private void Start()
    {
        mainCamera = Camera.main;
        playerHealthTest = GetComponent<PlayerHealthTest>();
        playerController = GetComponent<PlayerControllerTest>();

        if (gunVisual == null)
        {
            if (gunPivot != null)
            {
                Transform childGun = gunPivot.Find("Gun");
                gunVisual = (childGun != null) ? childGun : gunPivot;
            }
            else
            {
                gunVisual = transform;
            }
        }

        if (gunVisual != null)
        {
            originalGunScale = gunVisual.localScale;
        }

        if (gunAnimator == null)
        {
            if (gunPivot != null)
            {
                gunAnimator = gunPivot.GetComponent<Animator>();
                if (gunAnimator == null)
                {
                    gunAnimator = gunPivot.GetComponentInChildren<Animator>();
                }
            }

            if (gunAnimator == null)
            {
                gunAnimator = GetComponentInChildren<Animator>();
            }
        }

        SetupTrajectoryLineRenderer();
        SetupLaserLineRenderer();

        if (switchModeButton != null)
        {
            switchModeButton.onClick.AddListener(ToggleChargeMode);
        }

        UpdateModeUI();
    }

    private void SetupTrajectoryLineRenderer()
    {
        if (trajectoryLine == null)
        {
            GameObject lineObj = new GameObject("ChargeTrajectoryLine");
            lineObj.transform.SetParent(transform);
            trajectoryLine = lineObj.AddComponent<LineRenderer>();

            Shader defaultShader = Shader.Find("Sprites/Default");
            if (defaultShader != null)
            {
                trajectoryLine.material = new Material(defaultShader);
            }

            trajectoryLine.startWidth = trajectoryWidth;
            trajectoryLine.endWidth = trajectoryWidth;
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.sortingOrder = 15;
            trajectoryLine.positionCount = 0;
            trajectoryLine.enabled = false;
        }
        else
        {
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.enabled = false;
        }
    }

    private void SetupLaserLineRenderer()
    {
        // สร้าง Laser แยกอิสระเพื่อไม่ให้หมุนตามตัว Player
        GameObject laserObj = new GameObject("LaserBeamRenderer");
        laserLineRenderer = laserObj.AddComponent<LineRenderer>();

        Shader defaultShader = Shader.Find("Sprites/Default");
        if (defaultShader != null)
        {
            laserLineRenderer.material = new Material(defaultShader);
        }

        laserLineRenderer.startColor = laserColor;
        laserLineRenderer.endColor = laserColor;
        laserLineRenderer.startWidth = laserBeamWidth;
        laserLineRenderer.endWidth = laserBeamWidth;
        laserLineRenderer.useWorldSpace = true; // ⭐ ยึดตำแหน่งโลก
        laserLineRenderer.sortingOrder = 20;
        laserLineRenderer.positionCount = 0;
        laserLineRenderer.enabled = false;
    }

    private void Update()
    {
        if (PauseMenuManager.isGamePaused || (Time.timeScale <= 0f && !isSlowMoActive))
        {
            ResetSlowMotion();
            ResetGunScale();
            HideTrajectory();
            isCharging = false;
            currentChargeTimer = 0f;
            return;
        }

        if (IsPlayerDead() || (playerController != null && !playerController.CanShootAndControl))
        {
            ResetSlowMotion();
            ResetGunScale();
            HideTrajectory();
            currentChargeTimer = 0f;
            isCharging = false;
            return;
        }

        CheckModeSwitchInput();
        UpdateCooldown();
        HandleMouseAiming();
        HandleShootingInput();
        UpdateGunScale();
        UpdateTrajectoryPreview();
    }

    private void CheckModeSwitchInput()
    {
        if (Keyboard.current != null && Keyboard.current[toggleModeKey].wasPressedThisFrame)
        {
            ToggleChargeMode();
        }
    }

    public void ToggleChargeMode()
    {
        chargeMode = (chargeMode == ChargeMode.Projectile) ? ChargeMode.LaserBeam : ChargeMode.Projectile;
        UpdateModeUI();
        Debug.Log($"<color=yellow>[Weapon Mode] สลับโหมดการยิงเป็น: {chargeMode}</color>");
    }

    public void SetChargeMode(int modeIndex)
    {
        chargeMode = (ChargeMode)modeIndex;
        UpdateModeUI();
    }

    private void UpdateModeUI()
    {
        if (modeDisplayText != null)
        {
            string modeName = (chargeMode == ChargeMode.Projectile) ? "Bouncing Bullet" : "Bouncing Laser";
            modeDisplayText.text = $"Mode: {modeName} (Press [{toggleModeKey}])";
            modeDisplayText.color = (chargeMode == ChargeMode.Projectile) ? bulletTrajectoryColor : laserTrajectoryColor;
        }
    }

    private void UpdateCooldown()
    {
        if (chargeCooldownTimer > 0f)
        {
            chargeCooldownTimer -= Time.unscaledDeltaTime;

            if (chargeCooldownTimer <= 0f)
            {
                chargeCooldownTimer = 0f;
                Debug.Log("<color=green>[Charged Shot] ชาร์จยิงพร้อมใช้งานแล้ว! (Ready)</color>");
            }
        }
    }

    private bool IsPlayerDead()
    {
        if (playerHealthTest != null && playerHealthTest.currentHealth <= 0f)
        {
            return true;
        }
        return false;
    }

    private void HandleMouseAiming()
    {
        if (mainCamera == null || Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 pivotOrigin = (gunPivot != null) ? (Vector2)gunPivot.position : (Vector2)transform.position;

        Vector2 rawDirection = (Vector2)mouseWorldPos - pivotOrigin;
        float targetAngle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;

        targetAngle = Mathf.Clamp(targetAngle, -maxAimAngle, maxAimAngle);

        float rad = targetAngle * Mathf.Deg2Rad;
        currentAimDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        if (gunPivot != null)
        {
            gunPivot.rotation = Quaternion.Euler(0f, 0f, targetAngle + gunRotationOffset);
        }

        if (firePoint != null && firePoint.parent != gunPivot)
        {
            firePoint.position = pivotOrigin + (currentAimDirection * firePointDistance);
        }
    }

    private void HandleShootingInput()
    {
        if (Mouse.current == null) return;

        if (!enableChargeAttack)
        {
            ResetSlowMotion();
            ResetGunScale();
            HideTrajectory();
            isCharging = false;
            currentChargeTimer = 0f;

            if (Mouse.current.leftButton.wasPressedThisFrame && Time.unscaledTime >= nextFireTime)
            {
                FireBullet(bulletPrefab, normalDamage, 1.0f, isPiercing: false, bounces: normalBulletBounces);
                nextFireTime = Time.unscaledTime + fireRate;
            }
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isCharging = true;
            currentChargeTimer = 0f;
            hasLoggedFullCharge = false;
        }

        if (Mouse.current.leftButton.isPressed && isCharging)
        {
            currentChargeTimer += Time.unscaledDeltaTime;

            if (enableSlowMotion && !isSlowMoActive && chargeCooldownTimer <= 0f && currentChargeTimer >= slowMotionDelay)
            {
                ApplySlowMotion(true);
            }

            if (currentChargeTimer >= chargeTimeRequired && !hasLoggedFullCharge)
            {
                hasLoggedFullCharge = true;
                if (chargeCooldownTimer <= 0f)
                {
                    Debug.Log("<color=cyan>[Charged Shot] ชาร์จเต็ม 100%! พร้อมยิง</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=orange>[Charged Shot] ชาร์จเต็ม แต่ติดคูลดาวน์! เหลือ {chargeCooldownTimer:F1} วิ</color>");
                }
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isCharging)
        {
            ResetSlowMotion();
            HideTrajectory();
            isCharging = false;

            if (currentChargeTimer >= chargeTimeRequired)
            {
                if (chargeCooldownTimer <= 0f)
                {
                    if (chargeMode == ChargeMode.Projectile)
                    {
                        GameObject spawnPrefab = (chargedBulletPrefab != null) ? chargedBulletPrefab : bulletPrefab;
                        FireBullet(spawnPrefab, chargedDamage, chargedBulletScale, isPiercing: true, bounces: chargedBulletBounces);
                        Debug.Log($"<color=red>[Charged Shot] ยิงกระสุนชาร์จ! ดาเมจ {chargedDamage}</color>");
                    }
                    else if (chargeMode == ChargeMode.LaserBeam)
                    {
                        FireLaserBeam();
                        Debug.Log($"<color=red>[Charged Shot] ยิงเลเซอร์สะท้อนกำแพง! ดาเมจ {chargedDamage}</color>");
                    }

                    chargeCooldownTimer = chargeCooldown;
                }
                else
                {
                    FireBullet(bulletPrefab, normalDamage, 1.0f, isPiercing: false, bounces: normalBulletBounces);
                    Debug.LogWarning("[Charged Shot] ติดคูลดาวน์ ยิงเป็นกระสุนธรรมดาแทน");
                }
            }
            else if (Time.unscaledTime >= nextFireTime)
            {
                FireBullet(bulletPrefab, normalDamage, 1.0f, isPiercing: false, bounces: normalBulletBounces);
                nextFireTime = Time.unscaledTime + fireRate;
            }

            currentChargeTimer = 0f;
            hasLoggedFullCharge = false;
        }
    }

    // ⭐ คำนวณพิกัดเลเซอร์เป็นภาพนิ่ง ณ จังหวะที่ยิง (Snapshot) เพื่อไม่ให้ขยับตามปืน
    private void FireLaserBeam()
    {
        Vector2 startPos = (firePoint != null) ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 startDir = currentAimDirection;

        List<Vector3> staticPoints = new List<Vector3>();
        List<LaserSegment> staticSegments = new List<LaserSegment>();

        staticPoints.Add(startPos);

        Vector2 currentPos = startPos;
        Vector2 currentDir = startDir;
        int bouncesLeft = laserBounces;
        float remainingRange = laserRange;

        for (int i = 0; i <= laserBounces; i++)
        {
            RaycastHit2D[] wallHits = Physics2D.RaycastAll(currentPos, currentDir, remainingRange, wallLayer);
            System.Array.Sort(wallHits, (a, b) => a.distance.CompareTo(b.distance));

            bool hitWall = false;
            RaycastHit2D wallHit = new RaycastHit2D();

            foreach (var hit in wallHits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.transform.root == transform.root) continue;
                if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Bullet")) continue;
                if (hit.distance < 0.02f) continue;

                if (hit.collider.CompareTag("Wall"))
                {
                    wallHit = hit;
                    hitWall = true;
                    break;
                }
            }

            float segmentDist = hitWall ? wallHit.distance : remainingRange;
            Vector2 segmentEnd = hitWall ? wallHit.point : (currentPos + (currentDir * segmentDist));

            staticSegments.Add(new LaserSegment
            {
                start = currentPos,
                end = segmentEnd,
                length = segmentDist,
                direction = currentDir
            });

            staticPoints.Add(segmentEnd);

            if (hitWall)
            {
                remainingRange -= wallHit.distance;

                if (bouncesLeft > 0 && remainingRange > 0.1f)
                {
                    bouncesLeft--;

                    Vector2 normal = wallHit.normal;
                    if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x))
                    {
                        normal = new Vector2(0f, Mathf.Sign(normal.y));
                    }
                    else
                    {
                        normal = new Vector2(Mathf.Sign(normal.x), 0f);
                    }

                    currentDir = Vector2.Reflect(currentDir, normal).normalized;
                    currentPos = wallHit.point + (normal * 0.02f);
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (activeLaserCoroutine != null)
        {
            StopCoroutine(activeLaserCoroutine);
        }
        activeLaserCoroutine = StartCoroutine(StaticLaserRoutine(staticPoints, staticSegments));

        PlayShootAnimation();

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            AudioManger audioManager = audioObj.GetComponent<AudioManger>();
            if (audioManager != null && audioManager.Shoot != null)
            {
                audioManager.PlaySFX(audioManager.Shoot);
            }
        }
    }

    // ⭐ แอนิเมชันเลเซอร์ค้างอยู่กับที่ในฉาก ไม่หมุนและไม่เคลื่อนที่ตามผู้เล่น
    private IEnumerator StaticLaserRoutine(List<Vector3> points, List<LaserSegment> segments)
    {
        laserLineRenderer.positionCount = points.Count;
        laserLineRenderer.SetPositions(points.ToArray());
        laserLineRenderer.enabled = true;

        HashSet<EnemyHealthTest> damagedEnemies = new HashSet<EnemyHealthTest>();
        float elapsed = 0f;

        while (elapsed < laserDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            // ตรวจจับศัตรูตามแนวพิกัดเดิมที่ตรึงไว้
            for (int i = 0; i < segments.Count; i++)
            {
                LaserSegment seg = segments[i];
                RaycastHit2D[] enemyHits = Physics2D.CircleCastAll(seg.start, laserBeamWidth * 0.5f, seg.direction, seg.length);

                foreach (var hit in enemyHits)
                {
                    if (hit.collider == null) continue;
                    if (hit.collider.CompareTag("Enemy") || hit.collider.transform.root.CompareTag("Enemy"))
                    {
                        EnemyHealthTest enemy = hit.collider.GetComponentInParent<EnemyHealthTest>();
                        if (enemy != null && !damagedEnemies.Contains(enemy))
                        {
                            damagedEnemies.Add(enemy);
                            enemy.TakeDamage(chargedDamage);
                        }
                    }
                }
            }

            // ค่อยๆ ลดความหนาของลำแสงจนจางหายไป
            float progress = elapsed / laserDuration;
            float currentWidth = Mathf.Lerp(laserBeamWidth, 0.03f, progress);
            laserLineRenderer.startWidth = currentWidth;
            laserLineRenderer.endWidth = currentWidth;

            yield return null;
        }

        laserLineRenderer.enabled = false;
        laserLineRenderer.positionCount = 0;
        activeLaserCoroutine = null;
    }

    private void UpdateGunScale()
    {
        if (gunVisual == null) return;

        if (!enableGunScale || !enableChargeAttack)
        {
            gunVisual.localScale = originalGunScale;
            return;
        }

        bool isFullyCharged = isCharging && (currentChargeTimer >= chargeTimeRequired) && (chargeCooldownTimer <= 0f);
        Vector3 targetScale = isFullyCharged ? (originalGunScale * chargedGunScale) : originalGunScale;

        gunVisual.localScale = Vector3.Lerp(gunVisual.localScale, targetScale, Time.unscaledDeltaTime * gunScaleSmoothSpeed);
    }

    private void ResetGunScale()
    {
        if (gunVisual != null)
        {
            gunVisual.localScale = originalGunScale;
        }
    }

    // ⭐ อัปเดตสีและความยาวของเส้นเล็งตามโหมดปัจจุบัน
    private void UpdateTrajectoryPreview()
    {
        if (!enableTrajectory || !enableChargeAttack)
        {
            HideTrajectory();
            return;
        }

        bool canShow = false;
        if (isCharging && chargeCooldownTimer <= 0f)
        {
            canShow = showOnlyAtFullCharge ? (currentChargeTimer >= chargeTimeRequired) : true;
        }

        if (canShow)
        {
            if (chargeMode == ChargeMode.Projectile)
            {
                // โหมดกระสุน: ใช้สี bulletTrajectoryColor
                ApplyTrajectoryColor(bulletTrajectoryColor);
                DrawBounceTrajectory(chargedBulletBounces, maxTrajectoryDistance);
            }
            else if (chargeMode == ChargeMode.LaserBeam)
            {
                // โหมดเลเซอร์: ใช้สี laserTrajectoryColor
                ApplyTrajectoryColor(laserTrajectoryColor);
                DrawBounceTrajectory(laserBounces, laserRange);
            }
        }
        else
        {
            HideTrajectory();
        }
    }

    private void ApplyTrajectoryColor(Color col)
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.startColor = col;
            trajectoryLine.endColor = col;
        }
    }

    private void DrawBounceTrajectory(int maxBounces, float totalDistance)
    {
        if (trajectoryLine == null) return;

        Vector2 currentPos = (firePoint != null) ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 currentDir = currentAimDirection;

        List<Vector3> points = new List<Vector3>();
        points.Add(currentPos);

        int bouncesLeft = maxBounces;
        float remainingRange = totalDistance;

        for (int i = 0; i <= maxBounces; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(currentPos, currentDir, remainingRange, wallLayer);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool hitWall = false;
            RaycastHit2D wallHit = new RaycastHit2D();

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.transform.root == transform.root) continue;
                if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Bullet")) continue;
                if (hit.distance < 0.02f) continue;

                if (hit.collider.CompareTag("Wall"))
                {
                    wallHit = hit;
                    hitWall = true;
                    break;
                }
            }

            if (hitWall)
            {
                points.Add(wallHit.point);
                remainingRange -= wallHit.distance;

                if (bouncesLeft > 0 && remainingRange > 0.1f)
                {
                    bouncesLeft--;

                    Vector2 normal = wallHit.normal;
                    if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x))
                    {
                        normal = new Vector2(0f, Mathf.Sign(normal.y));
                    }
                    else
                    {
                        normal = new Vector2(Mathf.Sign(normal.x), 0f);
                    }

                    currentDir = Vector2.Reflect(currentDir, normal).normalized;
                    currentPos = wallHit.point + (normal * 0.02f);
                }
                else
                {
                    break;
                }
            }
            else
            {
                points.Add(currentPos + (currentDir * remainingRange));
                break;
            }
        }

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());
        trajectoryLine.enabled = true;
    }

    private void HideTrajectory()
    {
        if (trajectoryLine != null && trajectoryLine.enabled)
        {
            trajectoryLine.positionCount = 0;
            trajectoryLine.enabled = false;
        }
    }

    private void FireBullet(GameObject prefabToSpawn, int damage, float scale, bool isPiercing = false, int bounces = 0)
    {
        if (prefabToSpawn == null || firePoint == null) return;

        GameObject newBullet = Instantiate(prefabToSpawn, firePoint.position, Quaternion.identity);
        BulletTest bulletScript = newBullet.GetComponent<BulletTest>();
        if (bulletScript != null)
        {
            bulletScript.Setup(currentAimDirection, damage, scale, isPiercing, bounces);
        }

        PlayShootAnimation();

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            AudioManger audioManager = audioObj.GetComponent<AudioManger>();
            if (audioManager != null && audioManager.Shoot != null)
            {
                audioManager.PlaySFX(audioManager.Shoot);
            }
        }
    }

    private void PlayShootAnimation()
    {
        if (gunAnimator != null)
        {
            gunAnimator.Play(fireAnimationName, 0, 0f);
        }
    }

    private void ApplySlowMotion(bool enable)
    {
        if (enable)
        {
            Time.timeScale = slowMotionTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            isSlowMoActive = true;
        }
        else
        {
            ResetSlowMotion();
        }
    }

    private void ResetSlowMotion()
    {
        if (isSlowMoActive)
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f;
            isSlowMoActive = false;
        }
    }

    private void OnDisable()
    {
        ResetSlowMotion();
        ResetGunScale();
        HideTrajectory();
        if (laserLineRenderer != null) laserLineRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        ResetSlowMotion();
        ResetGunScale();
        HideTrajectory();
        if (laserLineRenderer != null && laserLineRenderer.gameObject != null)
        {
            Destroy(laserLineRenderer.gameObject);
        }
    }

    private void OnGUI()
    {
        if (PauseMenuManager.isGamePaused) return;
        if (playerController != null && !playerController.CanShootAndControl) return;
        if (!enableChargeAttack) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;

        string modeName = (chargeMode == ChargeMode.Projectile) ? "BULLET (เด้งกำแพง)" : "BOUNCING LASER (เลเซอร์เด้ง)";

        if (chargeCooldownTimer > 0f)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(20, 20, 400, 30), $"Charged Shot CD: {chargeCooldownTimer:F1}s", style);
        }
        else
        {
            style.normal.textColor = Color.green;
            GUI.Label(new Rect(20, 20, 400, 30), "Charged Shot: READY", style);
        }

        if (isCharging)
        {
            style.normal.textColor = Color.yellow;
            float chargePercent = Mathf.Clamp01(currentChargeTimer / chargeTimeRequired) * 100f;
            string status = (chargePercent >= 100f) ? "FULL (100%)" : $"{chargePercent:F0}%";
            string slowMoStatus = isSlowMoActive ? "[Slow-Mo]" : "[Normal Speed]";
            GUI.Label(new Rect(20, 50, 450, 30), $"Charging [{modeName}] {slowMoStatus}: {status}", style);
        }

        if (showOnScreenToggleButton)
        {
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 14;
            btnStyle.fontStyle = FontStyle.Bold;

            float btnX = Screen.width - 240f;
            float btnY = 20f;

            string btnText = (chargeMode == ChargeMode.Projectile)
                ? $"[Q] Mode: Bullet\n(คลิกสลับเป็น Laser)"
                : $"[Q] Mode: Laser\n(คลิกสลับเป็น Bullet)";

            if (GUI.Button(new Rect(btnX, btnY, 210, 48), btnText, btnStyle))
            {
                ToggleChargeMode();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, (Vector3)currentAimDirection * 2f);
    }

    public void ReduceChargeCooldown(float reductionTime)
    {
        if (chargeCooldownTimer > 0f)
        {
            chargeCooldownTimer = Mathf.Max(0f, chargeCooldownTimer - reductionTime);

            if (chargeCooldownTimer <= 0f)
            {
                Debug.Log("<color=green>[Item] ลดคูลดาวน์! ปืนพร้อมชาร์จยิงทันที!</color>");
            }
            else
            {
                Debug.Log($"<color=cyan>[Item] ลดคูลดาวน์ลง {reductionTime}s เหลืออีก {chargeCooldownTimer:F1}s</color>");
            }
        }
    }
}