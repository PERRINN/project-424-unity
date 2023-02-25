using UnityEngine;

namespace Perrinn424.AutopilotSystem
{
    [ExecuteInEditMode]
    public class AligmentDebug : MonoBehaviour
    {

        public Transform a;
        public Transform b;
        public Transform c;

        public Vector3 ab;
        public Vector3 ac;
        public Vector3 crossProduct;
        public float dotProduct_ac;
        public float dotProduct_ab;
        public float threshold = 0.01f;
        public float ratio;

        public bool belong;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            ab = b.position - a.position;
            ac = c.position - a.position;

            crossProduct = Vector3.Cross(ab.normalized, ac.normalized);
            dotProduct_ac = Vector3.Dot(ab, ac);
            dotProduct_ab = Vector3.Dot(ab, ab);

            belong = dotProduct_ac >= 0f && dotProduct_ac <= dotProduct_ab;
            ratio = dotProduct_ac / dotProduct_ab;

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(a.position, b.position);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(a.position, c.position);
        }
    } 
}
