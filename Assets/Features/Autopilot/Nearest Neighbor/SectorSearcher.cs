using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    public class SectorSearcher
    {
        private readonly IReadOnlyList<Vector3> path;
        private BruteForceSearcher bruteForceSearcher;
        private readonly int sectorSize;

        public int SectorSize => sectorSize;

        public SectorSearcher(IReadOnlyList<Vector3> path)
        {
            this.path = path;
            bruteForceSearcher = new BruteForceSearcher(path);
            sectorSize = Mathf.CeilToInt(Mathf.Sqrt(path.Count));
        }

        public (int, float) Search(Vector3 position)
        {
            (int nnSector, int searchedSectorSize) = SearchNearestSector(position, sectorSize, 4, 0);
            //(int nnSector, int searchedSectorSize) = SearchNearestSector(position);

            int securityMargin = 3;
            int from = nnSector - Mathf.CeilToInt(searchedSectorSize / 2f) - securityMargin * searchedSectorSize;
            int count = sectorSize + 2 * securityMargin * searchedSectorSize; //searchSize/2 to the left, searchSize/2 to the right, and +1 because of the middle point
            int step = 1;

            return bruteForceSearcher.Search(position, from, count, step);
        }

        //private (int, int) SearchNearestSector(Vector3 position)
        //{
        //    int searchSize = sectorSize;

        //    (int nearestSector, float distance) = bruteForceSearcher.Search(position, 0, path.Count, searchSize);

        //    if (distance > 4f)
        //    {
        //        searchSize = (searchSize / 2) + 1;
        //        (nearestSector, distance) = bruteForceSearcher.Search(position, 0, path.Count, searchSize);
        //    }

        //    if (distance > 4f)
        //    {
        //        searchSize = (searchSize / 2) + 1;
        //        (nearestSector, distance) = bruteForceSearcher.Search(position, 0, path.Count, searchSize);
        //    }

        //    return (nearestSector, searchSize);
        //}

        private (int, int) SearchNearestSector(Vector3 position)
        {
            int searchSize = sectorSize;
            (int nearestSector, float distance) = bruteForceSearcher.Search(position, 0, path.Count, searchSize);

            if (distance > 4f)
            {
                searchSize = (searchSize / 2) + 1;
                (nearestSector, distance) = bruteForceSearcher.Search(position, 0, path.Count, searchSize);
            }

            if (distance > 4f)
            {
                searchSize = (searchSize / 2) + 1;
                (nearestSector, distance) = bruteForceSearcher.Search(position, 0, path.Count, searchSize);
            }

            return (nearestSector, searchSize);
        }

        private (int, int) SearchNearestSector(Vector3 position, int searchSize, float tolerance, int depth)
        {
            (int nearestSector, float distance) = bruteForceSearcher.Search(position, 0, path.Count, searchSize);

            if (distance > tolerance && depth <= 2)
            {
                return SearchNearestSector(position, (searchSize / 2) + 1, tolerance, depth + 1);
            }

            return (nearestSector, searchSize);


            //if (distance < tolerance)
            //{
            //    return (nearestSector, searchSize);
            //}
            //else if (depth <= 2) // maxDepth
            //{
            //    return SearchNearestSector(position, (searchSize / 2) + 1, tolerance, depth + 1);
            //}
            //else
            //{
            //    return (nearestSector, searchSize);
            //}

        }
    } 
}
