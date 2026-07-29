using UnityEngine;

namespace Wizard
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PadTextFixedScale : MonoBehaviour
    {
        [SerializeField, Min(0.001f)] private float worldScale = 0.095f;

        private Vector3 _lastParentScale;

        private void OnEnable()
        {
            ApplyScale();
        }

        private void OnValidate()
        {
            ApplyScale();
        }

        private void LateUpdate()
        {
            if (transform.parent == null) return;

            Vector3 parentScale = transform.parent.lossyScale;
            if (parentScale == _lastParentScale) return;

            ApplyScale(parentScale);
        }

        private void ApplyScale()
        {
            if (transform.parent == null) return;

            ApplyScale(transform.parent.lossyScale);
        }

        private void ApplyScale(Vector3 parentScale)
        {
            transform.localScale = new Vector3(
                worldScale / Mathf.Max(Mathf.Abs(parentScale.x), 0.0001f),
                worldScale / Mathf.Max(Mathf.Abs(parentScale.y), 0.0001f),
                worldScale / Mathf.Max(Mathf.Abs(parentScale.z), 0.0001f));
            _lastParentScale = parentScale;
        }
    }
}
