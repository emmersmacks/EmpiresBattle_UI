using EmpiresBattle.Units;
using UnityEngine;

namespace EmpiresBattle.Grid
{
    /// <summary>
    /// A single hex cell instance placed by <see cref="HexGrid"/>. Tracks its
    /// coordinate, current occupant and highlight state.
    /// </summary>
    [DisallowMultipleComponent]
    public class HexCell : MonoBehaviour
    {
        [Tooltip("Optional dedicated highlight overlay. If left empty, the cell falls back to tinting spriteRenderer.")]
        [SerializeField] private SpriteRenderer highlightOverlay;

        [Tooltip("The hex's own sprite renderer, used as a highlight fallback when no overlay is assigned.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.4f, 1f);

        [Tooltip("Set by HexGrid when the cell is generated. Serialized so it survives entering play mode.")]
        [SerializeField] private HexCoord coord;

        private Color _originalColor;
        private bool _isHighlighted;

        public HexCoord Coord => coord;
        public HexUnit Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;
        public bool IsHighlighted => _isHighlighted;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                _originalColor = spriteRenderer.color;
            }

            if (highlightOverlay != null)
            {
                highlightOverlay.enabled = false;
            }
        }

        public void Initialize(HexCoord newCoord)
        {
            coord = newCoord;
        }

        public void SetOccupant(HexUnit unit)
        {
            Occupant = unit;
        }

        public void ClearOccupant()
        {
            Occupant = null;
        }

        public void SetHighlighted(bool on)
        {
            _isHighlighted = on;

            if (highlightOverlay != null)
            {
                highlightOverlay.enabled = on;
                return;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = on ? highlightColor : _originalColor;
            }
        }
    }
}
