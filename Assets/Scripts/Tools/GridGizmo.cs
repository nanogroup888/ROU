// ------------------------------------------------
// File: Assets/Scripts/Tools/GridGizmo.cs
// ------------------------------------------------
using UnityEngine;

namespace ROLike.Tools {
    [ExecuteAlways]
    public class GridGizmo : MonoBehaviour {
        public float cellSize = 2f;
        public int halfCells = 64; // 64 → ~256m span at 2m cell
        public Color lineColor = new Color(0f, 0f, 0f, 0.15f);
        public Color axisColor = new Color(0.2f, 0.5f, 1f, 0.6f);

        private void OnDrawGizmos(){
            Gizmos.color = lineColor;
            Vector3 c = transform.position;
            int N = halfCells;
            float s = cellSize;
            for (int i=-N; i<=N; i++){
                Vector3 a = c + new Vector3(i*s, 0f, -N*s);
                Vector3 b = c + new Vector3(i*s, 0f,  N*s);
                Vector3 c1 = c + new Vector3(-N*s, 0f, i*s);
                Vector3 d1 = c + new Vector3( N*s, 0f, i*s);
                Gizmos.DrawLine(a,b);
                Gizmos.DrawLine(c1,d1);
            }
            Gizmos.color = axisColor; // axes
            Gizmos.DrawLine(c + new Vector3(-N*s,0,0), c + new Vector3(N*s,0,0));
            Gizmos.DrawLine(c + new Vector3(0,0,-N*s), c + new Vector3(0,0,N*s));
        }
    }
}
