using UnityEngine;
using UnityEngine.AI;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public enum EscortBehaviorMode
    {
        IdleInShelter = 0,
        GuideToWaypoint = 1,
        FollowPlayerPair = 2,
        GatherForDualJump = 3
    }

    public class Sonya2040EscortController : MonoBehaviour
    {
        [Header("Escort Tuning (14 November 1950 Layer)")]
        [SerializeField] private Transform sonya2027Transform;
        [SerializeField] private float waitPlayerDistance = 6.2f;
        [SerializeField] private float pairFollowDistance = 1.8f;
        [SerializeField] private float warpCatchupDistance = 18.0f;
        [SerializeField] private float updateInterval = 0.2f;

        private NavMeshAgent _agent;
        private EscortBehaviorMode _mode = EscortBehaviorMode.IdleInShelter;
        private Vector3 _currentWaypoint;
        private float _timer;

        public EscortBehaviorMode CurrentMode => _mode;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed = 2.7f;
                _agent.stoppingDistance = pairFollowDistance;
            }
        }

        public void BindPlayer(Transform player)
        {
            sonya2027Transform = player;
        }

        public void StartGuidingTo(Vector3 worldDestination)
        {
            _currentWaypoint = worldDestination;
            _mode = EscortBehaviorMode.GuideToWaypoint;
        }

        public void SwitchToPairFollow()
        {
            _mode = EscortBehaviorMode.FollowPlayerPair;
        }

        public void GatherAtBridgeAnchor(Vector3 bridgeAnchorPosition)
        {
            _currentWaypoint = bridgeAnchorPosition;
            _mode = EscortBehaviorMode.GatherForDualJump;
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.stoppingDistance = 1.2f;
                _agent.SetDestination(_currentWaypoint);
            }
        }

        private void Update()
        {
            if (sonya2027Transform == null || _agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < updateInterval)
            {
                return;
            }
            _timer = 0f;

            float sqrPlayerDist = (sonya2027Transform.position - transform.position).sqrMagnitude;

            if (_mode == EscortBehaviorMode.GuideToWaypoint)
            {
                if (sqrPlayerDist > waitPlayerDistance * waitPlayerDistance)
                {
                    _agent.isStopped = true;
                    Vector3 lookDir = sonya2027Transform.position - transform.position;
                    lookDir.y = 0f;
                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.LookRotation(lookDir);
                    }
                }
                else
                {
                    _agent.isStopped = false;
                    _agent.stoppingDistance = 1.5f;
                    _agent.SetDestination(_currentWaypoint);
                }
            }
            else if (_mode == EscortBehaviorMode.FollowPlayerPair)
            {
                if (sqrPlayerDist > warpCatchupDistance * warpCatchupDistance)
                {
                    Vector3 targetPos = sonya2027Transform.position - sonya2027Transform.forward * pairFollowDistance;
                    if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                    {
                        _agent.Warp(hit.position);
                    }
                    return;
                }

                if (sqrPlayerDist > pairFollowDistance * pairFollowDistance)
                {
                    _agent.isStopped = false;
                    _agent.stoppingDistance = pairFollowDistance;
                    _agent.SetDestination(sonya2027Transform.position);
                }
            }
        }
    }
}
