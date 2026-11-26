using UnityEngine;
using UnityEngine.AI;

namespace TimeMachine.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class CompanionController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform targetToFollow;
        [SerializeField] private float stoppingDistance = 2.2f;
        [SerializeField] private float followSpeed = 3.8f;
        [SerializeField] private float updateInterval = 0.2f;

        private NavMeshAgent navAgent;
        private float nextUpdateTime;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.stoppingDistance = stoppingDistance;
                navAgent.speed = followSpeed;
            }
        }

        private void Start()
        {
            ValidateTarget();
        }

        public void Initialize(Transform target)
        {
            targetToFollow = target;
        }

        private void ValidateTarget()
        {
            if (targetToFollow == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("[TimeMachine] [CompanionController] Цель слежения не назначена. Fallback поиск игрока в Editor.", this);
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) targetToFollow = playerObj.transform;
#else
                Debug.LogError("[TimeMachine] [CompanionController] КРИТИЧЕСКАЯ ОШИБКА: Цель слежения (targetToFollow) не установлена!", this);
#endif
            }
        }

        private void Update()
        {
            if (targetToFollow == null || navAgent == null || !navAgent.isOnNavMesh) return;

            if (Time.time >= nextUpdateTime)
            {
                nextUpdateTime = Time.time + updateInterval;
                
                float sqrDistance = (targetToFollow.position - transform.position).sqrMagnitude;
                if (sqrDistance > stoppingDistance * stoppingDistance)
                {
                    navAgent.SetDestination(targetToFollow.position);
                }
            }
        }

        /// <summary>
        /// Безопасная телепортация спутника при переходе между эпохами или локациями.
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.Warp(position);
            }
            else
            {
                transform.position = position;
            }
        }
    }
}
