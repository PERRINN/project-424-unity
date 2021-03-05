using UnityEngine;
using VehiclePhysics.Timing;

namespace Perrinn424.UI
{
    public class LapTimerListener : MonoBehaviour
    {
        private LapTimer lapTimer;
        [SerializeField]
        private LapTimeTable lapTimeTable = default;

        [SerializeField]
        private TimeCell timeCell;

        private void Awake()
        {
            lapTimer = FindObjectOfType<LapTimer>();
        }

        private void OnEnable()
        {
            if (lapTimer != null)
            {
                lapTimer.onSector += OnSector;
            }
        }

        private void OnSector(int sector, float sectorTime)
        {
            lapTimeTable.AddSector(sectorTime);
        }

        private void OnDisable()
        {
            if (lapTimer != null)
            {
                lapTimer.onSector -= OnSector;
            }
        }

        private void Update()
        {
            if (lapTimer != null)
                timeCell.SetTime(lapTimer.currentLapTime, @"mm\:ss\:fff");
        }
    }
}
