using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    
    private const float END_LEVEL_DELAY = 2.0f;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject completedPanel;
    [SerializeField] private GameObject screenDarkener;
    [SerializeField] private LevelCompleteManager levelCompleteManager;
    [SerializeField] private GameObject fader;
    [SerializeField] private AudioClip evilBgm;

    private bool isPaused = false;
    private float originalTimeScale;

    private void Start() {
        FadeIn();
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
        FadeOut();
        Invoke("completeLevel", END_LEVEL_DELAY);
    }
    
    public void ReloadScene() {
        unpauseTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu() {
        unpauseTime();
        SceneManager.LoadScene(Constants.SCENE_MENU);
    }

    public void LoadNextLevel() {
        unpauseTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void FadeIn() {
        fader.SetActive(true);
        StartCoroutine(FadeFromBlack());
    }

    private IEnumerator FadeFromBlack() {
        var canvasGroup = fader.GetComponent<CanvasGroup>();
        while (canvasGroup.alpha > 0) {
            canvasGroup.alpha -= Time.deltaTime / 2.0f;
            yield return null;
        }
        canvasGroup.interactable = false;
        fader.SetActive(false);
        yield return null;
    }

    public void FadeOut() {
        fader.SetActive(true);
        StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeToBlack() {
        var canvasGroup = fader.GetComponent<CanvasGroup>();
        while (canvasGroup.alpha < 1) {
            canvasGroup.alpha += Time.deltaTime / 2.0f;
            yield return null;
        }
        canvasGroup.interactable = false;
        yield return null;
    }

    public bool IsPaused() {
        return isPaused;
    }

    public void PlayEvilBGM() {
        var audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.clip = evilBgm;
        audioSource.Play();
    }

    private void completeLevel() {
        isPaused = true;
        pausePanel.SetActive(false);
        completedPanel.SetActive(true);
        levelCompleteManager.ShowStars();
        pauseTime();
    }

    private void pauseTime() {
        Time.timeScale = 0.0f;
    }

    private void unpauseTime() {
        Time.timeScale = originalTimeScale;
    }
}
