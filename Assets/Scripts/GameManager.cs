using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Settings")]
    [SerializeField] private int initialMoves = 20;
    [SerializeField] private int scorePerMove = 10;

    private int currentMoves;
    private int currentScore;
    private bool isGameOver;

    private void Start()
    {
        // init state
        currentMoves = initialMoves;
        currentScore = 0;
        isGameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (movesText != null)
            movesText.text = currentMoves.ToString();

        if (scoreText != null)
            scoreText.text = currentScore.ToString("N0"); // show 1,250 instead of 1250
    }

    public void OnMakeMoveButton()
    {
        if (isGameOver)
            return;

        // basic logic for test: 1 move consumed, 10 points added
        currentMoves--;
        currentScore += scorePerMove;

        UpdateUI();

        if (currentMoves <= 0)
        {
            ShowGameOver();
        }
    }

    private void ShowGameOver()
    {
        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void OnReplayButton()
    {
        // Restart main scene
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
