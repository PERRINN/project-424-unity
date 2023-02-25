using System;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class AutopilotNearestNeighbourSearcher : INearestNeighbourSearcher
    {
        private HeuristicNearestNeighbor heuristicSearcher;
        private SectorSearcherNearestNeighbor sectorSearcher;

        public AutopilotNearestNeighbourSearcher(IReadOnlyList<Vector3> path)
        {
            this.heuristicSearcher = new HeuristicNearestNeighbor(path, 50, 100, 4);
            this.sectorSearcher = new SectorSearcherNearestNeighbor(path, 2, float.PositiveInfinity);
        }

        public AutopilotNearestNeighbourSearcher(HeuristicNearestNeighbor heuristicSearcher, SectorSearcherNearestNeighbor sectorSearcher)
        {
            this.heuristicSearcher = heuristicSearcher;
            this.sectorSearcher = sectorSearcher;
        }

        public int Index { get; private set; }

        public float Distance { get; private set; }

        public Vector3 Position { get; private set; }

        public void Search(Vector3 position)
        {
            INearestNeighbourSearcher searcherUsed;
            try
            {
                heuristicSearcher.Search(position);
                searcherUsed = heuristicSearcher;
            }
            catch (InvalidOperationException)
            {
                sectorSearcher.Search(position);
                heuristicSearcher.SetHeuristicIndex(sectorSearcher.Index);
                searcherUsed = sectorSearcher;
            }

            Index = searcherUsed.Index;
            Distance = searcherUsed.Distance;
            Position = searcherUsed.Position;
        }
    } 
}
