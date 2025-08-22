using UnityEngine;
using PuzzleGame.Core.Helpers;

namespace PuzzleGame.Gameplay.Managers
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("UI Sounds")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip levelCompleteSound;
        [SerializeField] private AudioClip levelFailSound;

        [Header("Cube Sounds")]
        [SerializeField] private AudioClip[] cubeClickSounds;
        [SerializeField] private AudioClip cubeMoveSound;
        [SerializeField] private AudioClip cubeBlockedSound;

        [Header("Collision Sounds")]
        [SerializeField] private AudioClip collisionSound;
        [SerializeField] private AudioClip[] errorSounds;

        [Header("Volume Settings")]
        [Range(0f, 1f)] [SerializeField] private float uiVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float cubeVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float collisionVolume = 0.8f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // UI Sounds
        public void PlayButtonClick()
        {
            Debug.Log("PlayButtonClick çağrıldı");
            PlaySound(buttonClickSound, uiVolume);
        }

        public void PlayLevelComplete()
        {
            Debug.Log("PlayLevelComplete çağrıldı");
            PlaySound(levelCompleteSound, uiVolume);
        }

        public void PlayLevelFail()
        {
            Debug.Log("PlayLevelFail çağrıldı");
            PlaySound(levelFailSound, uiVolume);
        }

        // Cube Sounds
        public void PlayCubeClick()
        {
            Debug.Log($"PlayCubeClick çağrıldı - Clip sayısı: {cubeClickSounds?.Length}");
            if (cubeClickSounds != null && cubeClickSounds.Length > 0)
            {
                AudioClip randomClip = cubeClickSounds[Random.Range(0, cubeClickSounds.Length)];
                PlaySound(randomClip, cubeVolume);
            }
        }

        public void PlayCubeMove()
        {
            Debug.Log("PlayCubeMove çağrıldı");
            PlaySound(cubeMoveSound, cubeVolume);
        }

        public void PlayCubeBlocked()
        {
            Debug.Log("PlayCubeBlocked çağrıldı");
            PlaySound(cubeBlockedSound, cubeVolume);
        }

        // Collision Sounds
        public void PlayCollision()
        {
            Debug.Log("PlayCollision çağrıldı");
            if (collisionSound != null)
            {
                PlaySound(collisionSound, collisionVolume);
            }
            else if (errorSounds != null && errorSounds.Length > 0)
            {
                AudioClip randomErrorSound = errorSounds[Random.Range(0, errorSounds.Length)];
                PlaySound(randomErrorSound, collisionVolume);
            }
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (audioSource != null && clip != null)
            {
                Debug.Log($"Ses çalınıyor: {clip.name} - Volume: {volume}");
                audioSource.PlayOneShot(clip, volume);
            }
            else
            {
                Debug.LogWarning($"AudioSource: {audioSource != null}, Clip: {clip != null}");
            }
        }
    }
}