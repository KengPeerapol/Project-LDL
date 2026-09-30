using UnityEngine;

public class AiDialogueFollow : MonoBehaviour
{
    public Transform target;
    public RectTransform dialogue;

    public float offset = 30f;
    public float screenPadding = 20f;

    private RectTransform canvasRect;

    void Start()
    {
        canvasRect = dialogue.GetComponentInParent<Canvas>()
                             .GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (target == null || dialogue == null)
            return;

        Vector3 screenPos =
            Camera.main.WorldToScreenPoint(target.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 targetPos
        );

        float halfWidth = dialogue.rect.width / 2f;
        float halfHeight = dialogue.rect.height / 2f;

        Rect canvasBounds = canvasRect.rect;

        float canvasTop = canvasBounds.yMax;
        float canvasBottom = canvasBounds.yMin;
        float canvasLeft = canvasBounds.xMin;
        float canvasRight = canvasBounds.xMax;

        Vector2 dialoguePos = targetPos;

        // อยู่ด้านบนตัวละคร
        dialoguePos.y = targetPos.y + halfHeight + offset;

        // ถ้าชนขอบบน → ย้ายลงด้านล่าง
        if (dialoguePos.y + halfHeight >
            canvasTop - screenPadding)
        {
            dialoguePos.y =
                targetPos.y - halfHeight - offset;
        }

        // ป้องกันขอบล่าง
        if (dialoguePos.y - halfHeight <
            canvasBottom + screenPadding)
        {
            dialoguePos.y =
                canvasBottom + screenPadding + halfHeight;
        }

        // ป้องกันขอบซ้าย
        if (dialoguePos.x - halfWidth <
            canvasLeft + screenPadding)
        {
            dialoguePos.x =
                canvasLeft + screenPadding + halfWidth;
        }

        // ป้องกันขอบขวา
        if (dialoguePos.x + halfWidth >
            canvasRight - screenPadding)
        {
            dialoguePos.x =
                canvasRight - screenPadding - halfWidth;
        }

        dialogue.localPosition = dialoguePos;
    }
}