using UnityEngine;
using UnityEngine.AI;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public class CompanionController : MonoBehaviour
    {
        [Header("Identity & Following")]
        [SerializeField] private string characterId = GameConstants.CharacterIds.Sasha;
        [SerializeField] private Transform followTarget;
        [SerializeField] private float desiredFollowDistance = 2.0f;
        [SerializeField] private float teleportCatchupDistance = 22.0f;
        [SerializeField] private float repathInterval = 0.25f;

        [Header("Guiding Hints (Never Reveals Direct Answer)")]
        [SerializeField] private string[] guidingHints = new string[]
        {
            "Посмотри на единицы измерения в условии — переведи всё в одинаковые величины!",
            "Проверь состав слова или орфограмму по правилу 4 класса, не торопись.",
            "Сверь ориентир с запиской из 1950 года или «Компасом поколений»!"
        };

        private NavMeshAgent _agent;
        private float _repathTimer;
        private int _hintIndex;

        public string CharacterId => characterId;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.stoppingDistance = desiredFollowDistance;
                _agent.speed = 3.1f;
            }
        }

        private void OnEnable()
        {
            GameEventBus.OnPlayerTeleported += HandlePlayerTeleported;
        }

        private void OnDisable()
        {
            GameEventBus.OnPlayerTeleported -= HandlePlayerTeleported;
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        public void SetCharacterId(string id)
        {
            characterId = id;
        }

        public void SpeakGuidingHint()
        {
            if (characterId == GameConstants.CharacterIds.Barni)
            {
                GameEventBus.RaiseCompanionSpeak(characterId, "Гав-гав! (Барни радостно виляет хвостом и смотрит в сторону подсказки.)");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayVoiceCue(characterId);
                }
                return;
            }

            if (characterId == GameConstants.CharacterIds.Zhulik)
            {
                GameEventBus.RaiseCompanionSpeak(characterId, "Мяу-рр... (Кот Жулик щурится на солнце и мягко шевелит ушами.)");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayVoiceCue(characterId);
                }
                return;
            }

            if (guidingHints == null || guidingHints.Length == 0)
            {
                return;
            }

            string hint = guidingHints[_hintIndex % guidingHints.Length];
            _hintIndex++;
            GameEventBus.RaiseCompanionSpeak(characterId, hint);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVoiceCue(characterId);
            }
        }

        private void HandlePlayerTeleported(Vector3 targetPos, float targetYaw)
        {
            Vector3 offset = targetPos - Vector3.forward * 1.4f + Vector3.right * 1.1f;
            if (_agent != null && _agent.enabled && NavMesh.SamplePosition(offset, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
            else
            {
                transform.position = offset;
            }
        }

        private void Update()
        {
            if (followTarget == null || _agent == null || !_agent.enabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _repathTimer += Time.deltaTime;
            if (_repathTimer < repathInterval)
            {
                return;
            }
            _repathTimer = 0f;

            Vector3 diff = followTarget.position - transform.position;
            float sqrDist = diff.sqrMagnitude;

            if (sqrDist > teleportCatchupDistance * teleportCatchupDistance)
            {
                Vector3 nearPlayer = followTarget.position - followTarget.forward * desiredFollowDistance;
                if (NavMesh.SamplePosition(nearPlayer, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
                return;
            }

            if (sqrDist > desiredFollowDistance * desiredFollowDistance)
            {
                _agent.SetDestination(followTarget.position);
            }
        }
    }
}
