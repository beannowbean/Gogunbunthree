using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 프리뷰 회전 전담 (마우스/터치)
/// - CustomizePreviewController와 함께 사용
/// </summary>
public class PreviewInputHandler : MonoBehaviour
{
    public CustomizePreviewController previewController;
    public float dragRotationSpeed = 0.2f;      // degrees per pixel drag

    [Header("Drag Area (optional)")]
    public RectTransform dragArea;
    public Camera dragAreaEventCamera;

    // runtime
    private bool isDragging = false;
    private Vector3 lastPointerPos = Vector3.zero;
    private int activePointerId = -1;

    private bool IsPointerInDragArea(Vector2 screenPos)
    {
        if (dragArea != null)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(dragArea, screenPos, dragAreaEventCamera);
        }
        if (EventSystem.current != null)
        {
            return !EventSystem.current.IsPointerOverGameObject();
        }
        return true;
    }

    private void Update()
    {
        var active = previewController != null ? previewController.ActivePreview : null;
        if (active == null) return;

        // Mouse
        if (Input.GetMouseButtonDown(0) && IsPointerInDragArea(Input.mousePosition))
        {
            isDragging = true;
            lastPointerPos = Input.mousePosition;
            activePointerId = -1;
        }
        if (Input.GetMouseButtonUp(0) && activePointerId == -1)
        {
            isDragging = false;
        }
        if (isDragging && activePointerId == -1)
        {
            Vector3 cur = Input.mousePosition;
            float deltaX = cur.x - lastPointerPos.x;
            active.transform.Rotate(Vector3.up, -deltaX * dragRotationSpeed, Space.World);
            lastPointerPos = cur;
        }

        // Touch
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began && IsPointerInDragArea(t.position))
            {
                isDragging = true;
                lastPointerPos = t.position;
                activePointerId = t.fingerId;
            }
            else if (t.fingerId == activePointerId)
            {
                if (t.phase == TouchPhase.Moved)
                {
                    Vector2 cur = t.position;
                    float deltaX = cur.x - lastPointerPos.x;
                    active.transform.Rotate(Vector3.up, -deltaX * dragRotationSpeed, Space.World);
                    lastPointerPos = cur;
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                    activePointerId = -1;
                }
            }
        }
    }
}
