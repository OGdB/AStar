using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Data", menuName = "Tile Data")]
public class TileDataSO : ScriptableObject
{
    [Header("Tile Properties"), SerializeField]
    private TerrainType terrainType;

    [SerializeField]
    private Material tileMaterial;

    [SerializeField]
    private float travelCost;

    public float TravelCost { get => travelCost; private set => travelCost = value; }
    public Material TileMaterial { get => tileMaterial; private set => tileMaterial = value; }
    public TerrainType TerrainType { get => terrainType; private set => terrainType = value; }
}
