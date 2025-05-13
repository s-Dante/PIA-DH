using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GameSettings;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Misión")]
    public Transform mapParent;
    public int missionsCount = 5;
    public int maxFailures = 3;

    private List<Transform> missionCountries;
    private int currentMissionIndex = 0;
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    [Header("HUD")]
    public TextMeshProUGUI countryNameText;
    public Image countryFlagImage;
    public List<Sprite> flagSprites;
    public TextMeshProUGUI successText;
    public TextMeshProUGUI failText;

    [Header("Extras HUD")]
    public TextMeshProUGUI difficultyLabel;
    public TextMeshProUGUI timerText;

    [Header("Pantallas finales")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Dificultad Hard")]
    public float timePerGame = 60f;  // segundos para Hard

    // Estado interno
    private int successCount = 0;
    private int failCount = 0;
    private Difficulty currentDifficulty;
    private Dictionary<Transform, Material[]> originalMaterials;
    private bool isGrayscaleActive = false;
    private float timeRemaining;
    private bool timerRunning = false;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Ocultar finales
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        // Leer dificultad seleccionada
        currentDifficulty = GameSettings.currentDifficulty;
        difficultyLabel.text = currentDifficulty.ToString().ToUpper();

        // Preparar según dificultad
        SetupDifficulty();

        // Iniciar misiones
        InitializeMissions();
        ShowNextMission();
        UpdateCounters();
    }

    void SetupDifficulty()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                // Nada extra aquí, se resaltará en ShowNextMission
                timerText.gameObject.SetActive(false);
                break;

            case Difficulty.Medium:
                // Guardar materiales originales para gris
                originalMaterials = new Dictionary<Transform, Material[]>();
                foreach (Transform c in mapParent)
                {
                    var rend = c.GetComponent<Renderer>();
                    if (rend != null)
                        originalMaterials[c] = rend.sharedMaterials;
                }
                timerText.gameObject.SetActive(false);
                break;

            case Difficulty.Hard:
                // Arrancar temporizador global
                timeRemaining = timePerGame;
                timerRunning = true;
                timerText.gameObject.SetActive(true);
                break;
        }
    }

    void Update()
    {
        // Medium: al presionar H
        if (currentDifficulty == Difficulty.Medium && Input.GetKeyDown(KeyCode.H))
            ToggleGrayscale();

        // Hard: actualizar timer
        if (currentDifficulty == Difficulty.Hard && timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = $"Tiempo: {Mathf.CeilToInt(timeRemaining)}s";
            if (timeRemaining <= 0f)
            {
                timerRunning = false;
                EndGame(false);
            }
        }
    }

    void InitializeMissions()
    {
        // Recoger y revolver
        var all = new List<Transform>();
        foreach (Transform child in mapParent) all.Add(child);

        for (int i = 0; i < all.Count; i++)
        {
            int r = Random.Range(i, all.Count);
            (all[i], all[r]) = (all[r], all[i]);
        }

        missionsCount = Mathf.Min(missionsCount, all.Count);
        missionCountries = all.Take(missionsCount).ToList();
    }

    public void OnPackageDelivered(bool correcto)
    {
        if (correcto)
        {
            successCount++;
            HapticManager.Instance.SendHaptic("{\"type\":\"haptic\",\"vib\":150}");
            HapticManager.Instance.SendHaptic("{\"type\":\"haptic\",\"led_ok\":1}");
            HapticManager.Instance.SendHaptic("{\"type\":\"haptic\",\"lcd\":\"OK\"}");

        }
        else
        {
            failCount++;
        }

        UpdateCounters();

        // Checar victoria/derrota
        if (successCount >= missionsCount) { 
            EndGame(true);
            return; 
        }

        if (failCount >= maxFailures) { 
            EndGame(false);
            return; 
        }

        // Siguiente misión
        currentMissionIndex++;
        ShowNextMission();
    }

    void ShowNextMission()
    {
        if (currentMissionIndex >= missionCountries.Count)
        {
            EndGame(successCount >= missionsCount);
            return;
        }

        currentTarget = missionCountries[currentMissionIndex];
        UpdateHUD();

        // Easy: resaltar con emisión
        if (currentDifficulty == Difficulty.Easy)
            HighlightTargetEmission();
    }

    void UpdateHUD()
    {
        countryNameText.text = currentTarget.name;
        countryFlagImage.sprite =
            flagSprites.Find(s => s.name == currentTarget.name);
    }

    void UpdateCounters()
    {
        successText.text = $"Entregas: {successCount} / {missionsCount}";
        failText.text = $"Errores: {failCount} / {maxFailures}";
    }

    void EndGame(bool won)
    {
        // Pausar
        Time.timeScale = 0f;

        // Mostrar panel
        winPanel.SetActive(won);
        losePanel.SetActive(!won);
    }

    // ——— Funciones de dificultad ———

    void HighlightTargetEmission()
    {
        var rend = currentTarget.GetComponent<Renderer>();
        if (rend == null) return;

        // Activar emisión amarilla
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.yellow * 2f);
    }

    void ToggleGrayscale()
    {
        isGrayscaleActive = !isGrayscaleActive;

        foreach (Transform country in mapParent)
        {
            var rend = country.GetComponent<Renderer>();
            if (rend == null) continue;

            if (country == currentTarget || !isGrayscaleActive)
            {
                // Restaurar original
                if (originalMaterials.TryGetValue(country, out var mats))
                    rend.materials = mats;
            }
            else
            {
                // Poner gris
                var orig = originalMaterials[country];
                var grayMats = new Material[orig.Length];
                for (int i = 0; i < orig.Length; i++)
                {
                    var m = new Material(orig[i]);
                    var c = m.color;
                    float g = c.grayscale;
                    m.color = new Color(g, g, g, c.a);
                    grayMats[i] = m;
                }
                rend.materials = grayMats;
            }
        }
    }
}
