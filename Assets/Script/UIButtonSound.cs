using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private AudioManger audioManager;

    private void OnEnable()
    {
        FindAudioManager();
    }

    private void FindAudioManager()
    {
        if (audioManager == null)
        {
            GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
            if (audioObj != null) audioManager = audioObj.GetComponent<AudioManger>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FindAudioManager();
        Debug.Log($"<color=cyan>[UI] ตรวจพบการชี้เมาส์ที่ปุ่ม: {gameObject.name}</color>");

        if (audioManager != null && audioManager.UIHover != null)
        {
            audioManager.PlaySFX(audioManager.UIHover);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        FindAudioManager();
        Debug.Log($"<color=green>[UI] ตรวจพบการคลิกที่ปุ่ม: {gameObject.name}</color>");

        if (audioManager != null && audioManager.UIClick != null)
        {
            audioManager.PlaySFX(audioManager.UIClick);
        }
    }
}