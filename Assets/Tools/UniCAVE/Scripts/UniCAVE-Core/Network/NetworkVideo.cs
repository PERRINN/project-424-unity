//MIT License
//Copyright 2016-Present
//Ross Tredinnick
//Kevin Ponto
//Benny Wysong-Grass
//University of Wisconsin - Madison Virtual Environments Group
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute,
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using Mirror;

namespace UniCAVE
{
    public class NetworkVideo : NetworkBehaviour
    {
        private VideoPlayer player;

        private float canonicalVideoTime = 0.0f;
        private float canonicalPlaybackSpeed = 1.0f;
        private bool canonicalPlaying = true;

        private float lastUpdateRealTime = 0.0f;

        public float maxPlaybackRatio = 1.5f;
        public bool scrubWhenPlaying = true;
        public float scrubThreshold = 1.0f;
        public float convergenceRate = 1.0f;

        [ClientRpc]
        private void RpcUpdateFields(float _canonicalVideoTime, float _canonicalPlaybackSpeed, bool _canonicalPlaying)
        {
            lastUpdateRealTime = Time.time;
            canonicalVideoTime = _canonicalVideoTime;
            canonicalPlaybackSpeed = _canonicalPlaybackSpeed;
            canonicalPlaying = _canonicalPlaying;
        }

        void Start()
        {
            player = GetComponent<VideoPlayer>();
        }

        void FixedUpdate()
        {

            if(isServer)
            {
                canonicalVideoTime = (float)player.time;
                canonicalPlaybackSpeed = player.playbackSpeed;
                canonicalPlaying = player.isPlaying;
                RpcUpdateFields(canonicalVideoTime, canonicalPlaybackSpeed, canonicalPlaying);
            }
            else
            {
                if(canonicalPlaying)
                {
                    if(!player.isPlaying) player.Play();
                    player.playbackSpeed = canonicalPlaybackSpeed * correctedPlaybackRatio;
                }
                else
                {
                    if(player.isPlaying) player.Pause();
                    player.time = canonicalVideoTime;
                    player.playbackSpeed = canonicalPlaybackSpeed;
                }
                if((scrubWhenPlaying && Mathf.Abs(expectedTimeDif) > scrubThreshold) || (Mathf.Abs(expectedTimeDif) > 0.1f && !canonicalPlaying))
                {
                    player.time = canonicalVideoTime;
                }
            }
        }

        private float expectedCanonicalVideoTime
        {
            get
            {
                float timeSinceLastUpdate = Time.time - lastUpdateRealTime;
                return canonicalVideoTime + canonicalPlaybackSpeed * timeSinceLastUpdate;
            }
        }
        private float expectedTimeDif
        {
            get
            {
                return (float)(expectedCanonicalVideoTime - player.time);
            }
        }
        private float correctedPlaybackRatio
        {
            get
            {
                return Mathf.Clamp(Mathf.Exp(expectedTimeDif * convergenceRate), 0.125f, maxPlaybackRatio);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkVideo))]
    public class NetworkVideoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            NetworkVideo netvid = target as NetworkVideo;

            //EditorGUILayout.FloatField("CanonicalVideoTime", netvid.canonicalVideoTime);
            //EditorGUILayout.FloatField("canonicalPlaybackSpeed", netvid.canonicalPlaybackSpeed);

            netvid.maxPlaybackRatio = EditorGUILayout.Slider(new GUIContent("Max Playback Ratio",
                "The maximum ratio of this clip's playback to the canonical clip's playback, for example Max Playback Ratio = 1.5 means the clip can go 50% faster."), netvid.maxPlaybackRatio, 1.1f, 4.0f);

            netvid.scrubWhenPlaying = EditorGUILayout.Toggle(new GUIContent("Scrub When Playing", "Scrub the video while it is playing if the video is out of sync more than a certain threshold"), netvid.scrubWhenPlaying);
            netvid.convergenceRate = EditorGUILayout.Slider(new GUIContent("Convergence Rate", "Relative value of how fast the videos will converge to the same time"), netvid.convergenceRate, 0.01f, 100.0f);

            if(netvid.scrubWhenPlaying)
            {
                netvid.scrubThreshold = EditorGUILayout.Slider(new GUIContent("Scrub Threshold",
                "If the clip is out of sync by more than this amount, it will scrub to the correct time instead of accelerating up to it."), netvid.scrubThreshold, 0.25f, 5.0f);
            }
        }
    }
#endif
}