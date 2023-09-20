using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileInteraction : MonoBehaviour
{
    public delegate void TileClickEventHandler(Tile clickedTile);
    public static event TileClickEventHandler OnTileClicked;

    public static EventHandler OnRightClickEvent;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.TryGetComponent<Tile>(out var clickedTile))
                {
                    OnTileClicked?.Invoke(clickedTile);
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            OnRightClickEvent.Invoke(this, EventArgs.Empty);
        }
    }
}
