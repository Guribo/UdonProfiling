using TLP.UdonUtils.DesignPatterns.MVC;
using TLP.UdonUtils.Sources;
using TLP.UdonUtils.Sources.Time;
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
                Error($"{nameof(NetworkTime)} is not set");
                return false;
            }

            if (!Utilities.IsValid(RefNetworkTime)) {
                Error($"{nameof(RefNetworkTime)} is not set");
                return false;
            }

            return true;
        }

        public double ServerTime => NetworkTime.TimeAsDouble();

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
    }
}