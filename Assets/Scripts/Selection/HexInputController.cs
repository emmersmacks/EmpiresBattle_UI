using System.Collections.Generic;
using EmpiresBattle.Grid;
using EmpiresBattle.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmpiresBattle.Selection
{
    /// <summary>
    /// Resolves mouse/touch clicks (new Input System) against the hex grid and
    /// translates them into selection/move intents on <see cref="SelectionManager"/>.
    /// </summary>
    public class HexInputController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        [Tooltip("Checked first, so a unit standing on a cell is preferred over the cell beneath it.")]
        [SerializeField] private LayerMask unitMask;

        [SerializeField] private LayerMask hexMask;

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleClick(Mouse.current.position.ReadValue());
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                HandleClick(Touchscreen.current.primaryTouch.position.ReadValue());
            }
        }

        private void HandleClick(Vector2 screenPos)
        {
            if (worldCamera == null || SelectionManager.Instance == null)
            {
                return;
            }

            SelectionManager manager = SelectionManager.Instance;

            if (manager.HasSelection && manager.Selected.IsBusy)
            {
                return;
            }

            Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, -worldCamera.transform.position.z);
            Vector3 worldPoint = worldCamera.ScreenToWorldPoint(screenPoint);

            Collider2D unitHit = Physics2D.OverlapPoint(worldPoint, unitMask);

            if (unitHit != null && unitHit.TryGetComponent(out HexUnit unit))
            {
                SelectionManager.Instance.Select(unit);
                return;
            }

            Collider2D hexHit = Physics2D.OverlapPoint(worldPoint, hexMask);

            if (hexHit != null && hexHit.TryGetComponent(out HexCell cell))
            {
                TryActOnCell(cell);
                return;
            }

            SelectionManager.Instance.Clear();
        }

        private void TryActOnCell(HexCell cell)
        {
            SelectionManager manager = SelectionManager.Instance;

            if (!manager.HasSelection || manager.Selected.IsBusy)
            {
                return;
            }

            if (manager.IsAttackable(cell.Coord))
            {
                manager.HideHighlights();
                manager.Selected.Attack(manager.RefreshHighlights);
                return;
            }

            if (!manager.IsReachable(cell.Coord))
            {
                return;
            }

            List<HexCell> path = HexPathfinder.FindPath(HexGrid.Instance, manager.Selected.CurrentCell, cell);

            if (path == null || path.Count == 0)
            {
                return;
            }

            manager.HideHighlights();
            manager.Selected.MoveAlongPath(path, manager.RefreshHighlights);
        }
    }
}
