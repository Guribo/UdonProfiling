using System.Diagnostics;
using JetBrains.Annotations;
using TLP.UdonUtils.Runtime.DesignPatterns.MVC;
using TLP.UdonUtils.Runtime.EditorOnly;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace TLP.UdonProfiling.Runtime.Ui
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [DefaultExecutionOrder(ExecutionOrder)]
    [TlpDefaultExecutionOrder(typeof(PerformanceStatController), ExecutionOrder)]
    public class PerformanceStatController : Controller
    {
        public override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = Controller.ExecutionOrder + 10;

        [SerializeField]
        [Range(1, 10)]
        private float UpdateInterval = 1f;

        #region State
        private int _previousFrameCount;
        private float _lastUpdated;
        private float _lastIntervalDuration;
        private PerformanceStatModel _performanceStatModel;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        #endregion

        #region Monobehaviour
        internal void OnEnable() {
            if (HasStartedOk) {
                Restart();
            }
        }

        protected override bool SetupAndValidate() {
            if (!base.SetupAndValidate()) {
                return false;
            }

            if (!InitializeMvcSingleGameObject(gameObject)) {
                Error($"MVC failed to initialize");
                return false;
            }

            Restart();
            return true;
        }

        private void Restart() {
            #region TLP_DEBUG
#if TLP_DEBUG
            DebugLog(nameof(Restart));
#endif
            #endregion

            Info("Starting refresh loop");
            _previousFrameCount = Time.frameCount;
            _lastUpdated = Time.timeSinceLevelLoad;
            SendCustomEventDelayedSeconds(nameof(UpdateFrameRate), UpdateInterval);
        }
        #endregion

        #region VRC
        public override void OnPlayerJoined(VRCPlayerApi player) {
            if (!IsControllerInitialized) {
                return;
            }

            if (!Utilities.IsValid(_performanceStatModel)) {
                Error($"{nameof(_performanceStatModel)} invalid");
            }

            _performanceStatModel.Dirty = true;
            _performanceStatModel.NotifyIfDirty(1);
        }

        public override void OnPlayerLeft(VRCPlayerApi player) {
            if (!IsControllerInitialized) {
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

            if (IsActiveAndEnabled)
                SendCustomEventDelayedSeconds(nameof(UpdateFrameRate), UpdateInterval);
            else {
                Info("Stopping refresh loop");
            }
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