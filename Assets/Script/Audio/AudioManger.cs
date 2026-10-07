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
    public AudioClip Hurt;
    public AudioClip Die;
    public AudioClip Alert;
    public AudioClip MidSide;

    [Header("-------- Anti-Spam Settings ----------")]
    [Tooltip("ระยะเวลาดีเลย์เฉพาะเสียงเตือนและจรวด (วินาที)")]
    public float missileSfxCooldown = 0.8f; // ⭐ เปลี่ยนชื่อให้ชัดเจนว่าเป็นดีเลย์ของจรวด

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

        // ⭐ ตรวจสอบว่าเสียงที่ส่งมา คือเสียง Alert หรือ MidSide ใช่หรือไม่?
        if (clip == Alert || clip == MidSide)
        {
            // ถ้าใช่ ให้เข้าสู่ระบบตรวจสอบ Cooldown (กันเสียงนัวเนีย)
            if (soundTimers.ContainsKey(clip))
            {
                if (UnityEngine.Time.time - soundTimers[clip] < missileSfxCooldown)
                {
                    return; // ถ้าเวลายังไม่พ้นคูลดาวน์ ให้ยกเลิกการเล่นเสียงนี้ไปเลย
                }
            }

            // บันทึกเวลาล่าสุดที่เสียงนี้ดังขึ้น
            soundTimers[clip] = UnityEngine.Time.time;
        }

        // ⭐ ถ้าเป็นเสียงอื่นๆ (เช่น เสียงปืน Shoot) จะข้ามการเช็กด้านบนมาที่บรรทัดนี้ แล้วเล่นทันที!
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