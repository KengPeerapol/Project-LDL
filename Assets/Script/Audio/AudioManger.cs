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

    [Header("--- Other SFX ---")]
    public AudioClip Die;
    public AudioClip Alert;
    public AudioClip MidSide;

    [Header("-------- Anti-Spam Settings ----------")]
    [Tooltip("ระยะเวลาดีเลย์ขั้นต่ำระหว่างเสียงมิสไซล์ (วินาที) แนะนำ 0.15s เพื่อให้ดังครบทุกจำนวนลูก")]
    public float missileSfxCooldown = 0.15f; // ⭐ ปรับลดจาก 0.8f เหลือ 0.15f เพื่อให้เสียงจรวดลูกถัดไปดังทันที

    [Header("-------- Random Pitch Settings ----------")]
    [Tooltip("ค่าความต่ำ-สูงของเสียงยิงปืน (ปกติคือ 1)")]
    public float minPitch = 0.75f;
    public float maxPitch = 1.25f;

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

        // ดักคูลดาวน์เฉพาะช่วงที่สั้นมากๆ เพื่อป้องกันการเปิดซ้ำซ้อนในเฟรมเดียวกัน แต่ยังดังครบตามจำนวนลูก
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
        else if (clip == Alert || clip == MidSide)
        {
            // สุ่มระดับเสียงเล็กน้อย (0.95 - 1.05) เพื่อให้เสียงมิสไซล์แต่ละลูกมีมิติและไม่กลืนกัน
            SFXSource.pitch = Random.Range(0.95f, 1.05f);
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