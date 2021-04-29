using EdyCommonTools;
using UnityEngine;
using UnityEngine.Audio;

namespace Perrinn424
{
    public class UnderFloorAudio : MonoBehaviour
    {
        [Header("Audio Effect")]
        [Tooltip("Loopable audio clip with the underfloor drag effect")]
        public AudioClip contactLoopClip;
        [Tooltip("Unity audio mixer to send the audio effects to (optional)")]
        public AudioMixerGroup output;

        [Space(5)]
        [Tooltip("The slightest contact is played at this volume")]
        public float minVolume = 0.2f;
        [Tooltip("Volume at the limit contact depth")]
        public float maxVolume = 1.0f;

        [Space(5)]
        [Tooltip("Below this speed (m/s) volume and pitch are faded out to zero. Above this speed pitch is interpolated to maxPitch at maxSpeed")]
        public float minSpeed = 50 / 3.6f;  // 50 km/h
        [Tooltip("Pitch is interpolated between minSpeed and maxSpeed (m/s) from minPitch to maxPitch")]
        public float maxSpeed = 100.0f;     // 360 km/h
        [Tooltip("Minimum audio pitch at minSpeed")]
        public float minPitch = 0.9f;
        [Tooltip("Maximum audio pitch at maxSpeed and above")]
        public float maxPitch = 1.1f;

        [Header("3D Audio Source")]
        [Tooltip("Maximum volume under this distance")]
        public float minDistance = 2.0f;
        [Tooltip("Volume clipped beyond this distance")]
        public float maxDistance = 400.0f;
        [Tooltip("Volume attenuated progressively until this distance")]
        public float attenuationDistance = 100.0f;
        [Tooltip("This volume constant from Attenuation Distance to Max Distance")]
        public float attenuatedVolume = 0.025f;

        public void ConfigureAudioSource(RuntimeAudio audio)
        {
            AudioSource source = audio.source;
            source.clip = contactLoopClip;
            source.outputAudioMixerGroup = output;
            source.spatialBlend = 1.0f;
            source.loop = true;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
            RuntimeAudio.SetVolumeRolloff(source, minDistance, 1.0f, attenuationDistance, attenuatedVolume, maxDistance);
        }

        public void UpdateContactAudio(UnderFloor.ContactPointData cpData, float speed)
        {
            if (!enabled)
                return;

            RuntimeAudio audio = cpData.contactAudio;
            if (audio == null)
                return;

            AudioSource source = audio.source;
            float absSpeed = MathUtility.FastAbs(speed);

            if (cpData.contactCount > cpData.lastPlayedContact && absSpeed > 0.005f)
            {
                if (!source.isPlaying) source.Play();

                float attenuation = Mathf.Clamp01(absSpeed / minSpeed);
                float intensity = Mathf.Clamp01(cpData.maxContactDepth / cpData.contactPoint.limitContactDepth);
                source.volume = Mathf.Lerp(minVolume, maxVolume, intensity) * attenuation;

                float speedFactor = Mathf.InverseLerp(minSpeed, maxSpeed, absSpeed);
                source.pitch = Mathf.Lerp(minPitch, maxPitch, speedFactor) * (0.8f + 0.2f * attenuation);   // Reduce pitch 80%
                MuteZeroPitch(source);

                cpData.lastPlayedContact = cpData.contactCount;
                cpData.maxContactDepth = 0.0f;
            }
            else
            {
                source.volume = 0.0f;
            }
        }

        void MuteZeroPitch(AudioSource audio)
        {
            // Moving the AudioListener around a source with zero or nearly-zero pitch causes artifacts.
            // The audio source is muted in such chase to prevent that

            float absPitch = MathUtility.FastAbs(audio.pitch);
            if (absPitch < 0.05f)
            {
                audio.volume *= absPitch / 0.05f;
            }
        }
    } 
}
