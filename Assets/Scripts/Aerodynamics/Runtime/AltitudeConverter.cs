using System;
using UnityEngine;

namespace Perrinn424.AerodynamicsSystem
{
    [Serializable]
    public class AltitudeConverter
    {
        [SerializeField]
        private float altitudeReference;
        [SerializeField]
        private float unityReference;

        public AltitudeConverter(float altitudeReference, float unityReference)
        {
            this.altitudeReference = altitudeReference;
            this.unityReference = unityReference;
        }

        public float ToAltitude(float unityYWorldPosition)
        {
            return unityYWorldPosition + (altitudeReference - unityReference);
        }

        public float ToUnity(float altitude)
        {
            return altitude - (altitudeReference - unityReference);
        }
    } 
}
