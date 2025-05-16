using UnityEngine;

[CreateAssetMenu(fileName = "NewFlagData", menuName = "Game/FlagData")]
public class FlagData : ScriptableObject
{
    public string countryName;  // El nombre EXACTO que coincide con tu Transform.name
    public Sprite flagSprite;   // Arrastra aqu� el Sprite (tu PNG importado)
}
