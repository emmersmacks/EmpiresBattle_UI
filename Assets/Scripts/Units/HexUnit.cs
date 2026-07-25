using EmpiresBattle.Grid;
using UnityEngine;

namespace EmpiresBattle.Units
{
    /// <summary>
    /// A unit that lives on the hex grid, occupying one <see cref="HexCell"/> at a time.
    /// </summary>
    public class HexUnit : MonoBehaviour
    {
        [SerializeField] private int moveRange = 3;

        [Tooltip("Cell this unit starts on. Assign it in the editor, then use \"Snap To Assigned Cell\" to preview the placement.")]
        [SerializeField] private HexCell startingCell;

        public int MoveRange => moveRange;
        public HexCell CurrentCell { get; private set; }
        public HexCoord CurrentCoord => CurrentCell.Coord;

        private void Start()
        {
            if (CurrentCell == null && startingCell != null)
            {
                PlaceOnCell(startingCell);
            }
        }

        /// <summary>Moves the unit's transform to the assigned starting cell without registering occupancy. Editor preview only.</summary>
        public void SnapToStartingCell()
        {
            if (startingCell != null)
            {
                transform.position = startingCell.transform.position;
            }
        }

        /// <summary>Initial placement onto the grid, with no previous cell to vacate.</summary>
        public void PlaceOnCell(HexCell cell)
        {
            CurrentCell = cell;
            cell.SetOccupant(this);
            transform.position = cell.transform.position;
        }

        /// <summary>Instantly moves the unit to another cell, updating occupancy on both cells.</summary>
        public void TeleportTo(HexCell targetCell)
        {
            if (CurrentCell != null)
            {
                CurrentCell.ClearOccupant();
            }

            transform.position = targetCell.transform.position;
            targetCell.SetOccupant(this);
            CurrentCell = targetCell;
        }
    }
}
