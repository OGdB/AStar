using Pathing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(0)]
public class GameManager : MonoBehaviour
{
    #region Properties
    public static GameManager Singleton;

    [SerializeField]
    private GameObject hexagonalTilePrefab;

    [SerializeField, Space(10), Tooltip("The width of the generated map")]
    private int mapWidth = 8;
    [SerializeField, Tooltip("The height (or depth) of the generated map")]
    private int mapLength = 8;

    [SerializeField, Space(10)]
    private TileDataSO[] tileData;

    private IList<IAStarNode> _shortestPath;
    private Tile _startNode;
    private Tile _endNode;

    private Tile[,] _map;

    public int MapLength { get => mapLength; }
    public int MapWidth { get => mapWidth; }
    #endregion

    private void Awake()
    {
        if (Singleton)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
    }

    private void Start() => GenerateMap();

    private void OnEnable()
    {
        TileInteraction.OnTileClicked += HandleTileClick;
        TileInteraction.OnRightClickEvent += OnRightClick;
    }

    private void OnDisable()
    {
        TileInteraction.OnTileClicked -= HandleTileClick;
        TileInteraction.OnRightClickEvent -= OnRightClick;
    }

    /// <summary>
    /// Generates a map of IAStarNodes, visualized by gameobjects.
    /// </summary>
    private void GenerateMap()
    {
        _map = new Tile[MapWidth, MapLength];

        Transform mapParent = new GameObject("Map").transform;

        // Nested for loop to iterate each tile of the map.
        for (int x = 0; x < MapWidth; x++)
        {
            for (int z = 0; z < MapLength; z++)
            {
                // Every odd row (z direction), there is a -0.5f offset on the X.
                float xOffset = z % 2 != 0 ? 0 : -0.5f;

                Vector3 tilePosition = new(x + xOffset, 0, z - 0.25f * z);
                GameObject tileGO = Instantiate(hexagonalTilePrefab, tilePosition, Quaternion.identity, mapParent);
                Tile tile = tileGO.GetComponentInChildren<Tile>(includeInactive: true);

                _map[x, z] = tile;

                // Set tile coordinates
                tile.XIndex = x;
                tile.ZIndex = z;

                // Assign one of random terrain types.
                int random = Random.Range(0, tileData.Length);
                TileDataSO data = tileData[random];
                tileGO.name = data.TerrainType.ToString();
                tile.TileData = data;
            }
        }
    }

    public void GenerateNewMap()
    {
        ResetVisualization();
        ResetValues();

        Destroy(GameObject.Find("Map"));

        GenerateMap();
    }

    private void HandleTileClick(Tile clickedTile)
    {
        if (clickedTile.TileData.TerrainType == TerrainType.Water)
        {
            Debug.LogWarning("Cannot move to water tile");
            return;
        }

        ResetVisualization();

        if (_startNode == null)
        {
            Debug.Log($"Clicked {clickedTile.TileData.TileMaterial} tile.");
            _startNode = clickedTile;
            _startNode.Elevate();

            SetTileOutline(_startNode, Color.cyan);
        }
        else if (_endNode == null)
        {
            _endNode = clickedTile;
            SetTileOutline(_endNode, Color.cyan);
            _endNode.Elevate();

            _shortestPath = AStar.GetPath(_startNode, _endNode);

            if (_shortestPath == null)
            {
                Debug.LogWarning("Path unreachable or invalid");
                ResetVisualization();
                ResetValues();
                return;
            }

            VisualizePath(_shortestPath);

            // Clear start and end nodes for the next pathfindsing request
            _startNode = null;
            _endNode = null;
        }
    }

    /// <summary>
    /// Invoked when right click is pressed ~ will reset any selection.
    /// </summary>
    private void OnRightClick(object sender, System.EventArgs e)
    {
        ResetVisualization();

        ResetValues();
    }

    /// <summary>
    /// Reset any path- or selection visualization.
    /// </summary>
    private void ResetVisualization()
    {
        if (_startNode) ResetTileVisualization(_startNode);
        if (_endNode) ResetTileVisualization(_endNode);

        if (_shortestPath == null) return;

        foreach (Tile tile in _shortestPath.Cast<Tile>())
        {
            ResetTileVisualization(tile);
        }

        static void ResetTileVisualization(Tile tile)
        {
            tile.LockedOutline = false;
            tile.SetOutlineColor(tile.DefaultOutlineColor, false);
            tile.Lower();
        }
    }

    private void SetTileOutline(Tile tile, Color outlineColor)
    {
        tile.SetOutlineColor(outlineColor);
        tile.LockedOutline = true;
    }
    private void VisualizePath(IList<IAStarNode> path)
    {
        foreach (Tile tile in path.Cast<Tile>())
        {
            tile.SetOutlineColor(Color.yellow);
            tile.Elevate();
            tile.LockedOutline = true;
        }
    }

    public IAStarNode GetTileAt(int x, int z)
    {
        // Check if the coordinates are within the bounds of the map
        if (x >= 0 && x < MapWidth && z >= 0 && z < MapLength)
        {
            return _map[x, z];
        }

        // Return null if out of bounds.
        return null;
    }

    private void ResetValues()
    {
        _startNode = null;
        _endNode = null;
        _shortestPath?.Clear();
    }

}