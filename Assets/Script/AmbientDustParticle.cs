using UnityEngine;

public class BackgroundDustManager : MonoBehaviour
{
    [Header("Particle Settings (การตั้งค่าละออง)")]
    [Tooltip("จำนวนเม็ดละอองทั้งหมดในหน้าจอ")]
    public int particleCount = 45;

    [Tooltip("ความเร็วในการลอยไปทางซ้าย (ขั้นต่ำ / สูงสุด)")]
    public float minSpeed = 1.2f;
    public float maxSpeed = 3.5f;

    [Tooltip("ขนาดของเม็ดละออง")]
    public float minSize = 0.04f;
    public float maxSize = 0.14f;

    [Header("Screen Bounds (ขอบเขตหน้าจอ)")]
    [Tooltip("พิกัด X ฝั่งซ้ายสุดที่ละอองจะรีเซ็ตวนกลับไปขวา")]
    public float leftBound = -12f;
    [Tooltip("พิกัด X ฝั่งขวาสุดที่ละอองจะเริ่มเกิด")]
    public float rightBound = 12f;
    [Tooltip("ขอบเขต Y บน-ล่าง ของพื้นที่เกิดละออง")]
    public float minY = -5.5f;
    public float maxY = 5.5f;

    [Header("Appearance & Layer (สีและความลึก)")]
    [Tooltip("สีละออง (แนะนำปรับค่า Alpha เพื่อความโปร่งแสงไม่แย่งซีนตัวละคร)")]
    public Color dustColor = new Color(1f, 1f, 1f, 0.35f);

    [Tooltip("Order in Layer ตั้งค่าน้อยๆ เช่น -10 เพื่อให้อยู่ด้านหลัง Player และ Enemy เสมอ")]
    public int sortingOrder = -10;

    private static Sprite cachedDustSprite;
    private static Material cachedSpriteMaterial;

    private void Start()
    {
        SpawnAllParticles();
    }

    private void SpawnAllParticles()
    {
        Sprite dustSprite = GetOrCreateDustSprite();
        Material spriteMat = GetOrCreateSpriteMaterial(); // ⭐ ดึง Material ที่ถูกต้องสำหรับ 2D

        for (int i = 0; i < particleCount; i++)
        {
            GameObject dustObj = new GameObject("BgDust_Particle");
            dustObj.transform.SetParent(transform);

            float startX = Random.Range(leftBound, rightBound);
            float startY = Random.Range(minY, maxY);
            dustObj.transform.position = new Vector3(startX, startY, 0f);

            float size = Random.Range(minSize, maxSize);
            dustObj.transform.localScale = Vector3.one * size;

            SpriteRenderer sr = dustObj.AddComponent<SpriteRenderer>();
            sr.sprite = dustSprite;
            if (spriteMat != null) sr.material = spriteMat; // ⭐ กำหนด Material ป้องกันสีชมพูม่วง

            float alphaVar = Random.Range(0.6f, 1.2f);
            Color c = dustColor;
            c.a = Mathf.Clamp01(dustColor.a * alphaVar);
            sr.color = c;
            sr.sortingOrder = sortingOrder;

            float speed = Random.Range(minSpeed, maxSpeed);

            BackgroundDustDrift drift = dustObj.AddComponent<BackgroundDustDrift>();
            drift.Setup(speed, leftBound, rightBound, minY, maxY);
        }
    }

    private static Sprite GetOrCreateDustSprite()
    {
        if (cachedDustSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            cachedDustSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return cachedDustSprite;
    }

    // ⭐ ค้นหา Material URP 2D อัตโนมัติ
    private static Material GetOrCreateSpriteMaterial()
    {
        if (cachedSpriteMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                cachedSpriteMaterial = new Material(shader);
            }
        }
        return cachedSpriteMaterial;
    }
}

public class BackgroundDustDrift : MonoBehaviour
{
    private float speed;
    private float leftBound;
    private float rightBound;
    private float minY;
    private float maxY;
    private float waveFrequency;
    private float waveAmplitude;
    private float timeOffset;

    public void Setup(float moveSpeed, float left, float right, float bottom, float top)
    {
        speed = moveSpeed;
        leftBound = left;
        rightBound = right;
        minY = bottom;
        maxY = top;

        waveFrequency = Random.Range(1f, 2.5f);
        waveAmplitude = Random.Range(0.08f, 0.25f);
        timeOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        Vector3 pos = transform.position;

        pos.x -= speed * Time.deltaTime;
        pos.y += Mathf.Sin((Time.time + timeOffset) * waveFrequency) * waveAmplitude * Time.deltaTime;

        if (pos.x <= leftBound)
        {
            pos.x = rightBound;
            pos.y = Random.Range(minY, maxY);
        }

        transform.position = pos;
    }
}