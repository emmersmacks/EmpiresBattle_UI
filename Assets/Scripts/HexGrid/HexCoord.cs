using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmpiresBattle.Grid
{
    /// <summary>
    /// Axial coordinate for a pointy-top hex grid. Rows are offset using the
    /// "odd-r" convention when converting to/from column/row (offset) coordinates.
    /// Mutable (non-readonly) fields so Unity can serialize it on <see cref="HexCell"/>.
    /// </summary>
    [Serializable]
    public struct HexCoord : IEquatable<HexCoord>
    {
        public int Q;
        public int R;

        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        public static readonly HexCoord[] AxialDirections =
        {
            new HexCoord(1, 0),
            new HexCoord(1, -1),
            new HexCoord(0, -1),
            new HexCoord(-1, 0),
            new HexCoord(-1, 1),
            new HexCoord(0, 1),
        };

        public static HexCoord FromOffset(int col, int row)
        {
            int q = col - (row - (row & 1)) / 2;
            return new HexCoord(q, row);
        }

        public Vector2Int ToOffset()
        {
            int col = Q + (R - (R & 1)) / 2;
            return new Vector2Int(col, R);
        }

        public HexCoord GetNeighbor(int direction)
        {
            HexCoord dir = AxialDirections[((direction % 6) + 6) % 6];
            return new HexCoord(Q + dir.Q, R + dir.R);
        }

        public IEnumerable<HexCoord> GetNeighbors()
        {
            for (int i = 0; i < AxialDirections.Length; i++)
            {
                yield return GetNeighbor(i);
            }
        }

        public int DistanceTo(HexCoord other)
        {
            int dq = Q - other.Q;
            int dr = R - other.R;
            return (Mathf.Abs(dq) + Mathf.Abs(dq + dr) + Mathf.Abs(dr)) / 2;
        }

        public static Vector3 ToWorldPosition(HexCoord coord, float cellOffset = 1f)
        {
            float x = cellOffset * (Mathf.Sqrt(3f) * coord.Q + Mathf.Sqrt(3f) / 2f * coord.R);
            float y = cellOffset * (3f / 2f * coord.R);
            return new Vector3(x, y, 0f);
        }

        public static HexCoord FromWorldPosition(Vector3 worldPos, float cellOffset = 1f)
        {
            float q = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * worldPos.y) / cellOffset;
            float r = (2f / 3f * worldPos.y) / cellOffset;
            return Round(q, r);
        }

        private static HexCoord Round(float q, float r)
        {
            float x = q;
            float z = r;
            float y = -x - z;

            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            float xDiff = Mathf.Abs(rx - x);
            float yDiff = Mathf.Abs(ry - y);
            float zDiff = Mathf.Abs(rz - z);

            if (xDiff > yDiff && xDiff > zDiff)
            {
                rx = -ry - rz;
            }
            else if (yDiff > zDiff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new HexCoord(rx, rz);
        }

        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;

        public override bool Equals(object obj) => obj is HexCoord other && Equals(other);

        public override int GetHashCode() => (Q, R).GetHashCode();

        public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);

        public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);

        public override string ToString() => $"({Q}, {R})";
    }
}
