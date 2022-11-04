using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// Add this script to a GameObject that has a UCNetwork component
    /// The referenced GameObject toEnable will be set to enabled once the UCNetwork reports itself as initialized
    /// </summary>
    public class EnableOnCaveInitialized : MonoBehaviour
    {
        public GameObject toEnable;

        private void OnEnable()
        {
            if(toEnable == null || gameObject.GetComponent<UCNetwork>() == null) Destroy(this);
        }

        void Update()
        {
            if(gameObject.GetComponent<UCNetwork>().Initialized)
            {
                toEnable.SetActive(true);
                Debug.Log("SetActive " + toEnable);
                Destroy(this);
            }
        }
    }
}