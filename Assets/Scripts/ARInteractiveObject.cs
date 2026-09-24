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
    public Color grabColor = Color.green;   // Цвет при касании/захвате

    private Camera arCam;
    private bool isDragging = false;
    private float dragDistance = 1.5f;

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
            // Подписка на события XR Interaction Toolkit (для преподавателя)
            grabInteractable.hoverEntered.AddListener(OnHoverEnter);
            grabInteractable.hoverExited.AddListener(OnHoverExit);
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    void Start()
    {
        arCam = Camera.main;
        if (arCam == null) arCam = FindObjectOfType<Camera>();
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
        if (arCam == null)
        {
            arCam = Camera.main ?? FindObjectOfType<Camera>();
            if (arCam == null) return;
        }

        // ==========================================
        // 1. КАСАНИЕ И ПЕРЕМЕЩЕНИЕ (1 палец на телефоне)
        // ==========================================
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = arCam.ScreenPointToRay(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
                {
                    isDragging = true;
                    dragDistance = Vector3.Distance(arCam.transform.position, transform.position);
                    SetColor(grabColor); // ВИЗУАЛЬНЫЙ ОТКЛИК: куб загорается зеленым
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                transform.position = ray.GetPoint(dragDistance); // ПЕРЕМЕЩЕНИЕ (DRAG) вслед за пальцем
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
                SetColor(originalColor); // Возврат исходного цвета
            }
        }
        else if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            if (isDragging)
            {
                isDragging = false;
                SetColor(originalColor);
            }
        }

        // ==========================================
        // 2. МАСШТАБИРОВАНИЕ (2 пальца - Pinch)
        // ==========================================
        if (Input.touchCount == 2)
        {
            isDragging = false;
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
                targetScale = Vector3.Max(Vector3.one * 0.05f, Vector3.Min(Vector3.one * 0.8f, targetScale));
                transform.localScale = targetScale;
            }
        }

        // ==========================================
        // 3. ТЕСТ НА ПК (ЛКМ - перетащить, Колёсико - размер)
        // ==========================================
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = arCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isDragging = true;
                dragDistance = Vector3.Distance(arCam.transform.position, transform.position);
                SetColor(grabColor);
            }
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            Ray ray = arCam.ScreenPointToRay(Input.mousePosition);
            transform.position = ray.GetPoint(dragDistance);
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            SetColor(originalColor);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.localScale += Vector3.one * scroll * 0.3f;
            transform.localScale = Vector3.Max(Vector3.one * 0.05f, Vector3.Min(Vector3.one * 0.8f, transform.localScale));
        }
    }
}