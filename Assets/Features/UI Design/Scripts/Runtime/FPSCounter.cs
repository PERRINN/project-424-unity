using UnityEngine;
using UnityEngine.UI;

namespace Perrinn424.UI
{
    public class FPSCounter : MonoBehaviour
    {
        public Text text = default;
        public float refreshRate = 1.0f;
        public string prepend = "";

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
            text.text = $"{prepend}{counter.Current:F0} FPS";
        }
    }
}
