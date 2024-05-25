using System;
using JetBrains.Annotations;
using TLP.UdonUtils.Runtime;
using TLP.UdonUtils.Runtime.Logger;
using UdonSharp;
using UnityEngine;

namespace TLP.UdonProfiling.Runtime
{
    [DefaultExecutionOrder(ExecutionOrder)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GlobalProfileKickoff : TlpBaseBehaviour
    {
        protected override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = TlpLogger.ExecutionOrder + 1;

        [NonSerialized]
        public System.Diagnostics.Stopwatch Stopwatch;

        public override void Start() {
            base.Start();
            Stopwatch = new System.Diagnostics.Stopwatch();
        }

        private void FixedUpdate() {
            Stopwatch.Restart();
        }

        private void Update() {
            Stopwatch.Restart();
        }

        private void LateUpdate() {
            Stopwatch.Restart();
        }

        public override void PostLateUpdate() {
            Stopwatch.Restart();
        }
    }
}