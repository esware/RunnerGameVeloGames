using System;
using System.Collections;
using System.Numerics;
using Dev.Scripts.Track;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace Dev.Scripts.Obstacles
{
    public class MovingObstacle:Obstacle
    {
        public Vector3 movingDirection;
        public float speed;
        
        public int maxObstacleCount = 3;
        public int minObstacleCount = 1;
        public int interactionDistance;
        
        private const int LeftMostLaneIndex = -1;
        private const int RightMostLaneIndex = 1;

        public TrackSegment _segment;
        private Vector3 _position;
        
        public override IEnumerator Spawn(TrackSegment segment, float t)
        {
            _segment = segment;
            int count =  Random.Range(minObstacleCount, maxObstacleCount + 1);
            int startLane =  Random.Range(LeftMostLaneIndex, RightMostLaneIndex + 1);
            
            Quaternion rotation;
            segment.GetPointAt(t, out _position, out rotation);

            for(int i = 0; i < count; ++i)
            {
                int lane = startLane + i;
                lane = lane > RightMostLaneIndex ? LeftMostLaneIndex : lane;

                AsyncOperationHandle op = Addressables.InstantiateAsync(gameObject.name, _position, rotation);
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
                    obj.transform.position += obj.transform.right * (lane * segment.trackManager.laneOffset);

                    obj.transform.SetParent(segment.objectRoot,true);
                    
                    
                    if (randomColor)
                    {
                        int colorIndex = Random.Range(0, colors.Length - 1);
                        var renderers = obj.GetComponentsInChildren<Renderer>();
                        foreach (var r in renderers)
                        {
                            r.materials[0].color = colors[colorIndex];
                        }
                    }
                    
                }
            }
        }

        private void Update()
        {
            CheckMove();
        }

        private void CheckMove()
        {
            if (_segment==null) return;
            if (!_segment.trackManager.IsMoving) return;
            
            if (transform.position.z - _segment.trackManager.WorldDistance <= interactionDistance )
            {
                Move();
            }
        }

        private void Move()
        {
            var position = transform.position + movingDirection;
            transform.DOMove(position, 1f/speed);
        }
    }
}