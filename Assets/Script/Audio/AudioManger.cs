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

    // ⭐ เปลี่ยนจาก Hurt ตัวเดียว เป็นแยก 4 ชนิดตามที่บิวต้องการ
    [Header("--- Player Hurt Sounds ---")]
    public AudioClip HurtLDL;
    public AudioClip HurtSpike;     // หนาม
    public AudioClip HurtMissile;   // จรวด
    public AudioClip HurtPlaque;    // Plaque

    [Header("--- Other SFX ---")]
    public AudioClip Die;
    public AudioClip Alert;
    public AudioClip MidSide;

    [Header("-------- Anti-Spam Settings ----------")]
    [Tooltip("ระยะเวลาดีเลย์เฉพาะเสียงเตือนและจรวด (วินาที)")]
    public float missileSfxCooldown = 0.8f;

    [Header("-------- Random Pitch Settings ----------")]
    [Tooltip("ค่าความต่ำ-สูงของเสียงยิงปืน (ปกติคือ 1)")]
    public float minPitch = 0.75f;
    public float maxPitch = 1.25f;

    // ตัวแปรแบบ Dictionary เพื่อจำเวลาเฉพาะเสียงที่ต้องการหน่วง
    private Dictionary<AudioClip, float> soundTimers = new Dictionary<AudioClip, float>();

    private void Start()
    {
        if (background != null)
        {
            musicSource.clip = background;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

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

        if (clip == Shoot)
        {
            SFXSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            SFXSource.pitch = 1f;
        }

        SFXSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}