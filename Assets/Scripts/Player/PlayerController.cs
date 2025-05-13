using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float yawAmount = 120f;
    private float yaw;

    [Header("Input")]
    [Tooltip("¿Usar joystick hardware si está disponible?")]
    public bool useHardwareInput = true;
    [Range(0f, 1f), Tooltip("Suavizado exponencial (más bajo = más lento)")]
    public float smoothing = 0.1f;
    [Range(0f, 0.5f), Tooltip("Deadzone para ignorar ruido pequeño")]
    public float deadzone = 0.1f;
    private float smoothX = 0f, smoothY = 0f;

    [Header("Dropper")]
    public PackageDropper dropper;

    [Header("Cámaras")]
    public CinemachineVirtualCamera thirdPerson, topView, frontView;
    private int cameraPos = 0;

    void Update()
    {
        // 1) Obtener raw input (hardware o teclado)
        float rawX, rawY;
        bool hw = useHardwareInput && HapticManager.Instance != null;
        if (hw)
        {
            rawX = HapticManager.Instance.joyX;
            rawY = HapticManager.Instance.joyY;
        }
        else
        {
            rawX = Input.GetAxis("Horizontal");
            rawY = Input.GetAxis("Vertical");
        }

        // 2) Deadzone
        if (Mathf.Abs(rawX) < deadzone) rawX = 0;
        if (Mathf.Abs(rawY) < deadzone) rawY = 0;

        // 3) Suavizado exponencial
        smoothX = Mathf.Lerp(smoothX, rawX, smoothing);
        smoothY = Mathf.Lerp(smoothY, rawY, smoothing);

        // 4) Desplazamiento hacia adelante
        transform.position += transform.forward * speed * Time.deltaTime;

        // 5) Rotaciones: yaw, pitch, roll
        yaw += yawAmount * smoothX * Time.deltaTime;
        float pitch = Mathf.Lerp(0, 20, Mathf.Abs(smoothY)) * Mathf.Sign(smoothY);
        float roll = Mathf.Lerp(0, 30, Mathf.Abs(smoothX)) * -Mathf.Sign(smoothX);
        transform.localRotation = Quaternion.Euler(
            Vector3.up * yaw +
            Vector3.right * pitch +
            Vector3.forward * roll
        );

        // 6) Botones (hardware o teclado)
        bool tabPressed = hw ? HapticManager.Instance.btnTab : Input.GetKeyDown(KeyCode.Tab);
        bool spacePressed = hw ? HapticManager.Instance.btnSpace : Input.GetKeyDown(KeyCode.Space);

        if (tabPressed) SwitchCamera();
        if (spacePressed)
        {
            dropper?.DropPackage();
            // breve vibración de feedback
            HapticManager.Instance?.SendHaptic("{\"vib\":200}");
            Invoke(nameof(StopVibe), 0.1f);
        }
    }

    void SwitchCamera()
    {
        cameraPos = (cameraPos + 1) % 3;
        thirdPerson.Priority = cameraPos == 0 ? 1 : 0;
        topView.Priority = cameraPos == 1 ? 1 : 0;
        frontView.Priority = cameraPos == 2 ? 1 : 0;
    }

    void StopVibe()
    {
        HapticManager.Instance?.SendHaptic("{\"vib\":0}");
    }
}
