using System;
using System.Collections.Generic;
using EmpiresBattle.Grid;
using EmpiresBattle.Units;
using UnityEngine;

namespace EmpiresBattle.Selection
{
    /// <summary>
    /// Owns the "currently selected unit" state and the highlight set for its
    /// reachable move-range hexes. This is the API future UI code subscribes to.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        public HexUnit Selected { get; private set; }
        public bool HasSelection => Selected != null;

        /// <summary>Fired whenever the selection changes; passes null on deselect.</summary>
        public event Action<HexUnit> SelectionChanged;

        private readonly HashSet<HexCoord> _highlighted = new();

        private void Awake()
        {
            Instance = this;
        }

        public bool IsReachable(HexCoord coord) => _highlighted.Contains(coord);

        public void Select(HexUnit unit)
        {
            if (unit == Selected)
            {
                return;
            }

            ClearHighlights();
            Selected = unit;
            ApplyHighlights();
            SelectionChanged?.Invoke(Selected);
        }

        public void Clear()
        {
            if (Selected == null)
            {
                return;
            }

            ClearHighlights();
            Selected = null;
            SelectionChanged?.Invoke(null);
        }

        /// <summary>Recomputes the highlight set for the current selection, e.g. after it moves.</summary>
        public void RefreshHighlights()
        {
            ClearHighlights();
            ApplyHighlights();
        }

        private void ApplyHighlights()
        {
            if (Selected == null || Selected.CurrentCell == null || HexGrid.Instance == null)
            {
                return;
            }

            _highlighted.UnionWith(HexReachability.ComputeReachable(HexGrid.Instance, Selected.CurrentCell, Selected.MoveRange));

            foreach (HexCoord coord in _highlighted)
            {
                if (HexGrid.Instance.TryGetCell(coord, out HexCell cell))
                {
                    cell.SetHighlighted(true);
                }
            }
        }

        private void ClearHighlights()
        {
            if (HexGrid.Instance != null)
            {
                foreach (HexCoord coord in _highlighted)
                {
                    if (HexGrid.Instance.TryGetCell(coord, out HexCell cell))
                    {
                        cell.SetHighlighted(false);
                    }
                }
            }

            _highlighted.Clear();
        }
    }
}
