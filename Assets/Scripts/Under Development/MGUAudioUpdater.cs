using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using System;


namespace Perrinn424
{
    public class MGUAudioUpdater : VehicleBehaviour
    {
        [Serializable]
        public class MGUSettings
        {
            public AudioSource audioSource;
            public float basePitch = 0.0f;
            public float baseVolume = 1.0f;
            public float trqGain = 0.003f;
            public float rpmGain = 0.001f;

            // Runtime

            [NonSerialized] public float rpm;
            [NonSerialized] public float mechanical;
        }

        public MGUSettings frontMGU = new MGUSettings();
        public MGUSettings rearMGU = new MGUSettings();


        public override void UpdateAfterFixedUpdate ()
        {
            if (vehicle.paused) return;

            // Gather updated data from the vehicle

            int[] custom = vehicle.data.Get(Channel.Custom);
            frontMGU.rpm = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.Rpm] / 1000.0f;
            frontMGU.mechanical = custom[Perrinn424Data.FrontMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;
            rearMGU.rpm = custom[Perrinn424Data.RearMguBase + Perrinn424Data.Rpm] / 1000.0f;
            rearMGU.mechanical = custom[Perrinn424Data.RearMguBase + Perrinn424Data.MechanicalTorque] / 1000.0f;

            // Update the MGU audios

            UpdateMguAudio(frontMGU);
            UpdateMguAudio(rearMGU);
        }


        public override void OnDisableVehicle ()
        {
            StopMguAudio(frontMGU);
            StopMguAudio(rearMGU);
        }


        public override void OnEnterPause ()
        {
            StopMguAudio(frontMGU);
            StopMguAudio(rearMGU);
        }


        void UpdateMguAudio (MGUSettings mgu)
        {
            if (mgu.audioSource == null) return;

            mgu.audioSource.pitch = mgu.basePitch + Mathf.Abs(mgu.rpm) * mgu.rpmGain;
            mgu.audioSource.volume = mgu.baseVolume + Mathf.Abs(mgu.mechanical) * mgu.trqGain;

            // Moving the AudioListener around a source with zero or nearly-zero pitch causes artifacts.
            // The audio source is muted in such chase to prevent that

            float absPitch = MathUtility.FastAbs(mgu.audioSource.pitch);
            if (absPitch < 0.05f)
            {
                mgu.audioSource.volume *= absPitch / 0.05f;
            }

            // Ensure it's playing

    		if (!mgu.audioSource.isPlaying)
                mgu.audioSource.Play();
        }


        void StopMguAudio (MGUSettings mgu)
        {
            if (mgu.audioSource != null)
                mgu.audioSource.Stop();
        }
    }

}