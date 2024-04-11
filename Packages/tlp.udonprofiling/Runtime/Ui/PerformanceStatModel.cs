using TLP.UdonUtils.DesignPatterns.MVC;
using TLP.UdonUtils.Sources;
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

        public double ServerTime
        {
            get
            {
                if (Utilities.IsValid(NetworkTime)) {
                    return NetworkTime.TimeAsDouble();
                }

                return Networking.GetServerTimeInMilliseconds() * 0.001;
            }
        }

        public double ServerTimeError
        {
            get
            {
                if (Utilities.IsValid(NetworkTime)) {
                    return RefNetworkTime.TimeAsDouble() - NetworkTime.TimeAsDouble();
                }

                return 0.0;
            }
        }
    }
}