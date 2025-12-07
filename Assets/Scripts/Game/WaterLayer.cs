using UnityEngine;

public class WaterLayer : MonoBehaviour
{
    [Header("Water Layer Sprites")]
    public SpriteRenderer fill;
    public SpriteRenderer top;
    public SpriteRenderer bottom;
    public SpriteRenderer cover;

    public void SetColor(Color c)
    {
        fill.color = c;
        top.color = c;
        bottom.color = c;
    }
}
