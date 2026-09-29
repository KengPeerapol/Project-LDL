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
    public AudioClip Player;
    public AudioClip Die;

    private void Start()
    {
        // เช็กให้ชัวร์ว่ามีการใส่ไฟล์เพลงไว้ในช่อง Background แล้วจริงๆ เกมจะได้ไม่ Error
        if (background != null)
        {
            musicSource.clip = background;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}