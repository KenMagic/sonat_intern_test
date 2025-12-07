using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start,
    Playing,
    Win,
    Lose
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState { get; private set; }

    public int currentLevel = 0;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        switch(currentState)
        {
            case GameState.Start:
                SceneManager.LoadScene("StartScene");
                break;
            case GameState.Playing:
                SceneManager.LoadScene("GameScene");
                break;
            case GameState.Win:
                currentLevel = 0;
                SceneManager.LoadScene("WinScene");
                AudioManager.Instance.PlayWin();
                break;
            case GameState.Lose:
                currentLevel = 0;
                SceneManager.LoadScene("LoseScene");
                AudioManager.Instance.PlayLose();
                break;
        }
    }

    // Helper function để restart level hiện tại
    public void RestartLevel()
    {
        SceneManager.LoadScene("GameScene");
        SetState(GameState.Playing);
    }
    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        SetState(GameState.Playing);
    }
}
