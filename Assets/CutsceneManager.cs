using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CutsceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    [Tooltip("ชื่อ Scene ด่านที่จะโหลดหลังจากคัตซีนจบ หรือกดข้าม")]
    public string nextSceneName = "Level 1";

    [Header("Skip Settings (กดค้าง 3 วิเพื่อข้าม)")]
    [Tooltip("ระยะเวลาที่ต้องกดค้าง (วินาที)")]
    public float holdDuration = 3f;

    [Header("UI Feedback")]
    [Tooltip("หลอดวงกลมหรือ Slider แสดงความคืบหน้าการกดค้าง (Optional)")]
    public Image skipProgressFill;
    [Tooltip("ข้อความกำกับ เช่น 'Hold Space to Skip (3s)'")]
    public TextMeshProUGUI skipText;

    [Header("Smooth Transition (แก้อาการเห็นพื้นหลังแวบหนึ่ง)")]
    [Tooltip("Image สีดำเต็มจอสำหรับทำจอมืดบังตอนเปลี่ยนซีน")]
    public Image fadeBlackOverlay;
    [Tooltip("ความเร็วในการมืดหน้าจอ (วินาที)")]
    public float fadeSpeed = 0.2f;

    private float currentHoldTimer = 0f;
    private bool isTransitioning = false;

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }

        if (skipProgressFill != null)
        {
            skipProgressFill.fillAmount = 0f;
        }

        // ตั้งค่าม่านดำเริ่มต้นให้โปร่งใส
        if (fadeBlackOverlay != null)
        {
            fadeBlackOverlay.gameObject.SetActive(true);
            Color c = fadeBlackOverlay.color;
            c.a = 0f;
            fadeBlackOverlay.color = c;
            fadeBlackOverlay.raycastTarget = false;
        }

        UpdateSkipText();
    }

    private void Update()
    {
        if (isTransitioning) return;

        bool isHolding = CheckSkipInput();

        if (isHolding)
        {
            currentHoldTimer += Time.deltaTime;

            if (skipProgressFill != null)
            {
                skipProgressFill.fillAmount = Mathf.Clamp01(currentHoldTimer / holdDuration);
            }

            if (currentHoldTimer >= holdDuration)
            {
                SkipCutscene();
            }
        }
        else
        {
            if (currentHoldTimer > 0f)
            {
                currentHoldTimer -= Time.deltaTime * 2.5f;
                currentHoldTimer = Mathf.Max(0f, currentHoldTimer);

                if (skipProgressFill != null)
                {
                    skipProgressFill.fillAmount = currentHoldTimer / holdDuration;
                }
            }
        }
    }

    private bool CheckSkipInput()
    {
#if ENABLE_INPUT_SYSTEM
        bool spaceHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool mouseHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
        return spaceHeld || mouseHeld;
#else
        return Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
#endif
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StartSceneTransition();
    }

    public void SkipCutscene()
    {
        if (isTransitioning) return;
        Debug.Log("<color=green>[Cutscene] ข้ามคัตซีนสำเร็จ!</color>");
        StartSceneTransition();
    }

    private void StartSceneTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(TransitionAndLoadRoutine());
    }

    private IEnumerator TransitionAndLoadRoutine()
    {
        // 1. ค่อยๆ ปรับจอดำ (Fade to Black) ปิดบังภาพค้างไว้
        if (fadeBlackOverlay != null)
        {
            fadeBlackOverlay.transform.SetAsLastSibling(); // ดึงขึ้นมาบังหน้าสุด
            float elapsed = 0f;
            Color c = fadeBlackOverlay.color;

            while (elapsed < fadeSpeed)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / fadeSpeed);
                fadeBlackOverlay.color = c;
                yield return null;
            }

            c.a = 1f;
            fadeBlackOverlay.color = c;
        }

        // 2. ซ่อน UI Skip เพื่อความเรียบร้อย
        if (skipText != null) skipText.gameObject.SetActive(false);
        if (skipProgressFill != null) skipProgressFill.gameObject.SetActive(false);

        // 3. โหลดฉากแบบ Async ในขณะที่จอมืดสนิท (จะไม่เห็นภาพกระพริบแน่นอน)
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }

    private void UpdateSkipText()
    {
        if (skipText != null)
        {
            skipText.text = "Hold Space / Click to Skip";
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}