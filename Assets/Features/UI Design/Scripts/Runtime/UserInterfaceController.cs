using UnityEngine;

namespace Perrinn424.UI
{
    public class UserInterfaceController : MonoBehaviour
    {
        [SerializeField]
        private Canvas canvas;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                canvas.enabled = !canvas.enabled;
            }
        }
    } 
}
