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

        private float timer;

        private void OnEnable()
        {
            counter = new Perrinn424.FPSCounter();
            Refresh();
        }

        private void Update()
        {

            if (Time.unscaledTime > timer)
            {
                Refresh();
                timer = Time.unscaledTime + refreshRate;
            }
        }

        private void Refresh()
        {
            counter.Update();
            text.text = $"{counter.Current:F0} FPS";
        }
    }
}
