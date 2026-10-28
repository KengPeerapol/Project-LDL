using System.Collections.Generic;
using UnityEngine;

public class AudioManger : MonoBehaviour
{
    [Header("-------- Audio Source ----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-------- Audio Clip ----------")]
    public AudioClip background;
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
    public AudioClip Alert;
    public AudioClip MidSide;

    [Header("-------- Volume Settings ----------")]
    [Tooltip("ปรับความดังเฉพาะเสียง Game Over (0 = เงียบสุด, 1 = ดังสุด)")]
    [Range(0f, 1f)] public float gameOverVolume = 0.5f;

    [Tooltip("ปรับความดังเสียงตอนชี้ปุ่ม (Hover)")]
    [Range(0f, 1f)] public float uiHoverVolume = 0.3f; // ⭐ พี่ตั้งค่าเริ่มต้นให้เบาลง (0.3)

    [Tooltip("ปรับความดังเสียงตอนคลิกปุ่ม (Click)")]
    [Range(0f, 1f)] public float uiClickVolume = 1.0f; // ⭐ เสียงคลิกดังปกติ (1.0)

    [Header("-------- Anti-Spam Settings ----------")]
    [Tooltip("ระยะเวลาดีเลย์ขั้นต่ำระหว่างเสียงมิสไซล์ (วินาที)")]
    public float missileSfxCooldown = 0.15f;

    [Header("-------- Random Pitch Settings ----------")]
    [Tooltip("ค่าความต่ำ-สูงของเสียงยิงปืน และเสียงคลิก UI")]
    public float minPitch = 0.75f;
    public float maxPitch = 1.25f;

    private Dictionary<AudioClip, float> soundTimers = new Dictionary<AudioClip, float>();

    private void Start()
    {
        // บังคับให้เสียง SFX ทะลวงการหยุดเกมออกมาได้
        if (SFXSource != null) SFXSource.ignoreListenerPause = true;

        if (background != null)
        {
            musicSource.clip = background;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // ดักคูลดาวน์มิสไซล์
        if (clip == Alert || clip == MidSide)
        {
            if (soundTimers.ContainsKey(clip))
            {
                if (UnityEngine.Time.time - soundTimers[clip] < missileSfxCooldown)
                {
                    return;
                }
            }
            soundTimers[clip] = UnityEngine.Time.time;
        }

        // นำเสียง UIClick มารวมกลุ่มสุ่ม Pitch (สูง-ต่ำ) ด้วย
        if (clip == Shoot || clip == UIClick)
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

        // ⭐ แยกระดับความดังของแต่ละเสียงก่อนเล่น
        if (clip == GameOverSound)
        {
            SFXSource.PlayOneShot(clip, gameOverVolume);
        }
        else if (clip == UIHover)
        {
            SFXSource.PlayOneShot(clip, uiHoverVolume); // ใช้ระดับเสียง Hover
        }
        else if (clip == UIClick)
        {
            SFXSource.PlayOneShot(clip, uiClickVolume); // ใช้ระดับเสียง Click
        }
        else
        {
            SFXSource.PlayOneShot(clip); // เสียงอื่นๆ ดังปกติ
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