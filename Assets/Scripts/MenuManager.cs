using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameSettings;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMain;
    public GameObject panelDifficulty;
    public GameObject panelControlStyle;
    public GameObject panelCredits;
    public GameObject panelInstructions;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 0.5f;

    [Header("Instrucciones")]
    [Tooltip("Tiempo en segundos que se muestra la pantalla de instrucciones antes de arrancar el juego")]
    public float instructionDuration = 3f;

    void Start()
    {
        ShowMain();
        // asegúrate que el fade empiece transparente
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;
        panelInstructions.SetActive(false);
    }

    // -----------------------
    // Panel Switching
    // -----------------------
    public void ShowMain()
    {
        panelMain.SetActive(true);
        panelDifficulty.SetActive(false);
        panelControlStyle.SetActive(false);
        panelCredits.SetActive(false);

    }

    public void ShowDifficulty()
    {
        panelMain.SetActive(false);
        panelDifficulty.SetActive(true);
    }

    public void ShowControlStyle()
    {
        panelMain.SetActive(false);
        panelControlStyle.SetActive(true);
    }

    public void ShowCredits()
    {
        panelMain.SetActive(false);
        panelCredits.SetActive(true);
    }

    // -----------------------
    // Botones de Main Menu
    // -----------------------
    public void OnPlayPressed()
    {
        panelMain.SetActive(false);
        panelInstructions.SetActive(true);
        StartCoroutine(InstructionsThenLoad());
    }

    IEnumerator InstructionsThenLoad()
    {
        // Muestra un temporizador real (ignora Time.timeScale)
        yield return new WaitForSecondsRealtime(instructionDuration);
        panelInstructions.SetActive(false);
        // Inicia la transición con fade y carga de escena 1
        StartCoroutine(FadeAndLoad(1));
    }


    public void OnOptionsPressed() => ShowDifficulty();

    public void OnControlStylePressed() => ShowControlStyle();

    public void OnCreditsPressed() => ShowCredits();

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    // -----------------------
    // Botones de Difficulty Submenu
    // -----------------------
    public void OnDifficultyEasy()
    {
        GameSettings.currentDifficulty = Difficulty.Easy;
        UpdateDifficultyUI();
    }
    public void OnDifficultyMedium()
    {
        GameSettings.currentDifficulty = Difficulty.Medium;
        UpdateDifficultyUI();
    }
    public void OnDifficultyHard()
    {
        GameSettings.currentDifficulty = Difficulty.Hard;
        UpdateDifficultyUI();
    }
    public void OnDifficultyBack() => ShowMain();

    void UpdateDifficultyUI()
    {
        // aquí podrías actualizar colores de botón, texto, etc.
        Debug.Log("Dificultad ahora: " + GameSettings.currentDifficulty);
    }

    // -----------------------
    // Botones de ControlStyle Submenu
    // -----------------------
    public void OnControlKeyboard()
    {
        GameSettings.currentControlStyle = ControlStyle.Keyboard;
        UpdateControlStyleUI();
    }
    public void OnControlHaptic()
    {
        GameSettings.currentControlStyle = ControlStyle.Haptic;
        UpdateControlStyleUI();
    }
    public void OnControlBack() => ShowMain();

    void UpdateControlStyleUI()
    {
        Debug.Log("Control Style: " + GameSettings.currentControlStyle);
    }

    // -----------------------
    // Botones de Credits Submenu
    // -----------------------
    public void OnCreditsBack() => ShowMain();
    // -----------------------
    // Fade + Scene Load
    // -----------------------
    IEnumerator FadeAndLoad(int sceneIndex)
    {
        fadeCanvas.blocksRaycasts = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        var op = SceneManager.LoadSceneAsync(sceneIndex);
        while (!op.isDone) yield return null;

        t = 0f;
        while (t < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;
    }
}
