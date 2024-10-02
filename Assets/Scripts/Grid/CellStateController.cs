using UnityEngine;

public class CellStateController : MonoBehaviour
{
    public GridManager gridManager;

    void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    public void UpdateCellState(int x, int y, CellState newState)
    {
        GridCell cell = gridManager.GetCell(x, y);
        if (cell != null)
        {
            cell.cellState = newState;
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }
    }
}