using System;
using TLP.UdonUtils.DesignPatterns.MVC;
using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
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
            PlayerCount.text = string.Format(
                    "<color=#00ff00>Players:\t{0}</color>",
                    VRCPlayerApi.GetPlayerCount().ToString()
            );
            FPSText.text = string.Format("<color=red>{0}</color>", _performanceStatModel.CountedFrames.ToString());
            FrameTimeText.text = string.Format(
                    "<color=red>{0} ms</color>",
                    _performanceStatModel.AverageFrameTime.ToString("F1")
            );


            UdonFrameTimeText.text =
                    $"UDON performance\navg. of last {_performanceStatModel.UdonProfiledFrames} frames:\t{_performanceStatModel.UdonAverageFrameTimeMs:F3} ms\nmax:\t{_performanceStatModel.UdonMaxFrameTimeMs:F3} ms\nfixed:\t {Time.fixedDeltaTime * 1000f:F3} ms\nlast:\t{_performanceStatModel.UdonFrameTimeMs:F3} ms";

            if (_performanceStatModel.TooSlow) {
                UdonFrameTimeText.text = $"<color=red>{UdonFrameTimeText.text}</color>";
            }
        }

        public void LateUpdate() {
            ServerTimeText.text = string.Format(
                    "<color=cyan>Network time:\t{0} s\nVRC error:\t\t{1} ms</color>",
                    _performanceStatModel.ServerTime.ToString("F4"),
                    _performanceStatModel.ServerTimeError.ToString("F3")
            );
        }
    }
}