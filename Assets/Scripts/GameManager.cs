using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GameSettings;
using System.Collections;

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
    public float timePerGame = 30f;  // segundos para Hard

    // Estado interno
    private int successCount = 0;
    private int failCount = 0;
    private Difficulty currentDifficulty;

    //Easy Mode
    [Header("Bandera Fácil")]
    public GameObject flagpolePrefab;
    private GameObject currentFlag;

    [Header("Datos de Bandera")]
    public List<FlagData> flags;      // asignas aquí todos tus ScriptableObjects

    [Header("UI")]
    public UnityEngine.UI.Image uiFlagImage;

    //Medium Mode
    public float highlightDuration = 3f;
    private Coroutine highlightCoroutine;
    private Dictionary<Transform, Material[]> originalMaterials;

    //Hard Mode
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
        {
            if (highlightCoroutine != null) StopCoroutine(highlightCoroutine);
            highlightCoroutine = StartCoroutine(HighlightTemporary());
        }

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
            HapticManager.Instance.SendHaptic("{\"vib\":150}");
            HapticManager.Instance.SendHaptic("{\"led_ok\":1}");
            // Muestra “OK” en la LCD
            HapticManager.Instance.PrintLCD("OK");
        }
        else
        {
            failCount++;
            HapticManager.Instance.SendHaptic("{\"led_fail\":1}");
            // Muestra “FALLA” en la LCD
            HapticManager.Instance.PrintLCD("FALLA");
        }

        UpdateCounters();

        // Victoria/derrota
        if (successCount >= missionsCount)
        {
            HapticManager.Instance.PrintLCD("GANASTE!");
            EndGame(true);
            return;
        }

        if (failCount >= maxFailures)
        {
            HapticManager.Instance.PrintLCD("PERDISTE!");
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

        // 1) Actualiza siempre la UI
        var data = flags.Find(f => f.countryName == currentTarget.name);
        if (data != null)
        {
            uiFlagImage.sprite = data.flagSprite;
            // Asignas el sprite…
            uiFlagImage.sprite = data.flagSprite;

            // Ahora ajustas la altura a, digamos, 80px y calculas el ancho manteniendo aspect ratio:
            RectTransform rt = uiFlagImage.rectTransform;
            float desiredHeight = 80f;
            float spriteW = data.flagSprite.rect.width;
            float spriteH = data.flagSprite.rect.height;
            float aspect = spriteW / spriteH;

            // Fijas sizeDelta: x = ancho, y = alto
            rt.sizeDelta = new Vector2(desiredHeight * aspect, desiredHeight);
        }
        else
        {
            Debug.LogWarning($"No hay FlagData para {currentTarget.name}");
        }

        // 2) Easy: instanciar asta con la bandera
        if (currentDifficulty == Difficulty.Easy)
        {
            // elimina asta previa
            if (currentFlag) Destroy(currentFlag);

            // calcula centro del país
            var rendCountry = currentTarget.GetComponent<Renderer>();
            Vector3 centerWorld = rendCountry != null
                ? rendCountry.bounds.center
                : currentTarget.position;

            // instancia el prefab
            float flagHeight = 2.0f;
            currentFlag = Instantiate(
                flagpolePrefab,
                centerWorld + Vector3.up * flagHeight,
                Quaternion.identity
            );
            currentFlag.transform.SetParent(currentTarget, worldPositionStays: true);

            // Busca el Renderer de la bandera dentro del prefab
            var flagTransform = currentFlag.transform.Find("Flag");
            if (flagTransform != null)
            {
                var flagRend = flagTransform.GetComponent<Renderer>();
                // Usa MaterialPropertyBlock para no duplicar materiales
                var block = new MaterialPropertyBlock();
                flagRend.GetPropertyBlock(block);
                block.SetTexture("_BaseMap", data.flagSprite.texture);
                flagRend.SetPropertyBlock(block);
            }
            else
            {
                Debug.LogError("No encontré el child 'Flag' en flagpolePrefab.");
            }
        }
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

    IEnumerator HighlightTemporary()
    {
        // aplica gris a todos menos currentTarget
        ApplyGrayscale(true);
        yield return new WaitForSeconds(highlightDuration);
        // quita el gris
        ApplyGrayscale(false);
    }

    void ApplyGrayscale(bool apply)
    {
        foreach (Transform country in mapParent)
        {
            var rend = country.GetComponent<Renderer>();
            if (rend == null) continue;
            if (country == currentTarget || !apply)
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

    // Resalta el país actual con emisión amarilla
    void HighlightTargetEmission()
    {
        var rend = currentTarget.GetComponent<Renderer>();
        if (rend == null) return;

        // Activar emisión amarilla
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.yellow * 2f);
    }
}
