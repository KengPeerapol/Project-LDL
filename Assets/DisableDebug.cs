using UnityEngine;

public class DisableDebug : MonoBehaviour
{
    private void Awake()
    {
        // สั่งปิดการแสดงผล Log ทั้งหมดทันทีที่เริ่มเกม
        Debug.unityLogger.logEnabled = false;
    }
}