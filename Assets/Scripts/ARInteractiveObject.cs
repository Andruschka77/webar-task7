using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class ARInteractiveObject : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Renderer objRenderer;
    private Color originalColor;

    [Header("Визуальный отклик")]
    public Color hoverColor = Color.cyan;   // Цвет при наведении
    public Color grabColor = Color.green;   // Цвет при захвате/касании

    private float initialPinchDistance;
    private Vector3 initialScale;

    void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            originalColor = objRenderer.material.color;
        }

        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // Подписываемся на события XR Interaction Toolkit
            grabInteractable.hoverEntered.AddListener(OnHoverEnter);
            grabInteractable.hoverExited.AddListener(OnHoverExit);
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    private void OnHoverEnter(HoverEnterEventArgs args) => SetColor(hoverColor);
    private void OnHoverExit(HoverExitEventArgs args) => SetColor(originalColor);
    private void OnSelectEnter(SelectEnterEventArgs args) => SetColor(grabColor);
    private void OnSelectExit(SelectExitEventArgs args) => SetColor(originalColor);

    private void SetColor(Color c)
    {
        if (objRenderer != null) objRenderer.material.color = c;
    }

    void Update()
    {
        // 1. Масштабирование двумя пальцами (Pinch-to-scale на смартфоне)
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                initialScale = transform.localScale;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                if (Mathf.Approximately(initialPinchDistance, 0)) return;

                float factor = currentDistance / initialPinchDistance;
                Vector3 targetScale = initialScale * factor;

                // Ограничиваем размер (от 5 см до 80 см)
                targetScale = Vector3.Max(Vector3.one * 0.05f, Vector3.Min(Vector3.one * 0.8f, targetScale));
                transform.localScale = targetScale;
            }
        }

        // 2. Для проверки на ПК (колёсико мыши)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.localScale += Vector3.one * scroll * 0.3f;
            transform.localScale = Vector3.Max(Vector3.one * 0.05f, Vector3.Min(Vector3.one * 0.8f, transform.localScale));
        }
    }
}