using System;
using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Data;

namespace TimeMachine.Gameplay
{
    public class DualReturnSynchronizer : MonoBehaviour
    {
        [Header("Dual Return Requirements (Chapter 5 -> Chapter 6)")]
        [SerializeField] private EraSwitcher eraSwitcher;
        [SerializeField] private Transform sonya2027Transform;
        [SerializeField] private Transform sonya2040Transform;
        [SerializeField] private Transform bridgeAnchor1950;
        [SerializeField] private Vector3 returnSpawn2027 = new Vector3(142f, 0.1f, 41f);
        [SerializeField] private float dualCaptureRadius = 2.5f;

        private InventorySystem _inventory;
        private bool _beaconSignalReady = true;

        public event Action OnDualReturnCompleted;

        public bool IsBeaconSignalReady
        {
            get => _beaconSignalReady;
            set => _beaconSignalReady = value;
        }

        public void BindSystems(EraSwitcher switcher, InventorySystem inventory)
        {
            eraSwitcher = switcher;
            _inventory = inventory;
        }

        public void ConfigureTransforms(Transform sonya2027, Transform sonya2040, Transform bridgeAnchor)
        {
            sonya2027Transform = sonya2027;
            sonya2040Transform = sonya2040;
            bridgeAnchor1950 = bridgeAnchor;
        }

        public bool CheckDualReturnReadiness(out string statusReason)
        {
            if (_inventory != null &&
                !_inventory.HasItem(GameConstants.ItemIds.Zarya7DualReady) &&
                !_inventory.HasItem(GameConstants.ItemIds.Zarya7Assembled))
            {
                statusReason = "Сначала установим усиленный медный контур и экран на «Зарю-7».";
                return false;
            }

            if (!_beaconSignalReady)
            {
                statusReason = "Маячок связи на «Компасе поколений» из 2027 года ещё калибруется.";
                return false;
            }

            if (sonya2027Transform != null && sonya2040Transform != null && bridgeAnchor1950 != null)
            {
                float rSqr = dualCaptureRadius * dualCaptureRadius;
                bool s27Near = (sonya2027Transform.position - bridgeAnchor1950.position).sqrMagnitude <= rSqr;
                bool s40Near = (sonya2040Transform.position - bridgeAnchor1950.position).sqrMagnitude <= rSqr;
                if (!s27Near || !s40Near)
                {
                    statusReason = "Обе Сони должны встать рядом у гранитного устоя моста через Охту.";
                    return false;
                }
            }

            statusReason = "Парный контур «Зари-7» и маячок друзей синхронизированы!";
            return true;
        }

        public bool ExecuteDualReturn()
        {
            if (!CheckDualReturnReadiness(out string reason))
            {
                GameEventBus.RaiseCompanionSpeak(GameConstants.CharacterIds.Sonya2040, reason);
                return false;
            }

            if (eraSwitcher != null)
            {
                eraSwitcher.TrySwitchEra(EraLayerId.Era2027);
            }

            GameEventBus.RaisePlayerTeleported(returnSpawn2027, 270f);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("SFX-56");
            }
            OnDualReturnCompleted?.Invoke();
            return true;
        }
    }
}
