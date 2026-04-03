using UnityEngine;

public class TileData : MonoBehaviour
{
    public enum TileType
    {
        Empty,
        Soil,
        Weed,
        Water,
        Carrot
    }

    public int id;//타일 ID
    public Vector2Int coord;
    public TileType tileType;
    public bool isFarmable;

    public void ApplyState(MiddleDB.TileState state)
    {
        id = state.id;
        coord = state.coord;
        tileType = state.tileType;
        isFarmable = state.isFarmable;
    }
}
