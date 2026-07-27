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

        private readonly HashSet<HexCoord> _moveHighlighted = new();
        private readonly HashSet<HexCoord> _attackHighlighted = new();

        private void Awake()
        {
            Instance = this;
        }

        public bool IsReachable(HexCoord coord) => _moveHighlighted.Contains(coord);

        public bool IsAttackable(HexCoord coord) => _attackHighlighted.Contains(coord);

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

        /// <summary>Hides the current highlight set immediately, e.g. as soon as a move/attack has been issued.</summary>
        public void HideHighlights() => ClearHighlights();

        private void ApplyHighlights()
        {
            if (Selected == null || Selected.CurrentCell == null || HexGrid.Instance == null)
            {
                return;
            }

            foreach (HexCoord neighborCoord in Selected.CurrentCell.Coord.GetNeighbors())
            {
                if (HexGrid.Instance.TryGetCell(neighborCoord, out _))
                {
                    _attackHighlighted.Add(neighborCoord);
                }
            }

            HashSet<HexCoord> reachable = HexReachability.ComputeReachable(HexGrid.Instance, Selected.CurrentCell, Selected.MoveRange);
            reachable.ExceptWith(_attackHighlighted);
            _moveHighlighted.UnionWith(reachable);

            foreach (HexCoord coord in _moveHighlighted)
            {
                if (HexGrid.Instance.TryGetCell(coord, out HexCell cell))
                {
                    cell.SetHighlighted(HexHighlightType.Move);
                }
            }

            foreach (HexCoord coord in _attackHighlighted)
            {
                if (HexGrid.Instance.TryGetCell(coord, out HexCell cell))
                {
                    cell.SetHighlighted(HexHighlightType.Attack);
                }
            }
        }

        private void ClearHighlights()
        {
            if (HexGrid.Instance != null)
            {
                foreach (HexCoord coord in _moveHighlighted)
                {
                    if (HexGrid.Instance.TryGetCell(coord, out HexCell cell))
                    {
                        cell.SetHighlighted(HexHighlightType.None);
                    }
                }

                foreach (HexCoord coord in _attackHighlighted)
                {
                    if (HexGrid.Instance.TryGetCell(coord, out HexCell cell))
                    {
                        cell.SetHighlighted(HexHighlightType.None);
                    }
                }
            }

            _moveHighlighted.Clear();
            _attackHighlighted.Clear();
        }
    }
}
