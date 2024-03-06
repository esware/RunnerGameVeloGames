using Dev.Scripts;
using Dev.Scripts.Track;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackSegment : MonoBehaviour
{
    [HideInInspector]
    public TrackManager manager;
    
    [Space,Header("Segment Settings")]
    public Transform pathParent;
    public Transform objectRoot;
	public Transform collectibleTransform;

    [Space,Header("Obstacle Settings")]
    public AssetReference[] possibleObstacles;
    public float[] obstaclePositions;


    #region Properties

    public float WorldLength => _worldLength;
    private float _worldLength;

    #endregion
    

    private void OnEnable()
    {
        UpdateWorldLength();
        GameObject obj = new GameObject("ObjectRoot");
		obj.transform.SetParent(transform);
		objectRoot = obj.transform;

		obj = new GameObject("Collectibles");
		obj.transform.SetParent(objectRoot);
		collectibleTransform = obj.transform;
    }
    public void GetPointAtInWorldUnit(float wt, out Vector3 pos, out Quaternion rot)
    {
        float t = wt / _worldLength;
        GetPointAt(t, out pos, out rot);
    }
    public void GetPointAt(float t, out Vector3 pos, out Quaternion rot)
    {
        float clampedT = Mathf.Clamp01(t);
        float scaledT = (pathParent.childCount - 1) * clampedT;
        int index = Mathf.FloorToInt(scaledT);
        float segmentT = clampedT - index;

        Transform orig = pathParent.GetChild(index);
        if (index == pathParent.childCount - 1)
        {
            pos = orig.position;
            rot = orig.rotation;
            return;
        }

        Transform target = pathParent.GetChild(index + 1);

        pos = Vector3.Lerp(orig.position, target.position, segmentT);
        rot = Quaternion.Lerp(orig.rotation, target.rotation, segmentT);
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

	public void Cleanup()
	{
		while(collectibleTransform.childCount > 0)
		{
			Transform t = collectibleTransform.GetChild(0);
			t.SetParent(null);
            Coin.CoinPool.Free(t.gameObject);
		}

	    Addressables.ReleaseInstance(gameObject);
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
    }
    
    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TrackSegment))]
class TrackSegmentEditor : Editor
{
    private TrackSegment m_Segment;

    public void OnEnable()
    {
        m_Segment = target as TrackSegment;
        m_Segment = (TrackSegment)target;
        EditorUtility.SetDirty(m_Segment);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add obstacles"))
        {
            ArrayUtility.Add(ref m_Segment.obstaclePositions, 0.0f);
        }
        
        if (m_Segment.obstaclePositions.Length > 0)
        {
            int toremove = -1;
            for (int i = 0; i < m_Segment.obstaclePositions.Length; i++)
            {
                GUILayout.BeginHorizontal();
                m_Segment.obstaclePositions[i] = EditorGUILayout.Slider(m_Segment.obstaclePositions[i], 0.0f, 1.0f);
                if (GUILayout.Button("-", GUILayout.MaxWidth(32)))
                {
                    toremove = i;
                }
                GUILayout.EndHorizontal();
            }

            if (toremove != -1)
            {
                ArrayUtility.RemoveAt(ref m_Segment.obstaclePositions, toremove);
            }
        }
    }
}

#endif