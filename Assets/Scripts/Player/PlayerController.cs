﻿using UnityEngine;
using Cinemachine;
using static GameSettings;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float yawAmount = 120f;
    private float yaw;

    [Header("Input")]
    public bool useHardwareInput = true;
    [Range(0f, 1f)] public float smoothing = 0.1f;
    [Range(0f, 0.5f)] public float deadzone = 0.1f;
    private float smoothX, smoothY;

    [Header("Dropper")]
    public PackageDropper dropper;

    [Header("Cámaras")]
    public CinemachineVirtualCamera thirdPerson, topView, frontView;
    private int cameraPos;

    [Header("Velocidad")]
    public float speed = 5f;
    public float minSpeed = 2f, maxSpeed = 20f;
    public float speedChangeRate = 10f;

    // flanco detection
    private bool lastTabHW, lastSpaceHW;

    void Update()
    {
        bool hw = currentControlStyle == ControlStyle.Haptic
                  && HapticManager.Instance != null;


        // --- 1) Obtener y suavizar ejes ---
        float rawX = hw
            ? HapticManager.Instance.joyX
            : Input.GetAxis("Horizontal");
        float rawY = hw
            ? HapticManager.Instance.joyY
            : Input.GetAxis("Vertical");

        if (Mathf.Abs(rawX) < deadzone) rawX = 0f;
        if (Mathf.Abs(rawY) < deadzone) rawY = 0f;

        smoothX = Mathf.Lerp(smoothX, rawX, smoothing);
        smoothY = Mathf.Lerp(smoothY, rawY, smoothing);


        if (hw)
        {
            // potValue va de –1 a +1, lo usamos como “gas” o “freno”
            speed += HapticManager.Instance.potValue * speedChangeRate * Time.deltaTime;
        }
        else
        {
            if (Input.GetKey(KeyCode.E)) speed += speedChangeRate * Time.deltaTime;
            if (Input.GetKey(KeyCode.Q)) speed -= speedChangeRate * Time.deltaTime;
        }
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);


        // --- 2) Movimiento adelante ---
        transform.position += transform.forward * speed * Time.deltaTime;

        // --- 3) Rotación ---
        yaw += yawAmount * smoothX * Time.deltaTime;
        float pitch = Mathf.Lerp(0, 20, Mathf.Abs(smoothY)) * Mathf.Sign(smoothY);
        float roll = Mathf.Lerp(0, 30, Mathf.Abs(smoothX)) * -Mathf.Sign(smoothX);
        transform.localRotation =
            Quaternion.Euler(Vector3.up * yaw + Vector3.right * pitch + Vector3.forward * roll);

        // --- 4) Cambio de cámara (Tab) ---
        bool tabHW = hw && HapticManager.Instance.btnTab;
        bool tabKB = !hw && Input.GetKeyDown(KeyCode.Tab);
        if ((tabHW && !lastTabHW) || tabKB)
        {
            SwitchCamera();
            if (hw) HapticManager.Instance.PrintLCD("CAMBIO CAM");
        }
        lastTabHW = tabHW;

        // --- 5) Soltar paquete (Space) ---
        bool spaceHW = hw && HapticManager.Instance.btnSpace;
        bool spaceKB = !hw && Input.GetKeyDown(KeyCode.Space);
        if ((spaceHW && !lastSpaceHW) || spaceKB)
        {
            // siempre dropeas
            dropper?.DropPackage();

            // solo háptico envía vibración y LCD
            if (hw)
            {
                HapticManager.Instance.SendHaptic("{\"vib\":200}");
                Invoke(nameof(StopVibe), 0.1f);
                HapticManager.Instance.PrintLCD("PAQUETE!");
            }
        }
        lastSpaceHW = spaceHW;
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
