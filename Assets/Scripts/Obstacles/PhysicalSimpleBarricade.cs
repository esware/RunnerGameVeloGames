using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace Dev.Scripts.Obstacles
{
    public class PhysicalSimpleBarricade:Obstacle
    {
        private const int MinObstacleCount = 1;
        private const int MaxObstacleCount = 3;
        private const int LeftMostLaneIndex = -1;
        private const int RightMostLaneIndex = 1;
        private const int TimeForDestroyAfterInteraction = 1;
        
        private readonly List<Rigidbody> _rigidbodies =new();
        
        private void Start()
        {
            GetChild();
        }

        private void GetChild()
        {
            foreach (Transform child in transform)
            {
                var rigid = child.gameObject.GetComponent<Rigidbody>();
                _rigidbodies.Add(rigid);
            }
        }
        
        public override IEnumerator Spawn(TrackSegment segment, float t)
        {
            
            int count =  Random.Range(MinObstacleCount, MaxObstacleCount + 1);
            int startLane =  Random.Range(LeftMostLaneIndex, RightMostLaneIndex + 1);
            
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
                    obj.transform.position += obj.transform.right * (lane * segment.trackManager.laneOffset);

                    obj.transform.SetParent(segment.objectRoot, true);
                    
                    Vector3 oldPos = obj.transform.position;
                    obj.transform.position += Vector3.back;
                    obj.transform.position = oldPos;
                    
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

        public override void Impacted()
        {
            base.Impacted();
            GetComponent<Collider>().enabled = false;

            StartCoroutine(ApplyForceAndDestroy());
        }

        private IEnumerator ApplyForceAndDestroy()
        {
            List<Rigidbody> rigidbodiesToRemove = new List<Rigidbody>();

            foreach (var rigidbody in _rigidbodies)
            {
                rigidbody.transform.SetParent(null);
                rigidbody.useGravity = true;
                rigidbody.isKinematic = false;

                var direction = (transform.position - Vector3.back + Vector3.up * 5f).normalized;
                rigidbody.AddForce(direction * 14f, ForceMode.Impulse);
                yield return null;

                rigidbodiesToRemove.Add(rigidbody);
            }

            yield return new WaitForSeconds(TimeForDestroyAfterInteraction);

            foreach (var rigidbodyToRemove in rigidbodiesToRemove)
            {
                rigidbodyToRemove.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBounce).OnComplete(() =>
                {
                    Destroy(rigidbodyToRemove.gameObject);
                });
            }

            DestroyImmediate(gameObject);
        }


    }
}