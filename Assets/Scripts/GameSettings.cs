using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }

    public static Difficulty currentDifficulty = Difficulty.Medium;

    public enum ControlStyle { Keyboard, Haptic }

    public static ControlStyle currentControlStyle = ControlStyle.Keyboard;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
