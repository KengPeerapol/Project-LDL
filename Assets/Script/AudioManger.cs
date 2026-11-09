using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManger : MonoBehaviour
{
    private static AudioManger instance;

    [Header("-------- Audio Source ----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-------- Audio Clip ----------")]
    public AudioClip background;
    public AudioClip menuBackground;
    public AudioClip Shoot;
    public AudioClip Pick;
    public AudioClip Time;

    [Header("--- Player Hurt Sounds ---")]
    public AudioClip HurtLDL;
    public AudioClip HurtSpike;
    public AudioClip HurtMissile;
    public AudioClip HurtPlaque;

    [Header("--- Score & UI Sounds ---")]
    public AudioClip CollectPoint;
    public AudioClip ScoreReached;
    public AudioClip GameOverSound;
    public AudioClip UIHover;
    public AudioClip UIClick;

    [Header("--- Other SFX ---")]
    public AudioClip Die;
    public AudioClip EnemyDie;
    public AudioClip Alert;
    public AudioClip MidSide;

    [Tooltip("⭐ ลากไฟล์เสียงตอนศัตรูหลุดรอดออกนอกจอมาใส่ช่องนี้")]
    public AudioClip EnemyEscape;

    [Header("-------- Volume Settings ----------")]
    [Range(0f, 1f)] public float gameOverVolume = 0.5f;
    [Range(0f, 1f)] public float uiHoverVolume = 0.3f;
    [Range(0f, 1f)] public float uiClickVolume = 1.0f;
    [Range(0f, 1f)] public float alertVolume = 0.8f;
    // ⭐ ข้อ 2: เพิ่มช่องสำหรับปรับความดังเสียงศัตรูหนีโดยเฉพาะ
    [Range(0f, 1f)] public float enemyEscapeVolume = 0.4f;

    [Header("-------- Anti-Spam Settings ----------")]
    public float missileSfxCooldown = 0.15f;

    [Header("-------- Game Over Settings ----------")]
    public float gameOverSoundDelay = 0.5f;

    [Header("-------- Random Pitch Settings ----------")]
    public float minPitch = 0.75f;
    public float maxPitch = 1.25f;

    private AudioSource alertSource;
    private int activeAlertCount = 0;
    private readonly Dictionary<AudioClip, float> soundTimers = new Dictionary<AudioClip, float>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        alertSource = gameObject.AddComponent<AudioSource>();
        alertSource.playOnAwake = false;
        alertSource.loop = true;
        alertSource.ignoreListenerPause = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) ForceStopAlert();
    }

    private void Update()
    {
        if (alertSource != null)
        {
            if (UnityEngine.Time.timeScale == 0f)
            {
                if (alertSource.isPlaying)
                {
                    alertSource.Pause();
                }
            }
            else
            {
                if (activeAlertCount > 0 && !alertSource.isPlaying)
                {
                    alertSource.UnPause();
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance != this) return;

        if (SFXSource != null) SFXSource.Stop();
        ForceStopAlert();

        if (SFXSource != null) SFXSource.ignoreListenerPause = true;
        if (musicSource == null) return;

        AudioClip clipToPlay = (scene.name == "MainMenu" || scene.name == "LevelSelect") ? menuBackground : background;

        if (clipToPlay != null && (musicSource.clip != clipToPlay || !musicSource.isPlaying))
        {
            musicSource.clip = clipToPlay;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayAlert()
    {
        activeAlertCount++;
        if (Alert == null || alertSource == null) return;

        alertSource.clip = Alert;
        alertSource.volume = alertVolume;
        if (!alertSource.isPlaying)
        {
            alertSource.Play();
        }
    }

    public void StopAlert()
    {
        activeAlertCount = Mathf.Max(0, activeAlertCount - 1);
        if (activeAlertCount == 0 && alertSource != null && alertSource.isPlaying)
        {
            alertSource.Stop();
        }
    }

    public void ForceStopAlert()
    {
        activeAlertCount = 0;
        if (alertSource != null && alertSource.isPlaying)
        {
            alertSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        if (clip == Alert || clip == MidSide)
        {
            if (soundTimers.ContainsKey(clip))
            {
                if (UnityEngine.Time.unscaledTime - soundTimers[clip] < missileSfxCooldown)
                {
                    return;
                }
            }
            soundTimers[clip] = UnityEngine.Time.unscaledTime;
        }

        if (clip == Shoot || clip == UIClick || clip == EnemyDie)
        {
            SFXSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else if (clip == Alert || clip == MidSide)
        {
            SFXSource.pitch = Random.Range(0.95f, 1.05f);
        }
        else
        {
            SFXSource.pitch = 1f;
        }

        if (clip == GameOverSound)
        {
            StartCoroutine(PlayGameOverSoundRoutine());
        }
        // ⭐ แยกระดับความดังของเสียงศัตรูหนี ไม่ให้ไปทับกับเสียงอื่นๆ
        else if (clip == EnemyEscape)
        {
            SFXSource.PlayOneShot(clip, enemyEscapeVolume);
        }
        else if (clip == UIHover)
        {
            SFXSource.PlayOneShot(clip, uiHoverVolume);
        }
        else if (clip == UIClick)
        {
            SFXSource.PlayOneShot(clip, uiClickVolume);
        }
        else
        {
            SFXSource.PlayOneShot(clip);
        }
    }

    private IEnumerator PlayGameOverSoundRoutine()
    {
        if (SFXSource != null) SFXSource.Stop();
        ForceStopAlert();
        StopMusic();

        yield return new WaitForSecondsRealtime(gameOverSoundDelay);

        if (SFXSource != null && GameOverSound != null)
        {
            SFXSource.pitch = 1f;
            SFXSource.PlayOneShot(GameOverSound, gameOverVolume);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}