using System.Collections.Generic;

namespace EmpiresBattle.Grid
{
    /// <summary>
    /// Stateless BFS flood-fill for computing which hexes a unit can reach
    /// within a given move range. Occupied cells block passage (Heroes 3 style):
    /// a unit cannot move through or land on a cell that is already occupied.
    /// </summary>
    public static class HexReachability
    {
        public static HashSet<HexCoord> ComputeReachable(HexGrid grid, HexCell origin, int moveRange)
        {
            var visited = new HashSet<HexCoord> { origin.Coord };

            if (moveRange <= 0)
            {
                visited.Remove(origin.Coord);
                return visited;
            }

            var frontier = new Queue<(HexCoord coord, int dist)>();
            frontier.Enqueue((origin.Coord, 0));

            while (frontier.Count > 0)
            {
                (HexCoord coord, int dist) = frontier.Dequeue();

                if (dist >= moveRange)
                {
                    continue;
                }

                foreach (HexCoord neighborCoord in coord.GetNeighbors())
                {
                    if (visited.Contains(neighborCoord))
                    {
                        continue;
                    }

                    if (!grid.TryGetCell(neighborCoord, out HexCell neighborCell))
                    {
                        continue;
                    }

                    if (neighborCell.IsOccupied)
                    {
                        continue;
                    }

                    visited.Add(neighborCoord);
                    frontier.Enqueue((neighborCoord, dist + 1));
                }
            }

            visited.Remove(origin.Coord);
            return visited;
        }
    }
}
