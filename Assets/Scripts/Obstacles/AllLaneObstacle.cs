using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Dev.Scripts.Obstacles
{
    public class AllLaneObstacle:Obstacle
    {
        public override IEnumerator Spawn(TrackSegment segment, float t)
        {
            Vector3 position;
            Quaternion rotation;
            segment.GetPointAt(t, out position, out rotation);
            AsyncOperationHandle op = Addressables.InstantiateAsync(gameObject.name, position, rotation);
            yield return op;
            if (op.Result == null || !(op.Result is GameObject))
            {
                Debug.LogWarning(string.Format("Unable to load obstacle {0}.", gameObject.name));
                yield break;
            }
            GameObject obj = op.Result as GameObject;
            if (obj != null)
            {
                obj.transform.SetParent(segment.objectRoot, true);

                Vector3 oldPos = obj.transform.position;
                obj.transform.position += Vector3.back;
                obj.transform.position = oldPos;
            }
        }

        public override void Impacted()
        {
            base.Impacted();
        }
        
    }
}