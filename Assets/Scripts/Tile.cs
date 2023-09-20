using Pathing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IAStarNode
{
    // The offset indexes of the neighbouring tiles differ depending on whether the row is even- or odd.
    private static readonly int[][] EVENROWOFFSETS =
    {
        new int[] { 1, 0 },    // Right
        new int[] { 0, 1 },    // Bottom Right
        new int[] { -1, 1 },   // Bottom Left
        new int[] { -1, 0 },   // Left
        new int[] { -1, -1 },  // Top Left
        new int[] { 0, -1 }    // Top Right
    };
    private static readonly int[][] ODDROWOFFSETS =
    {
        new int[] { 1, 0 },    // Right
        new int[] { 1, 1 },    // Bottom Right
        new int[] { 0, 1 },    // Bottom Left
        new int[] { -1, 0 },   // Left
        new int[] { 0, -1 },   // Top Left
        new int[] { 1, -1 }    // Top Right
    };

    public TileDataSO TileData
    {
        get => tileData; 
        set
        {
            tileData = value;
            UpdateMaterial();

            if (tileData.TerrainType == TerrainType.Water) DefaultOutlineColor = Color.red;
        }
    }
    private TileDataSO tileData;
    public int XIndex { get => xIndex; set => xIndex = value; }
    private int xIndex;
    public int ZIndex { get => zIndex; set => zIndex = value; }
    private int zIndex;

    private Outline _outline;
    public Color DefaultOutlineColor { get => _defaultOutlineColor; private set => _defaultOutlineColor = value; }
    private Color _defaultOutlineColor = Color.white;
    public bool LockedOutline { get; set; } = false;
    public bool Elevated { get; private set; } = false;


    private void Start()
    {
        _outline = GetComponentInChildren<Outline>(includeInactive: true);
        _outline.OutlineColor = DefaultOutlineColor;
    }

    /// <summary>
    /// Returns all traversable neighbours.
    /// </summary>
    public IEnumerable<IAStarNode> Neighbours
    {
        get
        {
            List<Tile> neighbours = new();
            // Determine the offset of the neighbours depending on whether it's an even or odd row.
            bool isEvenRow = ZIndex % 2 == 0;
            int[][] selectedNeighbourOffsets = isEvenRow ? EVENROWOFFSETS : ODDROWOFFSETS;

            // Calculate the coordinates of neighbors based on the tile's position
            for (int i = 0; i < selectedNeighbourOffsets.Length; i++)
            {
                int neighbourX = XIndex + selectedNeighbourOffsets[i][0];
                int neighbourZ = ZIndex + selectedNeighbourOffsets[i][1];

                // Check if the neighboring coordinates are within the bounds of the map and not a water tile
                if (IsValidTile(neighbourX, neighbourZ))
                {
                    // Retrieve the neighboring tile from the _map array
                    Tile neighbourTile = GameManager.Singleton.GetTileAt(neighbourX, neighbourZ) as Tile;

                    if (neighbourTile != null && neighbourTile.TileData.TerrainType != TerrainType.Water)
                    {
                        neighbours.Add(neighbourTile);
                    }
                }
            }

            return neighbours;
        }
    }


    /// <summary>
    /// Calculates the cost through terrain cost.
    /// Distance to neighbours is always the same (1 tile)
    /// </summary>
    /// <returns>The cost to move to this node</returns>
    public float CostTo(IAStarNode neighbor)
    {
        if (neighbor == null || !(neighbor is Tile tileNeighbor))
        {
            return float.PositiveInfinity; // Unreachable
        }

        float terrainCost = tileNeighbor.TileData.TravelCost;

        return terrainCost;
    }

    /// <summary>
    /// Simple heuristic estimation of cost to goal. Does not account for terrain costs.
    /// </summary>
    /// <returns>The cost calculated only by distance</returns>
    public float EstimatedCostTo(IAStarNode goal)
    {
        Tile goalTile = goal as Tile;
        float distanceCost = Vector3.Distance(transform.position, goalTile.transform.position);
        return distanceCost;
    }

    /// <summary>
    /// Are these coordinates within the map?
    /// </summary>
    /// <param name="x">X is width</param>
    /// <param name="z">Z is depth / height</param>
    /// <returns></returns>
    private bool IsValidTile(int x, int z)
    {
        return x >= 0 && x < GameManager.Singleton.MapWidth && z >= 0 && z < GameManager.Singleton.MapLength;
    }

    /// <summary>
    /// Set the tile's mesh material.
    /// </summary>
    private void UpdateMaterial()
    {
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();

        if (TileData != null && renderer != null)
        {
            renderer.material = TileData.TileMaterial;
        }
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return; // Hovering over UI

        _outline.enabled = true;
    }

    private void OnMouseExit()
    {
        if (LockedOutline || EventSystem.current.IsPointerOverGameObject()) return;

        _outline.enabled = false;
    }

    /// <summary>
    /// Set the color of the outline of this tile. Does not work if 'LockedOutline' is true.
    /// </summary>
    /// <param name="color">The new color of the outline</param>
    /// <param name="enable">Should the outline be activated? Default true.</param>
    public void SetOutlineColor(Color color, bool enable = true)
    {
        if (LockedOutline) return;

        _outline.enabled = enable;
        _outline.OutlineColor = color;
    }

    public void Elevate()
    {
        GetComponent<Animator>().SetBool("Elevate", true);
    }
    public void Lower()
    {
        GetComponent<Animator>().SetBool("Elevate", false);
    }
}
