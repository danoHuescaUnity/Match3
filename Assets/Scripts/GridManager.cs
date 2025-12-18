using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the logical grid: creation, spatial layout, match detection and refills.
/// Does NOT own score, moves or high-level game flow – that is GameManager's job.
/// </summary>

public class GridManager : MonoBehaviour
{
     [Header("Grid Settings")]
    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 5;

    [SerializeField] private float cellWidth = 1f;
    [SerializeField] private float cellHeight = 1f;

    [Header("Blocks")]
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Sprite[] blockSprites;

    private Block[,] blocks;
    private int[,] colors;

    private void Awake()
    {
        blocks = new Block[rows, columns];
        colors = new int[rows, columns];
    }

    /// <summary>
    /// Converts grid coordinates to world position.
    /// Row 0 is the bottom row, row increases upwards.
    /// </summary>
    private Vector3 GetWorldPosition(int row, int column)
    {
        Vector3 basePos = transform.position;
        return new Vector3(
            basePos.x + column * cellWidth,
            basePos.y + row * cellHeight,
            0f);
    }

    private void CreateBlock(int row, int column, int colorId)
    {
        Block newBlock = Instantiate(blockPrefab, transform);
        Sprite sprite = blockSprites[colorId];
        Vector3 worldPos = GetWorldPosition(row, column);

        newBlock.Init(row, column, colorId, sprite);
        newBlock.transform.position = worldPos;

        SetBlockSorting(newBlock, row, column);

        blocks[row, column] = newBlock;
        colors[row, column] = colorId;
    }

    /// <summary>
    /// Centralized place to compute sprite sorting based on row.
    /// Higher rows render above lower rows so the top block always covers the tab below.
    /// </summary>
    private void SetBlockSorting(Block block, int row, int column)
    {
        int order = row + 1;
        block.SetSortingOrder(order);
    }

    // Creates an initial random grid and guarantees at least on valid match.
    public void InitGrid()
    {
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                int colorId = Random.Range(0, blockSprites.Length);
                CreateBlock(r, c, colorId);
            }
        }
        //avoid soft lock boards: we keep re-rollig until there is at least one move.
        StartCoroutine(EnsureHasMatch(3));
    }

    public List<Block> CollectConnected(Block startBlock)
    {
        var result = new List<Block>();
        if (startBlock == null)
            return result;

        int targetColor = startBlock.ColorId;
        bool[,] visited = new bool[rows, columns];

        FloodFill(startBlock.Row, startBlock.Column, targetColor, visited, result);

        if (result.Count < 3)
        {
            return new List<Block>();
        }

        foreach (Block b in result)
        {
            int r = b.Row;
            int c = b.Column;

            blocks[r, c] = null;
            colors[r, c] = -1;

            Destroy(b.gameObject);
        }

        return result;
    }

    /// <summary>
    /// Classic flood fill collecting all neighbors with the same color (4-directional).
    /// This version builds a list of block instances.
    /// </summary>
    private void FloodFill(
        int row,
        int col,
        int targetColor,
        bool[,] visited,
        List<Block> result)
    {
        if (row < 0 || row >= rows || col < 0 || col >= columns)
            return;

        if (visited[row, col])
            return;

        Block b = blocks[row, col];
        if (b == null)
            return;

        if (colors[row, col] != targetColor)
            return;

        visited[row, col] = true;
        result.Add(b);

        FloodFill(row + 1, col, targetColor, visited, result);
        FloodFill(row - 1, col, targetColor, visited, result);
        FloodFill(row, col + 1, targetColor, visited, result);
        FloodFill(row, col - 1, targetColor, visited, result);
    }

    public void RefillGrid()
    {
        for (int c = 0; c < columns; c++)
        {
            int writeRow = 0;

            for (int r = 0; r < rows; r++)
            {
                if (blocks[r, c] != null)
                {
                    if (r != writeRow)
                    {
                        Block b = blocks[r, c];

                        blocks[writeRow, c] = b;
                        blocks[r, c] = null;

                        colors[writeRow, c] = colors[r, c];
                        colors[r, c] = -1;

                        Vector3 newPos = GetWorldPosition(writeRow, c);
                        b.SetGridPosition(writeRow, c, newPos);
                        SetBlockSorting(b, writeRow, c);
                    }

                    writeRow++;
                }
            }

            for (int r = writeRow; r < rows; r++)
            {
                int colorId = Random.Range(0, blockSprites.Length);
                CreateBlock(r, c, colorId);
            }
        }

        StartCoroutine(EnsureHasMatch(3));
    }

    public bool HasAnyMatch(int minSize = 3)
    {
        bool[,] visited = new bool[rows, columns];

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (blocks[r, c] == null || visited[r, c])
                    continue;

                int colorId = colors[r, c];
                int count = FloodCount(r, c, colorId, visited);

                if (count >= minSize)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Lightweight flood fill that only counts the size of a group.
    /// Used when we want to check for available moves without modifying the grid.
    /// </summary>
    private int FloodCount(int row, int col, int targetColor, bool[,] visited)
    {
        if (row < 0 || row >= rows || col < 0 || col >= columns)
            return 0;

        if (visited[row, col])
            return 0;

        if (blocks[row, col] == null)
            return 0;

        if (colors[row, col] != targetColor)
            return 0;

        visited[row, col] = true;

        int total = 1;
        total += FloodCount(row + 1, col,     targetColor, visited);
        total += FloodCount(row - 1, col,     targetColor, visited);
        total += FloodCount(row,     col + 1, targetColor, visited);
        total += FloodCount(row,     col - 1, targetColor, visited);

        return total;
    }

    public IEnumerator EnsureHasMatch(int minSize = 3)
    {
        int attempts = 0;
        const int maxAttempts = 50;

        while (!HasAnyMatch(minSize) && attempts < maxAttempts)
        {
            GameManager.Instance.NoMoreMatchs(show: true);
            RegenerateAllBlocks();
            attempts++;
            yield return new WaitForSeconds(2f);
        }

        GameManager.Instance.NoMoreMatchs(show: false);
    }

    private void RegenerateAllBlocks()
    {
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (blocks[r, c] != null)
                {
                    Destroy(blocks[r, c].gameObject);
                    blocks[r, c] = null;
                    colors[r, c] = -1;
                }
            }
        }

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                int colorId = Random.Range(0, blockSprites.Length);
                CreateBlock(r, c, colorId);
            }
        }
    }
}
