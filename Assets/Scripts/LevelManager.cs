using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    
    private const float END_LEVEL_DELAY = 2.0f;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject completedPanel;
    [SerializeField] private GameObject screenDarkener;
    
    private bool isPaused = false;
    private float originalTimeScale;

    private void Start() {
        originalTimeScale = Time.timeScale;
    }
    
    public void PauseGame() {
        isPaused = true;
        screenDarkener.SetActive(true);
        pausePanel.SetActive(true);
        pauseTime();
    }

    public void UnpauseGame() {
        isPaused = false;
        screenDarkener.SetActive(false);
        pausePanel.SetActive(false);
        unpauseTime();
    }

    public void FinishLevel() {
        Invoke("completeLevel", END_LEVEL_DELAY);
    }
    
    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu() {
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }

    public bool IsPaused() {
        return isPaused;
    }

    private void completeLevel() {
        isPaused = true;
        pausePanel.SetActive(false);
        completedPanel.SetActive(true);
        pauseTime();
    }

    private void pauseTime() {
        Time.timeScale = 0.0f;
    }

    private void unpauseTime() {
        Time.timeScale = originalTimeScale;
    }
}
