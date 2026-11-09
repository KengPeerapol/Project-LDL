using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyEscapeTrigger : MonoBehaviour
{
    [Header("Detection Tag")]
    public string enemyTag = "Enemy";

    private AudioManger audioManager;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManger>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag) || other.transform.root.CompareTag(enemyTag))
        {
            GameObject enemyObj = other.transform.root.gameObject;

            // ⭐ ดักจับ Missile: ถ้าชื่อของวัตถุมีคำว่า "Missile" (ตัวพิมพ์เล็กหรือใหญ่ก็ได้)
            // ให้ทำลายทิ้งไปเงียบๆ แล้วหยุดการทำงานทันที (return) เสียงจะได้ไม่ร้อง
            if (enemyObj.name.Contains("Missile") || enemyObj.name.Contains("missile"))
            {
                Destroy(enemyObj);
                return;
            }

            if (EnemyEscapeManager.Instance != null)
            {
                EnemyEscapeManager.Instance.OnEnemyEscaped(enemyObj.name);
            }

            if (audioManager != null && audioManager.EnemyEscape != null)
            {
                audioManager.PlaySFX(audioManager.EnemyEscape);
            }

            Destroy(enemyObj);
        }
    }
}