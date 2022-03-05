using UnityEngine;

public enum GameState
{
    PLAYING,
}

public class GameManager : Singleton<GameManager>
{
    public GameState state { get; private set; } = GameState.PLAYING;
    public float time { get; private set; }

    private void Update()
    {
        if (state == GameState.PLAYING)
        {
            time += Time.deltaTime;
        }
    }
}
