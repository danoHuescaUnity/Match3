using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Visual + data container for a single block in the grid.
/// No game logic here – it just knows its coordinates, color and visuals.
/// </summary>

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public int Row { get; private set; }
    public int Column { get; private set; }
    public int ColorId { get; private set; }

    public void Init(int row, int column, int colorId, Sprite sprite)
    {
        Row = row;
        Column = column;
        ColorId = colorId;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = sprite;
    }

    /// <summary>
    /// Updates the block grid coordinates and moves it to the given world position.
    /// Used when blocks "fall" during refills.
    /// </summary>
    public void SetGridPosition(int row, int column, Vector3 worldPosition)
    {
        Row = row;
        Column = column;
        transform.position = worldPosition;
    }

    /// <summary>
    /// Sorting is driven by the GridManager so rows can define render order.
    /// </summary>
    public void SetSortingOrder(int order)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder = order;
    }

    private void OnMouseDown()
    {

        if (GameManager.Instance != null) GameManager.Instance.OnBlockClicked(this);
        
    }
}
