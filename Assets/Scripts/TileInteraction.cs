using UnityEngine;
using UnityEngine.EventSystems;

public class TileInteraction : MonoBehaviour
{
    public delegate void TileClickEventHandler(Tile clickedTile);
    public static event TileClickEventHandler OnTileClicked;

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
    }
}
