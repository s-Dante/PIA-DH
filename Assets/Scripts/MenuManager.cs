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

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 0.5f;

    void Start()
    {
        ShowMain();
        // aseg�rate que el fade empiece transparente
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;
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
        StartCoroutine(FadeAndLoad(1));  // suponiendo escena �ndice 1 = juego
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
        // aqu� podr�as actualizar colores de bot�n, texto, etc.
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
        // bloquea raycasts durante la transici�n
        fadeCanvas.blocksRaycasts = true;
        // Fade-in
        float t = 0f;
        while (t < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        // Carga as�ncrona
        var op = SceneManager.LoadSceneAsync(sceneIndex);
        while (!op.isDone) yield return null;

        // Fade-out
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
