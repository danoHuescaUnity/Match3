using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("UI References")]
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject noMoreMatchsPanel;

    [Header("Game Settings")]
    [SerializeField] private int initialMoves = 20;
    [SerializeField] private int scorePerMove = 10;

    private int currentMoves;
    private int currentScore;
    private bool isGameOver;
    private bool isBusy;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        currentMoves = initialMoves;
        currentScore = 0;
        isGameOver = false;
        isBusy = false;

        if(gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateUI();

        if(gridManager != null) gridManager.InitGrid();
    }

    private void UpdateUI()
    {
        if(movesText != null) movesText.text = currentMoves.ToString();

        if(scoreText != null) scoreText.text = currentScore.ToString("N0");
    }

    public void OnMakeMoveButton()
    {
        if(isGameOver) return;

        currentMoves--;
        currentScore += scorePerMove;

        UpdateUI();

        if(currentMoves <= 0) ShowGameOver();
    }

    private void ShowGameOver()
    {
        isGameOver = true;

        if(gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void NoMoreMatchs(bool show)
    {
        if(noMoreMatchsPanel != null) noMoreMatchsPanel.SetActive(show);
        
        isBusy = show;
    }

    public void OnBlockClicked(Block block)
    {
        if(isGameOver || isBusy || block == null) return;
        StartCoroutine( HanldeBlockClick(block));
    }

    private IEnumerator HanldeBlockClick(Block block)
    {
        isBusy = true;

        List<Block> collectedBlocks = gridManager.CollectConnected(block);
        int count = collectedBlocks.Count;

        if(count > 0)
        {
            currentMoves--;
            currentScore += count;
            UpdateUI();

            yield return new WaitForSeconds(1f);

            gridManager.RefillGrid();

            if(currentMoves <= 0) ShowGameOver();
        }

        isBusy = false;
        
    }

    public void OnReplayButton()
    {
        // Restart main scene
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
