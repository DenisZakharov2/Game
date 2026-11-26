using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public class FreeRoamController : MonoBehaviour
    {
        [Header("POI Scanning Settings")]
        [SerializeField] private float scanInterval = 0.25f;
        [SerializeField] private float maxInteractionDistance = 3.0f;
        [SerializeField] private float maxScanRadius = 20.0f;

        [Header("References")]
        [SerializeField] private Transform playerTransform;

        private readonly List<Transform> poiList = new List<Transform>();
        private Transform currentNearestPoi;
        private float nextScanTime;
        private bool isScanningActive = true;

        private void Start()
        {
            ValidateDependencies();
        }

        public void Initialize(Transform targetPlayer)
        {
            playerTransform = targetPlayer;
        }

        private void ValidateDependencies()
        {
            if (playerTransform == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[TimeMachine] [FreeRoamController] playerTransform не назначен. Поиск игрока по тегу в Editor fallback.", this);
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
#else
                Debug.LogError("[TimeMachine] [FreeRoamController] КРИТИЧЕСКАЯ ОШИБКА: playerTransform не установлена!", this);
#endif
            }
        }

        public void RegisterPOI(Transform poi)
        {
            if (poi != null && !poiList.Contains(poi))
            {
                poiList.Add(poi);
            }
        }

        public void UnregisterPOI(Transform poi)
        {
            if (poi != null && poiList.Contains(poi))
            {
                poiList.Remove(poi);
            }
        }

        public void SetScanningActive(bool active)
        {
            isScanningActive = active;
            if (!active)
            {
                currentNearestPoi = null;
            }
        }

        private void Update()
        {
            if (!isScanningActive || playerTransform == null || poiList.Count == 0) return;

            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + scanInterval;
                ScanPointsOfInterest();
            }
        }

        private void ScanPointsOfInterest()
        {
            Transform nearest = null;
            float minSqrDistance = maxInteractionDistance * maxInteractionDistance;
            float maxScanSqrDistance = maxScanRadius * maxScanRadius;

            Vector3 playerPos = playerTransform.position;

            for (int i = poiList.Count - 1; i >= 0; i--)
            {
                Transform poi = poiList[i];
                if (poi == null)
                {
                    poiList.RemoveAt(i);
                    continue;
                }

                Vector3 diff = poi.position - playerPos;
                float sqrDist = diff.sqrMagnitude;

                // Ранний выход по максимальной дистанции
                if (sqrDist > maxScanSqrDistance) continue;

                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    nearest = poi;
                }
            }

            if (currentNearestPoi != nearest)
            {
                currentNearestPoi = nearest;
                OnNearestPoiChanged(currentNearestPoi);
            }
        }

        private void OnNearestPoiChanged(Transform newPoi)
        {
            if (newPoi != null)
            {
                Debug.Log($"[TimeMachine] [FreeRoamController] Обнаружена ближайшая точка интереса: {newPoi.name}");
            }
        }
    }
}
