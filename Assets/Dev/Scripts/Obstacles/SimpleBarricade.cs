using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Dev.Scripts.Obstacles
{
    public class SimpleBarricade :Obstacle
    {
        private const int MinObstacleCount = 1;
        private const int MaxObstacleCount = 2;
        private const int LeftMostLaneIndex = -1;
        private const int RightMostLaneIndex = 1;
        
        public override IEnumerator Spawn(TrackSegment segment, float t)
        {
            
            int count =  Random.Range(MinObstacleCount, MaxObstacleCount + 1);
            int startLane =  Random.Range(LeftMostLaneIndex, RightMostLaneIndex + 1);
            int colorIndex = Random.Range(0, colors.Length - 1);

            Vector3 position;
            Quaternion rotation;
            segment.GetPointAt(t, out position, out rotation);

            for(int i = 0; i < count; ++i)
            {
                int lane = startLane + i;
                lane = lane > RightMostLaneIndex ? LeftMostLaneIndex : lane;

                AsyncOperationHandle op = Addressables.InstantiateAsync(gameObject.name, position, rotation);
                yield return op;
                if (op.Result == null || !(op.Result is GameObject))
                {
                    Debug.LogWarning(string.Format("Unable to load obstacle {0}.", gameObject.name));
                    yield break;
                }
                GameObject obj = op.Result as GameObject;

                if (obj == null)
                    Debug.Log(gameObject.name);
                else
                {
                    obj.transform.position += obj.transform.right * (lane * segment.manager.laneOffset);

                    obj.transform.SetParent(segment.objectRoot, true);

                    //TODO : remove that hack related to #issue7
                    Vector3 oldPos = obj.transform.position;
                    obj.transform.position += Vector3.back;
                    obj.transform.position = oldPos;
                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        r.materials[0].color = colors[colorIndex];
                    }
                }
            }
        }

        public override void Impacted()
        {
            base.Impacted();
        }
        
    }
}