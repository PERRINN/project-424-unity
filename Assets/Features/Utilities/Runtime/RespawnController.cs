using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using VehiclePhysics;

namespace Perrinn424.Utilities

{
    public class RespawnController : MonoBehaviour
    {
        [SerializeField]
        private VehicleBase vehicle;

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            StartCoroutine(DoRespawn(position, rotation));
        }

        private IEnumerator DoRespawn(Vector3 position, Quaternion rotation)
        {
            vehicle.gameObject.SetActive(false);
            yield return new WaitForFixedUpdate();
            vehicle.transform.position = position;
            vehicle.transform.rotation = rotation;
            yield return new WaitForFixedUpdate();
            vehicle.gameObject.SetActive(true);
        }
    } 
}
