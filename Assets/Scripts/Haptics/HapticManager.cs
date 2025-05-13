using UnityEngine;
using System.IO.Ports;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    [Header("Serial COM")]
    public string portName = "COM5";     // Ajusta según tu sistema
    public int baudRate = 115200;
    private SerialPort port;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        port = new SerialPort(portName, baudRate);
        port.NewLine = "\n";
        port.Open();
    }

    void OnDestroy()
    {
        if (port != null && port.IsOpen) port.Close();
    }

    // Métodos de uso en tu lógica de juego:
    public void Vibrate(int intensity)
        => port.WriteLine($"{{\"vib\":{intensity}}}");

    public void StopVibration()
        => port.WriteLine("{\"vib\":0}");

    public void LedSuccess(bool on)
        => port.WriteLine(on ? "{\"led_ok\":1}" : "{\"led_ok\":0}");

    public void LedFail(bool on)
        => port.WriteLine(on ? "{\"led_fail\":1}" : "{\"led_fail\":0}");

    public void Buzzer(int freqHz, int durationMs)
        => port.WriteLine($"{{\"buz\":{freqHz},\"dur\":{durationMs}}}");

    public void PrintLCD(string text)
        => port.WriteLine($"{{\"lcd\":\"{text}\"}}");
}
