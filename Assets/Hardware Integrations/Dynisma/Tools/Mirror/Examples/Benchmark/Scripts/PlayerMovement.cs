using UnityEngine;
using VersionCompatibility;


namespace Mirror.Examples.Benchmark
{
    public class PlayerMovement : NetworkBehaviour
    {
        public float speed = 5;

        void Update()
        {
            if (!isLocalPlayer) return;

            float h = UnityInput.GetAxis(UnityAxis.Horizontal);
            float v = UnityInput.GetAxis(UnityAxis.Vertical);

            Vector3 dir = new Vector3(h, 0, v);
            transform.position += dir.normalized * (Time.deltaTime * speed);
        }
    }
}
