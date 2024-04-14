using TLP.UdonUtils.DesignPatterns.MVC;
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
        internal Text UdonFrameTimeText;

        private PerformanceStatModel _performanceStatModel;

        protected override bool InitializeInternal() {
            _performanceStatModel = (PerformanceStatModel)Model;
            return Utilities.IsValid(_performanceStatModel);
        }

        public override void OnModelChanged() {
            PlayerCount.text = $"<color=#00ff00>Players:\t{VRCPlayerApi.GetPlayerCount().ToString()}</color>";
            FPSText.text = $"<color=red>{_performanceStatModel.CountedFrames.ToString()}</color>";
            FrameTimeText.text = $"<color=red>{_performanceStatModel.AverageFrameTime:F1} ms</color>";


            UdonFrameTimeText.text =
                    $"UDON performance\navg. of last {_performanceStatModel.UdonProfiledFrames} frames:\t{_performanceStatModel.UdonAverageFrameTimeMs:F3} ms\nmax:\t{_performanceStatModel.UdonMaxFrameTimeMs:F3} ms\nfixed:\t {Time.fixedDeltaTime * 1000f:F3} ms\nlast:\t{_performanceStatModel.UdonFrameTimeMs:F3} ms";

            if (_performanceStatModel.TooSlow) {
                UdonFrameTimeText.text = $"<color=red>{UdonFrameTimeText.text}</color>";
            }
        }

        public void LateUpdate() {
            ServerTimeText.text =
                    $"<color=cyan>Network time:\t{_performanceStatModel.ServerTime * 1000:F3} ms\nVRC error:\t\t{_performanceStatModel.ServerTimeError * 1000:F3} ms</color>";
        }
    }
}