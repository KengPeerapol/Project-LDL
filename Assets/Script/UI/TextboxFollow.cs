using UnityEngine;

public class TextboxFollow : MonoBehaviour
{
    public RectTransform panel;
    public RectTransform textbox;

    public float offset = 20f;
    public float screenPadding = 10f;

    private bool isBelow = false;

    void LateUpdate()
    {
        if (panel == null || textbox == null)
            return;

        Vector3[] corners = new Vector3[4];
        panel.GetWorldCorners(corners);

        // มุมบนของ Panel
        Vector2 panelTop = RectTransformUtility.WorldToScreenPoint(
            null,
            corners[1]
        );

        float screenTop = Screen.height - screenPadding;

        // ชนขอบบน → ย้าย Textbox ลงด้านล่าง
        if (panelTop.y >= screenTop)
        {
            if (!isBelow)
            {
                textbox.anchoredPosition = new Vector2(
                    textbox.anchoredPosition.x,
                    -panel.rect.height / 2f
                    - textbox.rect.height / 2f
                    - offset
                );

                isBelow = true;
            }
        }
        // ไม่ชนขอบบน → ย้าย Textbox กลับด้านบน
        else
        {
            if (isBelow)
            {
                textbox.anchoredPosition = new Vector2(
                    textbox.anchoredPosition.x,
                    panel.rect.height / 2f
                    + textbox.rect.height / 2f
                    + offset
                );

                isBelow = false;
            }
        }
    }
}