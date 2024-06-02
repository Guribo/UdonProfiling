using TLP.UdonUtils.Runtime.DesignPatterns.MVC;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace TLP.UdonProfiling.Runtime.Ui
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    public class PerformanceStatView : View
    {
        [SerializeField]
        internal Text PlayerCount;

        [SerializeField]
        internal Text FPSText;

        [SerializeField]
        internal Text FrameTimeText;

        [SerializeField]
        internal Text ServerTimeText;

        [SerializeField]
        internal Text ServerDescriptionText;

        [SerializeField]
        internal Text UdonFrameTimeText;

        private PerformanceStatModel _performanceStatModel;



        protected override bool InitializeInternal() {
            _performanceStatModel = (PerformanceStatModel)Model;
            return Utilities.IsValid(_performanceStatModel);
        }

        public override void OnModelChanged() {
            if (_performanceStatModel.RealTimeMode) {
                return;
            }

            UpdateView();
        }

        public void LateUpdate() {
            if (_performanceStatModel.RealTimeMode) {
                UpdateView();
            }
        }

        private void UpdateView() {
            ServerDescriptionText.text = $"<color=cyan>\n" +
                                         $"TLP Time :\n" +
                                         $"VRC Time :\n" +
                                         $"Corrective drift each frame:\n" +
                                         $"Error (average of {_performanceStatModel.ServerTimeSamples} samples):\n" +
                                         $"Exact error:\n</color>";
            string text =
                    $"TLP network time\n" +
                    $"{_performanceStatModel.ServerTime * 1000:F3} ms\n" +
                    $"{_performanceStatModel.VrcServerTime * 1000:F3} ms\n" +
                    $"{_performanceStatModel.CorrectiveDrift * 1000:F3} ms\n" +
                    $"{_performanceStatModel.CumulativeError * 1000:F3} ms\n" +
                    $"{_performanceStatModel.ServerTimeError * 1000:F3} ms\n";

            if (_performanceStatModel.OutOfSync) {
                ServerTimeText.text = $"<color=red>{text}</color>";
            } else {
                ServerTimeText.text = $"<color=cyan>{text}</color>";
            }

            PlayerCount.text = $"<color=#00ff00>Players:\t{VRCPlayerApi.GetPlayerCount().ToString()}</color>";
            FPSText.text = $"<color=red>{_performanceStatModel.CountedFrames.ToString()}</color>";
            FrameTimeText.text = $"<color=red>{_performanceStatModel.AverageFrameTime:F1} ms</color>";


            UdonFrameTimeText.text =
                    $"UDON performance\n" +
                    $"avg. of last {_performanceStatModel.UdonProfiledFrames} frames: \t{_performanceStatModel.UdonAverageFrameTimeMs:F3} ms\n" +
                    $"max:\t{_performanceStatModel.UdonMaxFrameTimeMs:F3} ms\n" +
                    $"fixed:\t {Time.fixedDeltaTime * 1000f:F3} ms\n" +
                    $"last: \t{_performanceStatModel.UdonFrameTimeMs:F3} ms";

            if (_performanceStatModel.TooSlow) {
                UdonFrameTimeText.text = $"<color=red>{UdonFrameTimeText.text}</color>";
            }
        }
    }
}