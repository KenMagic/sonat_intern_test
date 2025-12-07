using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int tubeCount = 8;
    public int colorCount = 6;
    public int layerPerTube = 4;
    public float spacing = 1.8f;
    public float rowYOffset = 2f;
}
