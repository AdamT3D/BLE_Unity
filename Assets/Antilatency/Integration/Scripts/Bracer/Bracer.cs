// Copyright 2020, ALT LLC. All Rights Reserved.
// This file is part of Antilatency SDK.
// It is subject to the license terms in the LICENSE file found in the top-level directory
// of this distribution and at http://www.antilatency.com/eula
// You may not use this file except in compliance with the License.
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Antilatency.DeviceNetwork;

namespace Antilatency.Integration {
    /// <summary>
    /// Bracer basic implementation.
    /// </summary>
    public class Bracer : BracerComponent {
        [System.Serializable]
        public class BracerTouchEvent : UnityEngine.Events.UnityEvent<BracerTouchState> { }

        /// <summary>
        /// Only bracer marked with corresponding tag will be used by this component.
        /// </summary>
        public string BracerTag = "";

        /// <summary>
        /// </summary>
        public uint TouchSensivity = 700;

        /// <summary>
        /// Is bracer touchpad currently pressed or released.
        /// </summary>
        public BracerTouchState CurrentTouchState {
            get {
                return _touchPressed ? BracerTouchState.Pressed : BracerTouchState.Released;
            }
        }

        /// <summary>
        /// Will be called every time bracer when touchpad is pressed or released.
        /// </summary>
        public BracerTouchEvent BracerTouch = new BracerTouchEvent();

        private bool _touchPressed;

        protected override NodeHandle GetAvailableBracerNode() {
            var result = new NodeHandle();

            var network = GetNativeNetwork();
            if (network == null) {
                return result;
            }

            using (var cotaskConstructor = _bracerLibrary.getCotaskConstructor()) {
                if (cotaskConstructor == null) {
                    return result;
                }

                var nodes = cotaskConstructor.findSupportedNodes(network).Where(v =>
                        network.nodeGetStringProperty(v, "Tag") == BracerTag &&
                        network.nodeGetStatus(v) == Antilatency.DeviceNetwork.NodeStatus.Idle
                    ).ToList();

                if (nodes.Count > 0) {
                    result = nodes[0];
                }

                return result;
            }
        }

        protected override void Update() {
            base.Update();

            uint touchValue;
            if (!GetBracerTouchValue(out touchValue)) {
                return;
            }

            if (touchValue < TouchSensivity && !_touchPressed) {
                ExecuteVibrarion();
                _touchPressed = true;
                BracerTouch.Invoke(BracerTouchState.Pressed);
            }
            if (touchValue > TouchSensivity && _touchPressed) {
                ExecuteVibrarion();
                _touchPressed = false;
                BracerTouch.Invoke(BracerTouchState.Released);
            }
        }
    }
}