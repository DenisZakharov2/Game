using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeMachine.Gameplay
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Settings")]
        [Range(0f, 1f)] [SerializeField] private float defaultMusicVolume = 0.15f; // Тихая фоновая музыка
        [Range(0f, 1f)] [SerializeField] private float duckedMusicVolume = 0.04f;  // Приглушение (-18 дБ) при решении задач
        [SerializeField] private float fadeDuration = 0.8f;

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitSources()
        {
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.volume = defaultMusicVolume;
            musicSource.playOnAwake = false;
            sfxSource.playOnAwake = false;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.volume = defaultMusicVolume;
            musicSource.Play();
        }

        public void SetTaskDucking(bool isTaskActive)
        {
            float targetVolume = isTaskActive ? duckedMusicVolume : defaultMusicVolume;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeVolume(targetVolume));
        }

        private IEnumerator FadeVolume(float targetVolume)
        {
            float startVolume = musicSource.volume;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, targetVolume, time / fadeDuration);
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null) sfxSource.PlayOneShot(clip);
        }
    }
}
