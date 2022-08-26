using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniCAVE
{
    /// <summary>
    /// Copy non-stereo camera output to stereo camera output
    /// Essentially allows two cameras to render to Unity's passive stereo pipeline
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class StereoBlit : MonoBehaviour
    {
        public Camera lcam;
        public Camera rcam;

        private Camera cam;

        /// <summary>
        /// Assign my camera and create target camera render textures
        /// </summary>
        private void Start()
        {
            cam = GetComponent<Camera>();
            cam.depth = 100;

            if(lcam.targetTexture == null)
            {
                lcam.targetTexture = new RenderTexture(lcam.pixelWidth, lcam.pixelHeight, 1);
                lcam.targetTexture.name = Util.ObjectFullName(gameObject) + "_LeftTexture";
            }
            if(rcam.targetTexture == null)
            {
                rcam.targetTexture = new RenderTexture(rcam.pixelWidth, rcam.pixelHeight, 1);
                rcam.targetTexture.name = Util.ObjectFullName(gameObject) + "_RightTexture";
            }
        }

        /// <summary>
        /// Copy non-stereo camera texture to stereo camera left/right texture
        /// </summary>
        /// <param name="source">Unused parameter (ordinarily used for post processing)</param>
        /// <param name="destination">Texture associated with either left or right stereo eye, depending on pipeline phase</param>
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if(cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
            {
                Graphics.Blit(lcam.targetTexture, destination);
            }
            else
            {
                Graphics.Blit(rcam.targetTexture, destination);
            }
        }
    }
}