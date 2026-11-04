using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIBarPixelEffect : MonoBehaviour
{
    private struct PixelParticle
    {
        public RectTransform rect;
        public Image image;
        public Vector2 velocity;
        public float lifeTime;
        public float maxLifeTime;
        public bool active;
    }

    [Header("Shake Settings (การสั่นของหลอด)")]
    [Tooltip("ความแรงในการสั่น (พิกเซล)")]
    public float shakeIntensity = 8f;
    [Tooltip("ระยะเวลาการสั่น (วินาที)")]
    public float shakeDuration = 0.2f;

    [Header("Pixel Burst Settings (พิกเซลกระเด็น)")]
    [Tooltip("จำนวนเม็ดพิกเซลที่กระเด็นต่อครั้ง")]
    public int particleCount = 16;
    [Tooltip("ขนาดของเม็ดพิกเซล (กว้าง x สูง) แนะนำ 8x8 ถึง 12x12")]
    public Vector2 pixelSize = new Vector2(8f, 8f);
    [Tooltip("แรงดีดกระจาย")]
    public float burstForceMin = 80f;
    public float burstForceMax = 180f;
    [Tooltip("แรงโน้มถ่วงดึงพิกเซลร่วง")]
    public float gravity = 350f;
    [Tooltip("อายุของพิกเซล (วินาที)")]
    public float particleLifetime = 0.5f;

    [Header("Target & References")]
    public RectTransform barTransform;
    public Image barFillImage;

    private const int POOL_SIZE = 40;
    private PixelParticle[] particlePool = new PixelParticle[POOL_SIZE];
    private GameObject container;
    private Coroutine shakeCoroutine;
    private Coroutine particleCoroutine;
    private Vector2 originalBarAnchoredPos;
    private bool isInitialized = false;

    private void Awake()
    {
        InitBar();
        InitParticlePool();
    }

    private void InitBar()
    {
        if (barTransform == null) barTransform = GetComponent<RectTransform>();

        if (barTransform != null)
        {
            originalBarAnchoredPos = barTransform.anchoredPosition;
            isInitialized = true;
        }

        if (barFillImage == null) barFillImage = GetComponentInChildren<Image>();
    }

    private void InitParticlePool()
    {
        container = new GameObject("PixelParticlePool");
        container.transform.SetParent(transform, false);

        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject pObj = new GameObject($"Pixel_{i}");
            pObj.transform.SetParent(container.transform, false);

            RectTransform rect = pObj.AddComponent<RectTransform>();
            rect.sizeDelta = pixelSize;

            Image img = pObj.AddComponent<Image>();
            img.raycastTarget = false;

            pObj.SetActive(false);

            particlePool[i] = new PixelParticle
            {
                rect = rect,
                image = img,
                velocity = Vector2.zero,
                lifeTime = 0f,
                maxLifeTime = particleLifetime,
                active = false
            };
        }
    }

    public void PlayEffect(Color? burstColor = null)
    {
        Color effectColor = burstColor ?? (barFillImage != null ? barFillImage.color : Color.green);

        PlayShake();
        SpawnPixels(effectColor);
    }

    public void PlayShake()
    {
        if (!isInitialized || barTransform == null) return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            barTransform.anchoredPosition = originalBarAnchoredPos;
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float damper = 1f - Mathf.Clamp01(elapsed / shakeDuration);

            float offsetX = Random.Range(-1f, 1f) * shakeIntensity * damper;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity * damper;

            barTransform.anchoredPosition = originalBarAnchoredPos + new Vector2(offsetX, offsetY);
            yield return null;
        }

        barTransform.anchoredPosition = originalBarAnchoredPos;
        shakeCoroutine = null;
    }

    private void SpawnPixels(Color particleColor)
    {
        if (container == null) return;

        // ดึงคอนเทนเนอร์มาอยู่หน้าสุดเสมอเพื่อไม่ให้โดน Background ทับ
        container.transform.SetAsLastSibling();

        // คำนวณจุดปล่อยพิกเซลในระดับ Local Position (ไม่อิง Pos Y ของหน้าจอ)
        Vector2 spawnOrigin = Vector2.zero;
        if (barFillImage != null)
        {
            RectTransform fillRect = barFillImage.rectTransform;
            spawnOrigin = fillRect.anchoredPosition;

            if (barFillImage.type == Image.Type.Filled)
            {
                float fillWidth = fillRect.rect.width;
                float currentX = (barFillImage.fillAmount * fillWidth) - (fillWidth * 0.5f);
                spawnOrigin.x += currentX;
            }
        }

        int spawned = 0;
        for (int i = 0; i < POOL_SIZE && spawned < particleCount; i++)
        {
            if (!particlePool[i].active)
            {
                particlePool[i].active = true;
                particlePool[i].lifeTime = 0f;
                particlePool[i].maxLifeTime = particleLifetime * Random.Range(0.85f, 1.25f);
                particlePool[i].image.color = particleColor;
                particlePool[i].rect.sizeDelta = pixelSize;

                // สุ่มกระจายตำแหน่งรอบหัวหลอด
                particlePool[i].rect.anchoredPosition = spawnOrigin + new Vector2(Random.Range(-4f, 4f), Random.Range(-6f, 6f));

                // เวกเตอร์แรงดีดกระจายแบบระเบิดพิกเซล
                float angle = Random.Range(15f, 165f) * Mathf.Deg2Rad;
                float force = Random.Range(burstForceMin, burstForceMax);
                particlePool[i].velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * force;

                particlePool[i].rect.gameObject.SetActive(true);
                spawned++;
            }
        }

        if (particleCoroutine == null)
        {
            particleCoroutine = StartCoroutine(UpdateParticlesRoutine());
        }
    }

    private IEnumerator UpdateParticlesRoutine()
    {
        bool hasActive = true;

        while (hasActive)
        {
            hasActive = false;
            float dt = Time.unscaledDeltaTime;

            for (int i = 0; i < POOL_SIZE; i++)
            {
                if (particlePool[i].active)
                {
                    particlePool[i].lifeTime += dt;

                    if (particlePool[i].lifeTime >= particlePool[i].maxLifeTime)
                    {
                        particlePool[i].active = false;
                        particlePool[i].rect.gameObject.SetActive(false);
                        continue;
                    }

                    hasActive = true;

                    // ฟิสิกส์พิกเซลร่วงลงมาตามแรงโน้มถ่วง
                    particlePool[i].velocity.y -= gravity * dt;
                    particlePool[i].rect.anchoredPosition += particlePool[i].velocity * dt;

                    // ค่อยๆ จางลง
                    float alpha = 1f - (particlePool[i].lifeTime / particlePool[i].maxLifeTime);
                    Color c = particlePool[i].image.color;
                    c.a = alpha;
                    particlePool[i].image.color = c;
                }
            }

            yield return null;
        }

        particleCoroutine = null;
    }

    private void OnDisable()
    {
        if (barTransform != null && isInitialized)
        {
            barTransform.anchoredPosition = originalBarAnchoredPos;
        }

        for (int i = 0; i < POOL_SIZE; i++)
        {
            particlePool[i].active = false;
            if (particlePool[i].rect != null) particlePool[i].rect.gameObject.SetActive(false);
        }

        shakeCoroutine = null;
        particleCoroutine = null;
    }
}