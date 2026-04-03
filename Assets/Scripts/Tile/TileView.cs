using UnityEngine;

[RequireComponent(typeof(TileData), typeof(SpriteRenderer))]
public class TileView : MonoBehaviour
{
    private TileData tileData;
    private SpriteRenderer spriteRenderer;
    private TileManager tileManager;

    private void Awake()
    {
        tileData = GetComponent<TileData>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileManager = FindFirstObjectByType<TileManager>();
    }

    public void Refresh()
    {
        if (tileData == null || spriteRenderer == null) return;

        if (tileManager == null)
        {
            tileManager = FindFirstObjectByType<TileManager>();
        }

        spriteRenderer.sprite = tileManager != null ? tileManager.GetTileSprite(tileData.tileType) : null;
    }
}
