using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    PLAYING,
    END_SCREEN,
}

public class GameManager : Singleton<GameManager>
{
    public GameState state { get; private set; }
    public float time { get; private set; }
    [SerializeField]
    private List<GameObject> dontDestroy;

    protected override void Awake()
    {
        base.Awake();

        switch (SceneManager.GetActiveScene().name) {
            case "Game":
                state = GameState.PLAYING;
                break;
            case "EndScreen":
                state = GameState.END_SCREEN;
                break;
        }
    }

    private void Update()
    {
        if (state == GameState.PLAYING) {
            time += Time.deltaTime;
        }
    }

    public void End()
    {
        if (state == GameState.PLAYING) {
            state = GameState.END_SCREEN;
            SceneManager.LoadScene("EndScreen");
        }
    }

    public void Restart()
    {
        if (state == GameState.END_SCREEN) {
            state = GameState.PLAYING;
            SceneManager.LoadScene("Game");
        }
    }
}
