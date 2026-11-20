using System;
using UnityEngine;
using TimeMachine.Core;
using TimeMachine.Data;

namespace TimeMachine.Gameplay
{
    public enum EraLayerId
    {
        Era2027 = 2027,
        Era1950 = 1950
    }

    [Serializable]
    public struct EraAnchorPair
    {
        public string locationId;
        public string observation1950;
        public string observation2027;
        public Transform anchorTransform;
    }

    public class EraSwitcher : MonoBehaviour
    {
        [Header("Scene Era Containers (Scene_Okhta_DualEra)")]
        [SerializeField] private GameObject layerAnchors;
        [SerializeField] private GameObject layer2027;
        [SerializeField] private GameObject layer1950;
        [SerializeField] private EraAnchorPair[] anchorPairs;

        private GameState _gameState;
        private InventorySystem _inventory;
        private SaveSystem _saveSystem;
        private EraLayerId _currentEra = EraLayerId.Era2027;

        public event Action<EraLayerId> OnEraChanged;

        public EraLayerId CurrentEra => _currentEra;
        public GameObject LayerAnchors => layerAnchors;
        public GameObject Layer2027 => layer2027;
        public GameObject Layer1950 => layer1950;

        public void ConfigureContainers(GameObject anchors, GameObject era2027Obj, GameObject era1950Obj)
        {
            layerAnchors = anchors;
            layer2027 = era2027Obj;
            layer1950 = era1950Obj;
            ApplyEraImmediate(_currentEra);
        }

        public void BindSystems(GameState state, InventorySystem inventory, SaveSystem saveSystem)
        {
            _gameState = state;
            _inventory = inventory;
            _saveSystem = saveSystem;
            if (_gameState != null && _gameState.Data != null && _gameState.Data.currentEra == "1950")
            {
                ApplyEraImmediate(EraLayerId.Era1950);
            }
            else
            {
                ApplyEraImmediate(EraLayerId.Era2027);
            }
        }

        public bool CanJumpTo1950()
        {
            if (_gameState != null && _gameState.Data != null && _gameState.Data.isZarya7Assembled)
            {
                return true;
            }
            if (_inventory != null && _inventory.HasItem(GameConstants.ItemIds.Zarya7Assembled))
            {
                return true;
            }
            return false;
        }

        public bool TrySwitchEra(EraLayerId targetEra)
        {
            if (targetEra == EraLayerId.Era1950 && !CanJumpTo1950())
            {
                GameEventBus.RaiseCompanionSpeak(
                    GameConstants.CharacterIds.Sonya2027,
                    "Сначала нужно собрать все 5 узлов «Зари-7» на верстаке у горки!");
                return false;
            }

            ApplyEraImmediate(targetEra);

            if (_gameState != null && _gameState.Data != null)
            {
                _gameState.Data.currentEra = targetEra == EraLayerId.Era1950 ? "1950" : "2027";
                if (_saveSystem != null)
                {
                    _saveSystem.Save(_gameState.Data);
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(targetEra == EraLayerId.Era1950 ? "MUS-04" : "MUS-02", 1.2f);
            }

            OnEraChanged?.Invoke(targetEra);
            return true;
        }

        public void ApplyEraImmediate(EraLayerId targetEra)
        {
            _currentEra = targetEra;
            if (layerAnchors != null && !layerAnchors.activeSelf)
            {
                layerAnchors.SetActive(true);
            }
            if (layer2027 != null)
            {
                layer2027.SetActive(targetEra == EraLayerId.Era2027);
            }
            if (layer1950 != null)
            {
                layer1950.SetActive(targetEra == EraLayerId.Era1950);
            }
        }
    }
}
