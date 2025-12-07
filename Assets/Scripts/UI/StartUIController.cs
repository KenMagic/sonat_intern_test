using UnityEngine;
using UnityEngine.UI;

public class StartUIController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button playButton;

    void Awake()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
    }

    void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);
    }

    void OnPlayClicked()
    {
        GameManager.Instance.SetState(GameState.Playing);
    }

}
