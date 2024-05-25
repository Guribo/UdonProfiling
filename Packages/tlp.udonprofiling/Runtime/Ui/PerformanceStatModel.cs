using System;
using TLP.UdonUtils.Runtime.DesignPatterns.MVC;
using TLP.UdonUtils.Runtime.Sources;
using TLP.UdonUtils.Runtime.Sources.Time;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace TLP.UdonProfiling.Runtime.Ui
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    public class PerformanceStatModel : Model
    {
        public int CountedFrames { get; set; }
        public float AverageFrameTime { get; set; }
        public float UdonAverageFrameTimeMs { get; set; }
        public float UdonMaxFrameTimeMs { get; set; }
        public bool TooSlow { get; set; }

        public float UdonFrameTimeMs { get; set; }
        public int UdonProfiledFrames { get; set; }

        [SerializeField]
        private TimeSource NetworkTime;

        [SerializeField]
        private TimeSource RefNetworkTime;

        protected override bool SetupAndValidate() {
            if (!base.SetupAndValidate()) {
                return false;
            }

            if (!Utilities.IsValid(NetworkTime)) {
                NetworkTime = TlpNetworkTime.GetInstance();
                if (!Utilities.IsValid(NetworkTime)) {
                    ErrorAndDisableGameObject($"{nameof(NetworkTime)} is not set and also not found");
                    return false;
                }
            }

            if (!Utilities.IsValid(RefNetworkTime)) {
                var gameTime = NetworkTime.transform.Find("VrcTime");
                if (!gameTime) {
                    ErrorAndDisableGameObject(
                            $"{nameof(RefNetworkTime)} is not set and failed to find fallback 'VrcTime");
                    return false;
                }

                RefNetworkTime = gameTime.GetComponent<TimeSource>();
                if (!Utilities.IsValid(RefNetworkTime)) {
                    ErrorAndDisableGameObject(
                            $"{nameof(RefNetworkTime)} is not set and fallback is unavailable, add 'TLP_Essentials' prefab to your scene");
                    return false;
                }
            }

            return true;
        }

        public double ServerTime => NetworkTime.TimeAsDouble();

        public double VrcServerTime
        {
            get
            {
                var tlpNetworkTime = (TlpNetworkTime)NetworkTime;
                if (tlpNetworkTime) {
                    return tlpNetworkTime.SampledRealServerTime;
                }

                return 0f;
            }
        }

        public double ServerTimeError
        {
            get
            {
                var tlpNetworkTime = (TlpNetworkTime)NetworkTime;
                if (tlpNetworkTime) {
                    return tlpNetworkTime.ExactError;
                }

                return RefNetworkTime.TimeAsDouble() - NetworkTime.TimeAsDouble();
            }
        }

        public int ServerTimeSamples
        {
            get
            {
                var tlpNetworkTime = (TlpNetworkTime)NetworkTime;
                if (tlpNetworkTime) {
                    return tlpNetworkTime.Samples;
                }

                return -1;
            }
        }

        public double CumulativeError
        {
            get
            {
                var tlpNetworkTime = (TlpNetworkTime)NetworkTime;
                if (tlpNetworkTime) {
                    return tlpNetworkTime.AverageError;
                }

                return 0;
            }
        }

        public double CorrectiveDrift
        {
            get
            {
                var tlpNetworkTime = (TlpNetworkTime)NetworkTime;
                if (tlpNetworkTime) {
                    return tlpNetworkTime.AverageError / tlpNetworkTime.Samples;
                }

                return 0;
            }
        }

        public bool OutOfSync => Math.Abs(CumulativeError) > 0.01f;
    }
}