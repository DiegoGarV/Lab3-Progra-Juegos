using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Win/Lose Canvases")]
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private GameObject loseCanvas;

    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Damage Flash")]
    [SerializeField] private CanvasGroup damageFlash;
    [SerializeField] private float flashDuration = 0.6f;
    [SerializeField] private float flashAlpha = 0.6f;

    [Header("Damage / Lives")]
    [SerializeField] private int hitsToLose = 3;

    private int currentHits = 0;

    Coroutine flashCo;

    private bool endGameActive = false;

    private void Awake()
    {
        Instance = this;

        if (winCanvas != null) winCanvas.SetActive(false);
        if (loseCanvas != null) loseCanvas.SetActive(false);
    }

    public void TriggerWin()
    {
        if (endGameActive) return;
        endGameActive = true;
        StartCoroutine(EndSequence(winCanvas));
    }

    public void TriggerLose()
    {
        if (endGameActive) return;
        endGameActive = true;
        StartCoroutine(EndSequence(loseCanvas));
    }

    private IEnumerator EndSequence(GameObject canvasToShow)
    {
        if (canvasToShow != null)
        {
            canvasToShow.SetActive(true);
            var cg = canvasToShow.GetComponentInChildren<CanvasGroup>(true);
            if (cg != null) cg.alpha = 1f;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;

        if (playerInput != null) playerInput.enabled = false;

        Time.timeScale = 0f;
    }

    public void LoadLevel(string levelName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void RegisterHit()
    {
        if (endGameActive) return;

        currentHits++;

        if (currentHits >= hitsToLose)
        {
            TriggerLose();
            return;
        }

        if (damageFlash == null) return;

        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        damageFlash.gameObject.SetActive(true);
        damageFlash.alpha = flashAlpha;

        yield return new WaitForSecondsRealtime(flashDuration);

        damageFlash.alpha = 0f;
        damageFlash.gameObject.SetActive(false);
    }
}