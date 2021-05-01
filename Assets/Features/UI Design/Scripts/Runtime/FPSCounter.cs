using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField]
        private Text text = default;

        [SerializeField]
        private float refreshRate = 1.0f;

        private Perrinn424.FPSCounter counter;

        private void OnEnable()
        {
            counter = new Perrinn424.FPSCounter();
        }

        private void Update()
        {
            counter.refreshRate = refreshRate;
            counter.Update();
            Refresh();
        }

        private void Refresh()
        {
            text.text = $"{counter.Current:F0} FPS";
        }
    }
}
