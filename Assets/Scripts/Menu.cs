using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static GameSettings;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class Menu : MonoBehaviour
{

    [Header("UI")]
    public TextMeshProUGUI difficultyText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void jugar()
    {
        SceneManager.LoadSceneAsync(1);
    }

    /// <summary>
    /// Cicla la dificultad Easy → Medium → Hard → Easy …
    /// </summary>  
    public void dificultad()
    {
        switch (GameSettings.currentDifficulty)
        {
            case Difficulty.Easy:
                GameSettings.currentDifficulty = Difficulty.Medium;
                break;
            case Difficulty.Medium:
                GameSettings.currentDifficulty = Difficulty.Hard;
                break;
            default:
                GameSettings.currentDifficulty = Difficulty.Easy;
                break;
        }
        UpdateDifficultyText();
    }

    void UpdateDifficultyText()
    {
        difficultyText.text = $"Dificultad: {GameSettings.currentDifficulty}";
    }


    public void controlStyle()
    {
        switch (GameSettings.currentControlStyle)
        {
            case ControlStyle.Keyboard:
                GameSettings.currentControlStyle = ControlStyle.Haptic;
                break;
            default:
                GameSettings.currentControlStyle = ControlStyle.Keyboard;
                break;
        }
    }


    public void salir()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        Debug.Log("El juego se ha cerrado desde el editor.");
#else
        Application.Quit(0);
        Debug.Log("El juego se ha cerrado.");
#endif
    }
}
