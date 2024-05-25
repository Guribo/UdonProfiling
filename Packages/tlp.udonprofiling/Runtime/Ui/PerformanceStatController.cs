using System.Diagnostics;
using TLP.UdonUtils.Runtime.DesignPatterns.MVC;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace TLP.UdonProfiling.Runtime.Ui
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    public class PerformanceStatController : Controller
    {
        [SerializeField]
        [Range(1, 10)]
        private float UpdateInterval = 1f;

        #region State
        private int _previousFrameCount;
        private bool _hasStarted;
        private float _lastUpdated;
        private float _lastIntervalDuration;
        private PerformanceStatModel _performanceStatModel;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        #endregion

        #region Monobehaviour
        internal void OnEnable() {
            if (!Initialized) {
                enabled = false;
                return;
            }

            if (_hasStarted) {
                Start();
            }
        }

        public override void Start() {
            base.Start();
            if (!Initialized && !InitializeMvcSingleGameObject(gameObject)) {
                Error($"Failed to initialize");
                enabled = false;
                return;
            }

            _previousFrameCount = Time.frameCount;
            _lastUpdated = Time.timeSinceLevelLoad;
            SendCustomEventDelayedSeconds(nameof(UpdateFrameRate), UpdateInterval);
            _hasStarted = true;
        }
        #endregion

        #region VRC
        public override void OnPlayerJoined(VRCPlayerApi player) {
            if (!Initialized) {
                return;
            }

            if (!Utilities.IsValid(_performanceStatModel)) {
                Error($"{nameof(_performanceStatModel)} invalid");
            }

            _performanceStatModel.Dirty = true;
            _performanceStatModel.NotifyIfDirty(1);
        }

        public override void OnPlayerLeft(VRCPlayerApi player) {
            if (!Initialized) {
                return;
            }

            _performanceStatModel.Dirty = true;
            _performanceStatModel.NotifyIfDirty(1);
        }
        #endregion

        #region Internal
        public void UpdateFrameRate() {
            float currentTime = Time.timeSinceLevelLoad;
            _lastIntervalDuration = currentTime - _lastUpdated;
            if (_lastIntervalDuration < UpdateInterval) {
                return;
            }

            double elapsedTotalMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            _stopwatch.Restart();

            _lastUpdated = currentTime;

            int frameCount = Time.frameCount;
            int countedFrames = frameCount - _previousFrameCount;
            _performanceStatModel.CountedFrames = countedFrames;

            if (countedFrames > 0) {
                _performanceStatModel.AverageFrameTime = (float)(elapsedTotalMilliseconds / countedFrames);
            } else {
                _performanceStatModel.AverageFrameTime = 0f;
            }

            _previousFrameCount = frameCount;
            _performanceStatModel.Dirty = true;
            _performanceStatModel.NotifyIfDirty(1);

            SendCustomEventDelayedSeconds(nameof(UpdateFrameRate), UpdateInterval);
        }

        protected override bool InitializeInternal() {
#if TLP_DEBUG
            DebugLog(nameof(InitializeInternal));
#endif

            _performanceStatModel = (PerformanceStatModel)Model;
            enabled = true;

            bool initSuccess = Utilities.IsValid(_performanceStatModel);
            if (initSuccess) {
                enabled = true;
            }

            return initSuccess;
        }
        #endregion
    }
}