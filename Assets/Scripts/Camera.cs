using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform cam;

    public float rotationSpeed = 150f;

    public float zoomSpeed = 10f;
    public float minFOV = 30f;
    public float maxFOV = 180f;

    private Camera cameraComponent;

    void Start()
    {
        cameraComponent = cam.GetComponent<Camera>();
    }

    void Update()
    {
        if (IsUIOpen()) return;

        Rotate();
        Zoom();
    }

    bool IsUIOpen()
    {
        UIManager ui = FindObjectOfType<UIManager>();
        return ui != null && ui.isPanelOpen;
    }

    void Rotate()
    {
        if (Input.GetMouseButton(1)) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
        }
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        cameraComponent.fieldOfView -= scroll * zoomSpeed ;
        cameraComponent.fieldOfView = Mathf.Clamp(cameraComponent.fieldOfView, minFOV, maxFOV);
    }
}