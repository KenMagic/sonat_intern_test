using UnityEngine;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button restartButton;
    public Button menuButton;

    void Awake()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);
        if (menuButton != null)
            menuButton.onClick.RemoveListener(OnMenuClicked);
    }

    void OnRestartClicked()
    {
        GameManager.Instance.SetState(GameState.Playing);
        if (GameManager.Instance.currentState == GameState.Playing)
        {
            GameManager.Instance.RestartLevel();
        }
    }

    void OnMenuClicked()
    {
        GameManager.Instance.SetState(GameState.Start);
    }
}
