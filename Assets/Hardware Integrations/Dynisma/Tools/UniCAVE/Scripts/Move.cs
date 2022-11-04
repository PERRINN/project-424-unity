using UnityEngine;

namespace UniCAVE
{
    public class Move : MonoBehaviour
    {
        void Update()
        {
            transform.localPosition = new Vector3(Mathf.Sin(Time.time), Mathf.Cos(Time.time), -1);
        }
    }
}