//==================================================================================================
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//
// Replacement for Unity UI's StandaloneInputModule and InputSystemUIInputModule.
// Allows UIs based in Unity UI to work regardless the active input method.
//==================================================================================================


using UnityEngine;
#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#else
using UnityEngine.EventSystems;
#endif


namespace VersionCompatibility
{

public class UnityUIInputModule : MonoBehaviour
	{
	void Awake ()
		{
		#if !ENABLE_LEGACY_INPUT_MANAGER
		gameObject.AddComponent<InputSystemUIInputModule>();
		#else
		gameObject.AddComponent<StandaloneInputModule>();
		#endif
		}
	}

}
