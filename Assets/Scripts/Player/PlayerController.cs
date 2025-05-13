using UnityEngine;
using Cinemachine;
using System;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float yawAmount = 120;

    private float yaw;

    //Other variables
    public float maxSpeed = 100.0f;
    public float minSpeed = 5.0f;


    //Cinemachine
    public CinemachineVirtualCamera thirdPerson;
    public CinemachineVirtualCamera topView;
    public CinemachineVirtualCamera frontView;
    private int cameraPos = 0; //0 = thirdPerson, 1 = topView, 2 = frontView


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        //input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");


        //yaw, pitch, roll
        yaw += yawAmount * horizontalInput * Time.deltaTime;
        // -> transform.eulerAngles = new Vector3(0, yaw, 0);
        // -> transform.Rotate(Vector3.up, yawAmount * horizontalInput * Time.deltaTime);
        float pitch = Mathf.Lerp(0, 20, Mathf.Abs(verticalInput)) * Mathf.Sign(verticalInput);

        float roll = Mathf.Lerp(0, 30, Mathf.Abs(horizontalInput)) * -Mathf.Sign(horizontalInput);

        //apply rotation
        transform.localRotation = Quaternion.Euler(Vector3.up * yaw + Vector3.right * pitch + Vector3.forward * roll);



        //Cinemachine camera change
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (cameraPos == 0)
            {
                cameraPos = 1;
                thirdPerson.Priority = 0;
                topView.Priority = 1;
                frontView.Priority = 0;
            }
            else if (cameraPos == 1)
            {
                cameraPos = 2;
                thirdPerson.Priority = 0;
                topView.Priority = 0;
                frontView.Priority = 1;
            }
            else if (cameraPos == 2)
            {
                cameraPos = 0;
                thirdPerson.Priority = 1;
                topView.Priority = 0;
                frontView.Priority = 0;
            }
            
            Console.WriteLine("Camera changed to: " + cameraPos);
        }
    }
}
