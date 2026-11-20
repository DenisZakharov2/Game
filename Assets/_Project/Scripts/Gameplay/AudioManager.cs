using System.Collections.Generic;
using UnityEngine;
using TimeMachine.Core;

namespace TimeMachine.Gameplay
{
    public enum SurfaceType
    {
        Asphalt = 0,
        Gravel = 1,
        ConcreteStairs = 2,
        WoodFloor = 3
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Pool & Ducking Settings")]
        [SerializeField] private int sfxPoolSize = 10;
        [SerializeField] private float taskDuckingDb = -18f;
        [SerializeField] private float duckingLerpSpeed = 8f;

        private AudioSource _musicPrimary;
        private AudioSource _musicSecondary;
        private AudioSource[] _sfxPool;
        private int _sfxCursor;

        private readonly Dictionary<string, AudioClip> _loadedClips = new Dictionary<string, AudioClip>(96);
        private readonly Dictionary<string, string> _eventToClipName = new Dictionary<string, string>(96);

        private float _userMusicVolume = 0.8f;
        private float _userSfxVolume = 0.9f;
        private float _currentDuckMultiplier = 1f;
        private float _targetDuckMultiplier = 1f;
        private float _duckLinearFactor;

        public float UserMusicVolume => _userMusicVolume;
        public float UserSfxVolume => _userSfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _duckLinearFactor = Mathf.Pow(10f, taskDuckingDb / 20f); // -18 dB -> ~0.1259f

            _musicPrimary = gameObject.AddComponent<AudioSource>();
            _musicPrimary.loop = true;
            _musicPrimary.playOnAwake = false;

            _musicSecondary = gameObject.AddComponent<AudioSource>();
            _musicSecondary.loop = true;
            _musicSecondary.playOnAwake = false;

            _sfxPool = new AudioSource[Mathf.Max(4, sfxPoolSize)];
            for (int i = 0; i < _sfxPool.Length; i++)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                _sfxPool[i] = src;
            }

            LoadShotlistCsv();
        }

        private void LoadShotlistCsv()
        {
            TextAsset csv = Resources.Load<TextAsset>("Audio/audio_shotlist");
            if (csv == null || string.IsNullOrEmpty(csv.text))
            {
                return;
            }

            string[] lines = csv.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] cols = line.Split(';');
                if (cols.Length >= 3)
                {
                    string eventId = cols[0].Trim();
                    string fileName = cols[2].Trim();
                    int dotIndex = fileName.LastIndexOf('.');
                    string clipResourceName = dotIndex > 0 ? fileName.Substring(0, dotIndex) : fileName;
                    _eventToClipName[eventId] = clipResourceName;
                }
            }
        }

        public void SetTaskWindowOpen(bool isOpen)
        {
            _targetDuckMultiplier = isOpen ? _duckLinearFactor : 1f;
        }

        public void SetMusicVolume(float volume)
        {
            _userMusicVolume = Mathf.Clamp01(volume);
            ApplyMusicVolume();
        }

        public void SetSfxVolume(float volume)
        {
            _userSfxVolume = Mathf.Clamp01(volume);
        }

        public void PlayMusic(string musicEventId, float crossfadeDuration = 1.0f)
        {
            AudioClip clip = ResolveClip(musicEventId);
            if (clip == null || (_musicPrimary.isPlaying && _musicPrimary.clip == clip))
            {
                return;
            }

            _musicPrimary.clip = clip;
            _musicPrimary.volume = _userMusicVolume * _currentDuckMultiplier;
            _musicPrimary.Play();
        }

        public void PlaySfx(string sfxEventId, float pitchVariation = 0f)
        {
            if (_userSfxVolume <= 0.001f)
            {
                return;
            }

            AudioClip clip = ResolveClip(sfxEventId);
            if (clip == null)
            {
                return;
            }

            AudioSource src = _sfxPool[_sfxCursor];
            _sfxCursor = (_sfxCursor + 1) % _sfxPool.Length;
            src.pitch = pitchVariation > 0f ? Random.Range(1f - pitchVariation, 1f + pitchVariation) : 1f;
            src.volume = _userSfxVolume;
            src.PlayOneShot(clip);
        }

        public void PlayVoiceCue(string speakerId)
        {
            switch (speakerId)
            {
                case GameConstants.CharacterIds.Sonya2027:
                    PlaySfx("SFX-23", 0.03f);
                    break;
                case GameConstants.CharacterIds.Sonya2040:
                    PlaySfx("SFX-24", 0.02f);
                    break;
                case GameConstants.CharacterIds.Artur:
                    PlaySfx("SFX-25", 0.02f);
                    break;
                case GameConstants.CharacterIds.Babushka:
                    PlaySfx("SFX-26", 0.02f);
                    break;
                case GameConstants.CharacterIds.Barni:
                    PlaySfx("SFX-19", 0.05f);
                    break;
                case GameConstants.CharacterIds.Zhulik:
                    PlaySfx("SFX-21", 0.04f);
                    break;
                default:
                    PlaySfx("SFX-22", 0.03f);
                    break;
            }
        }

        public void PlayFootstep(SurfaceType surface)
        {
            switch (surface)
            {
                case SurfaceType.Gravel:
                    PlaySfx("SFX-14", 0.06f);
                    break;
                case SurfaceType.ConcreteStairs:
                    PlaySfx("SFX-15", 0.05f);
                    break;
                case SurfaceType.WoodFloor:
                    PlaySfx("SFX-16", 0.05f);
                    break;
                default:
                    PlaySfx("SFX-13", 0.06f);
                    break;
            }
        }

        private AudioClip ResolveClip(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return null;
            }
            if (_loadedClips.TryGetValue(eventId, out AudioClip cached))
            {
                return cached;
            }

            string resourceName = _eventToClipName.TryGetValue(eventId, out string mapped) ? mapped : eventId;
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + resourceName);
            _loadedClips[eventId] = clip;
            return clip;
        }

        private void Update()
        {
            if (!Mathf.Approximately(_currentDuckMultiplier, _targetDuckMultiplier))
            {
                _currentDuckMultiplier = Mathf.MoveTowards(
                    _currentDuckMultiplier,
                    _targetDuckMultiplier,
                    duckingLerpSpeed * Time.unscaledDeltaTime);
                ApplyMusicVolume();
            }
        }

        private void ApplyMusicVolume()
        {
            if (_musicPrimary != null)
            {
                _musicPrimary.volume = _userMusicVolume * _currentDuckMultiplier;
            }
        }
    }
}
