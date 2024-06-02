using TLP.UdonProfiling.Runtime.Ui;
using TLP.UdonUtils.Runtime;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace TLP.UdonProfiling.Runtime
{
    [DefaultExecutionOrder(int.MaxValue)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GlobalProfileHandler : TlpBaseBehaviour
    {
        private GlobalProfileKickoff _kickoff;

        public float MaxFrameTimeMs = 10f;

        [SerializeField]
        [Range(1, 10000)]
        private int MeasureDurationInFrames = 1000;

        public override void Start() {
            base.Start();
            _kickoff = GetComponent<GlobalProfileKickoff>();
            _model = GetComponent<PerformanceStatModel>();
            if (Utilities.IsValid(_model)) {
                return;
            }

            Error($"{nameof(PerformanceStatModel)} component required");
            enabled = false;
        }

        private int _currentFrame = -1;
        private float _elapsedTime;
        private float _measuredTimeTotal;
        private int _measuredTimeFrameCount;
        private float _maxMeasured;
        private PerformanceStatModel _model;

        private void FixedUpdate() {
            if (_currentFrame != Time.frameCount) {
                _elapsedTime = 0f;
                _currentFrame = Time.frameCount;
            }

            if (_kickoff) {
                _elapsedTime += (float)_kickoff.Stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        private void Update() {
            if (_currentFrame != Time.frameCount) // FixedUpdate didn't run this frame, so reset the time
            {
                _elapsedTime = 0f;
            }

            if (_kickoff) {
                _elapsedTime += (float)_kickoff.Stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        private void LateUpdate() {
            if (_kickoff) {
                _elapsedTime += (float)_kickoff.Stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        public override void PostLateUpdate() {
            if (_kickoff) {
                _elapsedTime += (float)_kickoff.Stopwatch.Elapsed.TotalMilliseconds;
            }

            if (_elapsedTime > _maxMeasured) {
                _maxMeasured = _elapsedTime;
                _model.UdonMaxFrameTimeMs = _maxMeasured;
            }

            _measuredTimeTotal += _elapsedTime;
            _measuredTimeFrameCount++;

            float thresholdMs = MaxFrameTimeMs > 0f ? MaxFrameTimeMs : Time.fixedDeltaTime * 1000f;
            bool tooSlow = _elapsedTime > thresholdMs;
            _model.UdonFrameTimeMs = _elapsedTime;
            _model.UdonProfiledFrames = _measuredTimeFrameCount;

            float udonTotalTimeMs = _measuredTimeTotal / _measuredTimeFrameCount;
            _model.UdonAverageFrameTimeMs = udonTotalTimeMs;

            if (_measuredTimeFrameCount >= MeasureDurationInFrames) {
                _measuredTimeTotal = 0f;
                _measuredTimeFrameCount = 0;
                _maxMeasured = 0;
            }

            if (tooSlow) {
                if (Utilities.IsValid(Logger) && Logger.CreateDebugFrameLog) {
                    Warn(
                            $"UDON is taking way too long: {thresholdMs:F4}ms < {_elapsedTime:F4}ms\n{Logger.DebugLogOfFrame}"
                    );
                } else {
                    Warn($"UDON is taking way too long: {thresholdMs:F4}ms < {_elapsedTime:F4}ms (no logs available)");
                }

                _model.TooSlow = true;
                _model.Dirty = true;
                _model.NotifyIfDirty();
            } else {
                _model.TooSlow = false;
            }
        }
    }
}