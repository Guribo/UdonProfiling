using System;
using JetBrains.Annotations;
using TLP.UdonUtils.Runtime;
using TLP.UdonUtils.Runtime.Logger;
using UdonSharp;
using UnityEngine;

namespace TLP.UdonProfiling.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [DefaultExecutionOrder(ExecutionOrder)]
    [TlpDefaultExecutionOrder(typeof(GlobalProfileKickoff), ExecutionOrder)]
    public class GlobalProfileKickoff : TlpBaseBehaviour
    {
        public override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = TlpLogger.ExecutionOrder + 1;

        [NonSerialized]
        public System.Diagnostics.Stopwatch Stopwatch;


        protected override bool SetupAndValidate() {
            if (!base.SetupAndValidate()) {
                return false;
            }

            Stopwatch = new System.Diagnostics.Stopwatch();
            return true;
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