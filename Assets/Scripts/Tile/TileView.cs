using UnityEngine;

[RequireComponent(typeof(TileData), typeof(SpriteRenderer))]
public class TileView : MonoBehaviour
{
    private TileData tileData;
    private SpriteRenderer spriteRenderer;
    private TileManager tileManager;

    public SpriteRenderer cropLayer_Soil; // 작물이 있을 때 켜지는 흙 오버레이
    public SpriteRenderer cropLayer1;      // 수확 가능 상태의 작물 오브젝트

    private void Awake()
    {
        tileData = GetComponent<TileData>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileManager = FindFirstObjectByType<TileManager>();
    }

    public void Refresh()
    {
        if (tileData == null || spriteRenderer == null)
        {
            return;
        }

        if (tileManager == null)
        {
            tileManager = FindFirstObjectByType<TileManager>();
        }

        if (tileManager == null)
        {
            return;
        }

        spriteRenderer.sprite = tileManager.GetTileSprite(tileData);

        bool hasCrop = tileData.cropType != TileData.CropType.IsEmpty;
        bool blocksCropLayers =
            tileData.tileType == TileData.TileType.Tree ||
            tileData.tileType == TileData.TileType.Rock ||
            tileData.tileType == TileData.TileType.Water;

        if (cropLayer_Soil != null)
        {
            cropLayer_Soil.gameObject.SetActive(hasCrop && !blocksCropLayers);
        }

        if (cropLayer1 != null)
        {
            cropLayer1.sprite = !blocksCropLayers && tileData.cropState == TileData.CropState.IsHarvastable
                ? tileManager.GetCropSpirte(tileData.cropType)
                : null;
        }
    }
}
