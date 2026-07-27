using System.Collections.Generic;

namespace EmpiresBattle.Grid
{
    /// <summary>
    /// Stateless BFS pathfinder. All hex edges have equal cost, so a BFS
    /// "came-from" search yields the shortest path. Occupied cells block
    /// passage, mirroring the rule used by <see cref="HexReachability"/> so
    /// the two stay consistent with each other.
    /// </summary>
    public static class HexPathfinder
    {
        public static List<HexCell> FindPath(HexGrid grid, HexCell start, HexCell target)
        {
            if (start.Coord == target.Coord)
            {
                return new List<HexCell>();
            }

            var cameFrom = new Dictionary<HexCoord, HexCoord> { [start.Coord] = start.Coord };
            var frontier = new Queue<HexCoord>();
            frontier.Enqueue(start.Coord);

            while (frontier.Count > 0)
            {
                HexCoord current = frontier.Dequeue();

                if (current == target.Coord)
                {
                    break;
                }

                foreach (HexCoord neighborCoord in current.GetNeighbors())
                {
                    if (cameFrom.ContainsKey(neighborCoord))
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

                    cameFrom[neighborCoord] = current;
                    frontier.Enqueue(neighborCoord);
                }
            }

            if (!cameFrom.ContainsKey(target.Coord))
            {
                return null;
            }

            var path = new List<HexCell>();
            HexCoord step = target.Coord;

            while (step != start.Coord)
            {
                grid.TryGetCell(step, out HexCell cell);
                path.Add(cell);
                step = cameFrom[step];
            }

            path.Reverse();
            return path;
        }
    }
}
