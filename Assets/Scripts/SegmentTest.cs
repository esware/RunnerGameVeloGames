using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SegmentTest : MonoBehaviour
{
    private float _worldLength;
    public Transform pathParent;
    
    [Space,Header("Obstacle Settings")]
    public float[] obstaclePositions;

    public float[] pathPositions;
    private void OnEnable()
    {
        UpdateWorldLength();
        GameObject obj = new GameObject("ObjectRoot");
        obj.transform.SetParent(transform);
    }
    
    private void UpdateWorldLength()
    {
        _worldLength = 0;

        for (int i = 1; i < pathParent.childCount; ++i)
        {
            var orig = pathParent.GetChild(i - 1);
            var end = pathParent.GetChild(i);

            var vec = end.position - orig.position;
            _worldLength += vec.magnitude;
        }
    }
    
    private void GetPointAtInWorldUnit(float wt, out Vector3 pos, out Quaternion rot)
    {
        float t = wt / _worldLength;
        GetPointAt(t, out pos, out rot);
    }
    private void GetPointAt(float t, out Vector3 pos, out Quaternion rot)
    {
        float clampedT = Mathf.Clamp01(t);
        float segmentCount = pathParent.childCount - 1;
        float scaledT = segmentCount * clampedT;
        int index = Mathf.FloorToInt(scaledT);
        float segmentT = clampedT - index / segmentCount;

        
        if (index != pathParent.childCount - 1)
        {
            segmentT *= segmentCount;
        }

        Transform start = pathParent.GetChild(index);
        Transform end = pathParent.GetChild(Mathf.Min(index + 1, pathParent.childCount - 1));
        
        pos = Vector3.Lerp(start.position, end.position, segmentT);
        rot = Quaternion.Lerp(start.rotation, end.rotation, segmentT);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (pathParent == null)
            return;

        Color c = Gizmos.color;
        Gizmos.color = Color.red;
        for (int i = 1; i < pathParent.childCount; ++i)
        {
            Transform orig = pathParent.GetChild(i - 1);
            Transform end = pathParent.GetChild(i);

            Gizmos.DrawLine(orig.position, end.position);
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < obstaclePositions.Length; ++i)
        {
            Vector3 pos;
            Quaternion rot;
            GetPointAt(obstaclePositions[i], out pos, out rot);
            Gizmos.DrawSphere(pos, 0.5f);
        }
        Gizmos.color = c;
        
        Gizmos.color = Color.green;
        for (int i = 0; i < pathPositions.Length; ++i)
        {
            Vector3 pos;
            Quaternion rot;
            GetPointAt(pathPositions[i], out pos, out rot);
            Gizmos.DrawSphere(pos, 0.5f);
        }
    }
    
#endif
    
#if UNITY_EDITOR
    [CustomEditor(typeof(SegmentTest))]
    class TrackSegmentEditor : Editor
    {
        private SegmentTest _segment;

        public void OnEnable()
        {
            _segment = target as SegmentTest;
            _segment = (SegmentTest)target;
            EditorUtility.SetDirty(_segment);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Add obstacles"))
            {
                ArrayUtility.Add(ref _segment.obstaclePositions, 0.0f);
            }
            
            if (_segment.obstaclePositions.Length > 0)
            {
                int toremove = -1;
                for (int i = 0; i < _segment.obstaclePositions.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    _segment.obstaclePositions[i] = EditorGUILayout.Slider(_segment.obstaclePositions[i], 0.0f, 1.0f);
                    if (GUILayout.Button("-", GUILayout.MaxWidth(42)))
                    {
                        toremove = i;
                    }
                    GUILayout.EndHorizontal();
                }

                if (toremove != -1)
                {
                    ArrayUtility.RemoveAt(ref _segment.obstaclePositions, toremove);
                }
            }

            GUILayout.Space(50);
            if (GUILayout.Button("Show World Length"))
            {
                _segment.UpdateWorldLength();
                Debug.Log("World Lenght"+_segment._worldLength);
            }

            if (_segment.pathPositions.Length>0)
            {
                int remove = -1;
                for (int i = 0; i < _segment.pathPositions.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    _segment.pathPositions[i] = EditorGUILayout.Slider(_segment.pathPositions[i], 0.0f, 1.0f);
                    if (GUILayout.Button("+", GUILayout.MaxWidth(42)))
                    {
                        remove = i;
                    }
                    GUILayout.EndHorizontal();
                }
            
                if (remove != -1)
                {
                    ArrayUtility.RemoveAt(ref _segment.pathPositions, remove);
                }
            }
            
        }
    }

#endif
}


