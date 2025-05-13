using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }
    public string portName = "COM3";
    public int baud = 115200;

    private SerialPort port;
    private Thread readerThread;
    private bool running;

    // Último estado recibido
    public float joyX, joyY;
    public bool btnEnter, btnTab, btnSpace, btnH;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[SerialIO] Puertos disponibles: {string.Join(",", SerialPort.GetPortNames())}");
        try
        {
            port = new SerialPort(portName, baud) { NewLine = "\n", ReadTimeout = 50 };
            port.Open();
            Debug.Log($"[SerialIO] Abierto {portName}@{baud}");
            running = true;
            readerThread = new Thread(ReadLoop) { IsBackground = true };
            readerThread.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SerialIO] No pude abrir {portName}: {e.Message}");
        }
    }

    void ReadLoop()
    {
        while (running)
        {
            try
            {
                string line = port.ReadLine();
                Debug.Log($"[SerialIO] RAW ← {line}");  // ← trazas crudas
                ParseLine(line);
            }
            catch (System.Exception ex)
            {
                // Timeout o fallo de lectura, ignora
            }
        }
    }

    void ParseLine(string line)
    {
        // Filtrar sólo mensajes de tipo input
        if (!line.Contains("\"type\":\"input\"")) return;

        joyX = float.Parse(GetValue(line, "\"x\":", ","));
        joyY = float.Parse(GetValue(line, "\"y\":", ","));
        btnEnter = GetValue(line, "\"enter\":", ",") == "1";
        btnTab = GetValue(line, "\"tab\":", ",") == "1";
        btnSpace = GetValue(line, "\"space\":", ",") == "1";
        // Para la última propiedad (h) usamos '}' como delimitador
        btnH = GetValue(line, "\"h\":", "}") == "1";
    }

    // Extrae el substring que está entre 'start' y 'end'
    string GetValue(string s, string start, string end)
    {
        int i = s.IndexOf(start);
        if (i < 0) return "";
        i += start.Length;
        int j = s.IndexOf(end, i);
        if (j < 0) j = s.Length;
        return s.Substring(i, j - i);
    }

    void OnDestroy()
    {
        running = false;
        readerThread?.Join();
        if (port?.IsOpen == true) port.Close();
    }

    /// <summary>
    /// Envía comandos hápticos a través del mismo puerto.
    /// </summary>
    public void SendHaptic(string json)
    {
        if (port != null && port.IsOpen)
            port.WriteLine(json);
    }
}
