using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Menu : MonoBehaviour
{
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

    public void dificultad()
    {

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
