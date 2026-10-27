using Framework.Services;
using Game.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Systems.SoundSystem
{
    public class MusicManager : MonoBehaviour, IGlobalService
    {
        [SerializeField] private AudioSource _playlistAudioSourcePrefab;
        [SerializeField] private AudioSource _musicAudioSourcePrefab;
        [SerializeField] private List<SoundData> _playlist = new();
        private int _currentTrackOnPlaylistIndex = 0;

        private AudioSource _mainAudioSource;
        private AudioSource _playlistAudioSource;
        private Coroutine _playlistCoroutine;

        public void AddSoundToPlayList(SoundData soundData)
        {
            if (soundData == null)
            {
                LogX.LogWarning("SoundData is null.", this);

                return;
            }

            _playlist.Add(soundData);
        }

        public void AddSoundsToPlaylist(IEnumerable<SoundData> soundDataCollection)
        {
            if (soundDataCollection == null)
            {
                LogX.LogWarning("SoundData collection is null.", this);
                return;
            }

            _playlist.AddRange(soundDataCollection);
        }

        public void AddSoundsRandomToPlaylist(IList<SoundData> soundDataCollection)
        {
            List<SoundData> remainSounds = new(soundDataCollection);
            for (int i = 0; i < soundDataCollection.Count; i++)
            {
                int randomSoundIndex = Random.Range(0, remainSounds.Count);

                _playlist.Add(remainSounds[randomSoundIndex]);

                remainSounds.RemoveAt(randomSoundIndex);
            }
        }

        public void ClearPlaylist()
        {
            StopPlaylist();

            _playlist.Clear();
            _currentTrackOnPlaylistIndex = 0;
        }

        public void StartPlaylist()
        {
            if (_playlistAudioSource == null)
            {
                _playlistAudioSource = CreatePlaylistAudioSource();
            }

            if (_playlist.Count == 0)
            {
                LogX.LogWarning("Playlist is empty.", this);

                return;
            }

            StopPlaylist();

            _playlistCoroutine = StartCoroutine(StartPlaylistCoroutine());
        }

        public void NextTrackOnPlaylist()
        {
            if (_playlist.Count == 0)
            {
                LogX.LogWarning("Playlist is empty.", this);
                return;
            }

            if (_playlistCoroutine != null)
            {
                StopCoroutine(_playlistCoroutine);
                _playlistCoroutine = null;
            }

            _currentTrackOnPlaylistIndex = (_currentTrackOnPlaylistIndex + 1) % _playlist.Count;

            StartPlaylist();
        }

        public void StopPlaylist()
        {
            if (_playlistCoroutine != null)
            {
                StopCoroutine(_playlistCoroutine);
                _playlistCoroutine = null;
            }

            if (_playlistAudioSource != null)
            {
                _playlistAudioSource.Stop();
            }
        }

        private IEnumerator StartPlaylistCoroutine()
        {
            while (true)
            {
                var currentSoundData = _playlist[_currentTrackOnPlaylistIndex];

                PlayPlaylistSound(currentSoundData);

                yield return new WaitForSeconds(currentSoundData.Clip.length);

                _currentTrackOnPlaylistIndex = (_currentTrackOnPlaylistIndex + 1) % _playlist.Count;
            }
        }

        private void PlayPlaylistSound(SoundData soundData)
        {
            if (_playlistAudioSource == null)
            {
                _playlistAudioSource = CreatePlaylistAudioSource();
            }

            if (soundData == null || soundData.Clip == null)
            {
                LogX.LogWarning("SoundData or AudioClip is null.");

                return;
            }

            if (soundData.AudioMixerGroup != null)
            {
                _playlistAudioSource.outputAudioMixerGroup = soundData.AudioMixerGroup;
            }

            _playlistAudioSource.clip = soundData.Clip;
            _playlistAudioSource.volume = soundData.Volume;
            _playlistAudioSource.Play();
        }

        public void PlayMusic(SoundData soundData)
        {
            if (_mainAudioSource == null)
            {
                _mainAudioSource = CreateMainAudioSource();
            }

            if (soundData == null || soundData.Clip == null)
            {
                LogX.LogWarning("SoundData or AudioClip is null.");

                return;
            }

            if (soundData.AudioMixerGroup != null)
            {
                _mainAudioSource.outputAudioMixerGroup = soundData.AudioMixerGroup;
            }

            _mainAudioSource.clip = soundData.Clip;
            _mainAudioSource.volume = soundData.Volume;
            _mainAudioSource.Play();
        }

        public void StopMusic()
        {
            if (_mainAudioSource == null)
            {
                LogX.LogWarning("No music is currently playing.", this);

                return;
            }

            _mainAudioSource.Stop();
        }

        private AudioSource CreateMainAudioSource()
        {
            return Instantiate(_musicAudioSourcePrefab, transform);
        }

        private AudioSource CreatePlaylistAudioSource()
        {
            return Instantiate(_playlistAudioSourcePrefab, transform);
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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MusicManager))]
    public class MusicManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MusicManager musicManager = (MusicManager)target;
            if (GUILayout.Button("Start Playlist"))
            {
                musicManager.StartPlaylist();
            }
            if (GUILayout.Button("Next Track on Playlist"))
            {
                musicManager.NextTrackOnPlaylist();
            }
            if (GUILayout.Button("Stop Playlist"))
            {
                musicManager.StopPlaylist();
            }
        }
    }
#endif
}
