using Framework.Services;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.SoundSystem
{
    public class SoundManager : MonoBehaviour, IGlobalService
    {
        [Header("Prefabs")]
        [SerializeField] private AudioSource _audioSourcePrefab;

        private AudioSource _mainAudioSource;

        public enum OnFinished
        {
            None,
            Destroy
        }

        public void Init()
        {
            // noop
        }

        public void AfterInit()
        {
            // noop
        }

        public void Dispose()
        {
            // noop
        }

        public AudioSource PlaySound(SoundData soundData, Transform parent = null, OnFinished onFinished = OnFinished.Destroy)
        {
            if (soundData == null || soundData.Clip == null)
            {
                LogX.LogWarning("SoundData or AudioClip is null.");

                return null;
            }

            AudioSource audioSource = Instantiate(_audioSourcePrefab, transform);

            if (parent != null)
            {
                audioSource.transform.SetParent(parent);
            }
            audioSource.transform.localPosition = Vector3.zero;

            // Configure the AudioSource with the SoundData properties
            audioSource.clip = soundData.Clip;
            audioSource.volume = soundData.Volume;
            audioSource.loop = soundData.IsLoop;

            if (soundData.AudioMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = soundData.AudioMixerGroup;
            }

            audioSource.Play();

            switch (onFinished)
            {
                case OnFinished.None:
                    break;
                case OnFinished.Destroy:
                    Destroy(audioSource.gameObject, soundData.Clip.length);
                    break;
            }

            return audioSource;
        }

        public void PlayOneShot(SoundData soundData)
        {
            if (soundData == null || soundData.Clip == null)
            {
                LogX.LogWarning("SoundData or AudioClip is null.");

                return;
            }

            if (_mainAudioSource == null)
            {
                _mainAudioSource = Instantiate(_audioSourcePrefab, transform);
            }

            _mainAudioSource.PlayOneShot(soundData.Clip, soundData.Volume);
        }
    }
}