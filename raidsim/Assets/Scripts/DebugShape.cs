// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;

namespace dev.susybaka.raidsim.Editor
{
    public class DebugShape : MonoBehaviour
    {
        public const string dummy = "editor.script";
#if UNITY_EDITOR
        public enum Shape { sphere, box, line, dynamicLine }

        public Color color = new Color(1, 0.64f, 0, 0.5f);
        public Shape shape = Shape.sphere;
        public bool wireframe = false;
        public bool whenSelected = false;
        public Vector3 size = Vector3.one;
        public Vector3 offset = Vector3.zero;

        private void OnDrawGizmos()
        {
            if (whenSelected)
                return;

            DrawGizmos();
        }

        public void OnDrawGizmosSelected()
        {
            if (!whenSelected)
                return;

            DrawGizmos();
        }

        private void DrawGizmos()
        {
            Gizmos.color = color;
            Vector3 position = transform.position + offset;

            switch (shape)
            {
                case Shape.sphere:
                    if (wireframe)
                        Gizmos.DrawWireSphere(position, size.x);
                    else
                        Gizmos.DrawSphere(position, size.x);
                    break;
                case Shape.box:
                    if (wireframe)
                        Gizmos.DrawWireCube(position, size);
                    else
                        Gizmos.DrawCube(position, size);
                    break;
                case Shape.line:
                    Gizmos.DrawLine(position, position + size);
                    break;
                case Shape.dynamicLine:
                    for (int i = 0; i < transform.childCount - 1; i++)
                    {
                        Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
                    }
                    break;
            }
        }
#endif
    }
}