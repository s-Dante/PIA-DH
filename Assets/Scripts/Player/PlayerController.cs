using UnityEngine;
using Cinemachine;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Velocidad")]
    public float speed = 5.0f;
    public float minSpeed = 5.0f;
    public float maxSpeed = 100.0f;
    [Tooltip("Qué tan rápido cambia la velocidad (unidades/s) al mantener E/Q")]
    public float speedChangeRate = 20f;

    [Header("Altitud")]
    [Tooltip("Altitud mínima que puede alcanzar el avión (mundo Y)")]
    public float minAltitude = 0f;
    [Tooltip("Altitud máxima que puede alcanzar el avión (mundo Y)")]
    public float maxAltitude = 50f;

    [Header("Rotación")]
    public float yawAmount = 120;
    private float yaw;

    [Header("Cámaras Cinemachine")]
    public CinemachineVirtualCamera thirdPerson;
    public CinemachineVirtualCamera topView;
    public CinemachineVirtualCamera frontView;
    private int cameraPos = 0; // 0=third,1=top,2=front

    void Update()
    {
        HandleSpeedInput();
        MoveAndClampAltitude();
        HandleRotation();
        HandleCameraSwitch();
    }

    void HandleSpeedInput()
    {
        // Aumenta velocidad con E, disminuye con Q
        if (Input.GetKey(KeyCode.E))
            speed += speedChangeRate * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q))
            speed -= speedChangeRate * Time.deltaTime;

        // Clamp entre minSpeed y maxSpeed
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }

    void MoveAndClampAltitude()
    {
        // Mover
        transform.position += transform.forward * speed * Time.deltaTime;

        // Limitar altitud (eje Y)
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, minAltitude, maxAltitude);
        transform.position = pos;
    }

    void HandleRotation()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Yaw
        yaw += yawAmount * horizontalInput * Time.deltaTime;

        // Pitch y roll para inclinación
        float pitch = Mathf.Lerp(0, 20, Mathf.Abs(verticalInput)) * Mathf.Sign(verticalInput);
        float roll = Mathf.Lerp(0, 30, Mathf.Abs(horizontalInput)) * -Mathf.Sign(horizontalInput);

        transform.localRotation = Quaternion.Euler(
            Vector3.up * yaw +
            Vector3.right * pitch +
            Vector3.forward * roll
        );
    }

    void HandleCameraSwitch()
    {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;

        cameraPos = (cameraPos + 1) % 3;
        thirdPerson.Priority = cameraPos == 0 ? 1 : 0;
        topView.Priority = cameraPos == 1 ? 1 : 0;
        frontView.Priority = cameraPos == 2 ? 1 : 0;

        Debug.Log("Camera changed to: " + cameraPos);
    }
}
