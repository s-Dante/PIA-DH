using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GameSettings;
using System.Collections;
using UnityEngine.SceneManagement;

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


    //Buzzer
    readonly (int freq, int dur) BUZZ_CORRECT = (1000, 80);
    readonly (int freq, int dur) BUZZ_WRONG = (400, 200);
    readonly (int freq, int dur) BUZZ_WIN = (1200, 300);
    readonly (int freq, int dur) BUZZ_LOSE = (200, 500);


    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
            HapticManager.Instance.SendHaptic($"{{\"led_ok\":1}}");
            HapticManager.Instance.SendHaptic($"{{\"buz\":{BUZZ_CORRECT.freq},\"dur\":{BUZZ_CORRECT.dur}}}");
            HapticManager.Instance.PrintLCD("OK");
        }
        else
        {
            failCount++;
            HapticManager.Instance.SendHaptic($"{{\"led_fail\":1}}");
            HapticManager.Instance.SendHaptic($"{{\"buz\":{BUZZ_WRONG.freq},\"dur\":{BUZZ_WRONG.dur}}}");
            HapticManager.Instance.PrintLCD("FALLA");
        }

        UpdateCounters();

        // limpiar LCD al cabo de 1.5s
        StartCoroutine(ClearLCDAfter(1.5f));

        // victoria/derrota…
        if (successCount >= missionsCount)
        {
            HapticManager.Instance.SendHaptic($"{{\"buz\":{BUZZ_WIN.freq},\"dur\":{BUZZ_WIN.dur}}}");
            HapticManager.Instance.PrintLCD("GANASTE!");
            StartCoroutine(ClearLCDAfter(2f));
            EndGame(true);
        }
        else if (failCount >= maxFailures)
        {
            HapticManager.Instance.SendHaptic($"{{\"buz\":{BUZZ_LOSE.freq},\"dur\":{BUZZ_LOSE.dur}}}");
            HapticManager.Instance.PrintLCD("PERDISTE!");
            StartCoroutine(ClearLCDAfter(2f));
            EndGame(false);
        }
        else
        {
            // esperar a que se limpie y mostrar la siguiente
            Invoke(nameof(AdvanceMission), 1.6f);
        }
    }

    void AdvanceMission()
    {
        currentMissionIndex++;
        ShowNextMission();
    }

    void ShowNextMission()
    {
        // 1) ¿Nos quedamos sin misiones?
        if (currentMissionIndex >= missionCountries.Count)
        {
            EndGame(successCount >= missionsCount);
            return;
        }

        // 2) Asignar el nuevo target
        currentTarget = missionCountries[currentMissionIndex];

        // 3) Mostrarlo en el LCD y programar su limpieza
        HapticManager.Instance.PrintLCD(currentTarget.name);
        StartCoroutine(ClearLCDAfter(1.5f));

        // 4) Actualizar toda la UI (texto, bandera UI y tamaño)
        UpdateHUD();

        var data = flags.Find(f => f.countryName == currentTarget.name);
        if (data != null)
        {
            uiFlagImage.sprite = data.flagSprite;
            RectTransform rt = uiFlagImage.rectTransform;
            float desiredHeight = 190f;
            float aspect = data.flagSprite.rect.width / data.flagSprite.rect.height;
            rt.sizeDelta = new Vector2(desiredHeight * aspect, desiredHeight);
        }

        // 5) Easy-mode: instanciar asta con bandera
        if (currentDifficulty == Difficulty.Easy)
        {
            if (currentFlag) Destroy(currentFlag);
            var rendCountry = currentTarget.GetComponent<Renderer>();
            Vector3 center = rendCountry != null
                ? rendCountry.bounds.center
                : currentTarget.position;

            currentFlag = Instantiate(
                flagpolePrefab,
                center + Vector3.up * 2f,
                Quaternion.identity
            );
            currentFlag.transform.SetParent(currentTarget, true);

            var flagTransform = currentFlag.transform.Find("Flag");
            if (flagTransform != null)
            {
                var block = new MaterialPropertyBlock();
                var rendFlag = flagTransform.GetComponent<Renderer>();
                rendFlag.GetPropertyBlock(block);
                block.SetTexture("_BaseMap", data.flagSprite.texture);
                rendFlag.SetPropertyBlock(block);
            }
            else Debug.LogError("No encontré 'Flag' dentro de flagpolePrefab");
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

    IEnumerator ClearLCDAfter(float secs)
    {
        yield return new WaitForSeconds(secs);
        HapticManager.Instance.PrintLCD("");
    }



    //Paraganar o perder
    /// <summary>
    /// Llamar desde los botones de Win/Lose para reiniciar el nivel
    /// </summary>
    public void RestartAfterEnd()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
