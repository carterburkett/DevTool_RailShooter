using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beziers {
    public abstract class BezierManager : MonoBehaviour {
        [HideInInspector] protected enum _objectType {bezier, point};
        [SerializeField] protected _objectType type;

        [SerializeField] protected GameObject objectOnRail;
        [Range(0f, 1f)]
        [SerializeField] protected float mTime;
        [SerializeField] protected float lookSpeed;
        [Tooltip("Do not set to false unless you've tied forward movement to player input")]
        [SerializeField] protected bool autoMove = true;
        [Range(0.001f, 10f)]
        [SerializeField] protected float speed;
        [SerializeField] protected bool isCyclic = true;
        private protected List<Transform> waypoints;
        protected Vector3 myPosition;

        

        public abstract void GetBezier(out Vector3 pos, List<Transform> Checkpoints, float time);

        private IEnumerator Start() {
            while (true) {
                if (autoMove) {
                    mTime += Time.deltaTime * speed;

                    if (speed <= 0f) {
                        speed = 0.1f;
                    }

                    if (mTime >= 1f && isCyclic) {
                        mTime = 0f;
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}