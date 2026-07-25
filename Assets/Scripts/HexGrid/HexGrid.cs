using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EmpiresBattle.Grid
{
    /// <summary>
    /// Generates a pointy-top hex grid from a prefab and width/height parameters.
    /// The grid is authored in the editor (via <c>HexGridEditor</c>'s "Generate Grid"
    /// button) and saved as children of this object; at runtime it just looks up the
    /// cells that are already there. Also acts as the runtime lookup service for hex
    /// cells by coordinate.
    /// </summary>
    public class HexGrid : MonoBehaviour
    {
        [SerializeField] private HexCell hexPrefab;
        [SerializeField] private int width = 7;
        [SerializeField] private int height = 6;

        [Tooltip("Distance between neighboring hex cells. Lower this if the grid looks too spread out.")]
        [SerializeField] private float cellOffset = 0.6f;

        private readonly Dictionary<HexCoord, HexCell> _cells = new();

        public static HexGrid Instance { get; private set; }

        public IReadOnlyDictionary<HexCoord, HexCell> Cells => _cells;
        public float CellOffset => cellOffset;

        private void Awake()
        {
            Instance = this;

            if (transform.childCount > 0)
            {
                RebuildCellLookup();
            }
            else
            {
                GenerateGrid();
            }
        }

        /// <summary>Rebuilds the coordinate lookup from cells already present under this transform (editor-authored grid).</summary>
        public void RebuildCellLookup()
        {
            _cells.Clear();

            foreach (HexCell cell in GetComponentsInChildren<HexCell>(true))
            {
                _cells[cell.Coord] = cell;
            }
        }

        /// <summary>Destroys all existing cells and instantiates a fresh grid. Callable from the editor or at runtime.</summary>
        public void GenerateGrid()
        {
            ClearGrid();

            if (hexPrefab == null || width <= 0 || height <= 0)
            {
                Debug.LogWarning($"{nameof(HexGrid)}: cannot generate grid, missing prefab or non-positive dimensions.", this);
                return;
            }

            var coords = new List<HexCoord>(width * height);
            var worldPositions = new List<Vector3>(width * height);

            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    HexCoord coord = HexCoord.FromOffset(col, row);
                    Vector3 pos = HexCoord.ToWorldPosition(coord, cellOffset);

                    coords.Add(coord);
                    worldPositions.Add(pos);

                    min = Vector3.Min(min, pos);
                    max = Vector3.Max(max, pos);
                }
            }

            Vector3 center = (min + max) / 2f;

            for (int i = 0; i < coords.Count; i++)
            {
                HexCoord coord = coords[i];
                Vector3 localPos = worldPositions[i] - center;

                HexCell cell = InstantiateCell(hexPrefab, transform);
                cell.transform.localPosition = localPos;
                cell.name = $"Hex_{coord}";
                cell.Initialize(coord);

                _cells[coord] = cell;
            }
        }

        /// <summary>Destroys all generated cells (editor- or runtime-safe).</summary>
        public void ClearGrid()
        {
            _cells.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.DestroyObjectImmediate(child);
                    continue;
                }
#endif
                Destroy(child);
            }
        }

        private static HexCell InstantiateCell(HexCell prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var editorInstance = (HexCell)PrefabUtility.InstantiatePrefab(prefab, parent);
                Undo.RegisterCreatedObjectUndo(editorInstance.gameObject, "Generate Hex Grid");
                return editorInstance;
            }
#endif
            return Instantiate(prefab, parent);
        }

        public bool TryGetCell(HexCoord coord, out HexCell cell) => _cells.TryGetValue(coord, out cell);

        public bool IsInBounds(HexCoord coord) => _cells.ContainsKey(coord);

        public IEnumerable<HexCell> GetNeighbors(HexCell cell)
        {
            foreach (HexCoord neighborCoord in cell.Coord.GetNeighbors())
            {
                if (_cells.TryGetValue(neighborCoord, out HexCell neighbor))
                {
                    yield return neighbor;
                }
            }
        }
    }
}
