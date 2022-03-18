using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MAIN_MENU,
    PLAYING,
    END_SCREEN,
}

public class GameManager : Singleton<GameManager>
{
    public GameState state { get; private set; }
    public float time { get; private set; }
    public float fastestTime { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        switch (SceneManager.GetActiveScene().name) {
            case "MainMenu":
                state = GameState.MAIN_MENU;
                break;
            case "Game":
                state = GameState.PLAYING;
                break;
            case "EndScreen":
                state = GameState.END_SCREEN;
                break;
        }

        fastestTime = PlayerPrefs.GetFloat("fastestTime");
    }

    private void RecordTime()
    {
        if (fastestTime == 0 || time < fastestTime) {
            PlayerPrefs.SetFloat("fastestTime", time);
            fastestTime = time;
        }
    }

    private void Update()
    {
        if (state == GameState.PLAYING) {
            time += Time.deltaTime;
        }
    }

    public void StartPlaying()
    {
        if (state == GameState.MAIN_MENU) {
            state = GameState.PLAYING;
            SceneManager.LoadScene("Game");
        }
    }

    public void End()
    {
        if (state == GameState.PLAYING) {
            state = GameState.END_SCREEN;
            RecordTime();
            SceneManager.LoadScene("EndScreen");
        }
    }

    public void Restart()
    {
        if (state == GameState.END_SCREEN || state == GameState.PLAYING) {
            state = GameState.PLAYING;
            SceneManager.LoadScene("Game");
            time = 0;
        }
    }

    public void Quit()
    {
        #if (UNITY_EDITOR)
            UnityEditor.EditorApplication.isPlaying = false;
        #elif (UNITY_STANDALONE) 
            Application.Quit();
        #elif (UNITY_WEBGL)
            Application.OpenURL("https://twitter.com/bigpi_dev");
        #endif
    }
}
