using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI feedbackText; 
    [SerializeField] private float feedbackDisplayTime = 2f; 

    [Header("Wave UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button startWaveButton;
    
    private Coroutine feedbackCoroutine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        feedbackText.gameObject.SetActive(false); // Hide at start
        
        // Hook up the button's OnClick event to the WaveManager
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(() => WaveManager.Instance.StartNextWaveButton());
        }
    }

    public void UpdateMoneyText(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = $"GOLD: {money}";
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
    }

    public void ShowFeedbackMessage(string message)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(message));
    }

    private IEnumerator FeedbackRoutine(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(feedbackDisplayTime);
        feedbackText.gameObject.SetActive(false);
    }
    public void UpdateWaveText(int currentWave)
    {
        if (waveText != null)
        {
            waveText.text = $"WAVE: {currentWave}";
        }
    }
    public void UpdateCountdownText(float time)
    {
        if (countdownText != null)
        {
            if (time > 0)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = $"Next wave in: {Mathf.CeilToInt(time)}s";
            }
            else
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowStartWaveButton(bool show)
    {
        if(startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(show);
        }
    }

}