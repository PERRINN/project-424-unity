using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.TrackMapSystem
{
    public static class ReplayAssetUtils
    {
        public static void GetPositions(this VPReplayAsset replay, float timeStep, out Vector3[] positions, out Rect rect)
        {
            timeStep = Mathf.Max(timeStep, replay.timeStep);
            int step = (int)(timeStep / replay.timeStep);
            int newLength = replay.recordedData.Count / step;
            positions = new Vector3[newLength];

            float minX, minZ, maxX, maxZ;
            minX = minZ = float.PositiveInfinity;
            maxX = maxZ = float.NegativeInfinity;

            for (int i = 0; i < newLength; i++)
            {
                positions[i] = replay.recordedData[i * step].position;

                Vector3 test = positions[i];

                if (test.x < minX)
                    minX = test.x;

                if (test.z < minZ)
                    minZ = test.z;

                if (test.x > maxX)
                    maxX = test.x;

                if (test.z > maxZ)
                    maxZ = test.z;
            }

            rect = new Rect(minX, minZ, maxX - minX, maxZ - minZ);
        }
    } 
}
