using Game.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.EmoteSystem
{
    [CreateAssetMenu(fileName = "New Emote Library", menuName = "Game/Emote System/Emote Library")]
    public class EmoteLibrary : ScriptableObject
    {
        [SerializeField] private EmoteEntry[] _emoteEntries;

        private Dictionary<EmoteType, EmoteEntry> _emoteEntryLookup;

        public EmoteEntry GetEmoteEntry(EmoteType emoteType)
        {
            if (_emoteEntries == null || _emoteEntries.Length == 0)
            {
                LogX.LogError("EmoteLibrary has no EmoteEntries defined!", this);

                return null;
            }

            if (_emoteEntryLookup == null)
            {
                _emoteEntryLookup = new Dictionary<EmoteType, EmoteEntry>();
                for (int i = 0; i < _emoteEntries.Length; i++)
                {
                    _emoteEntryLookup[_emoteEntries[i].EmoteType] = _emoteEntries[i];
                }
            }

            if (_emoteEntryLookup.TryGetValue(emoteType, out EmoteEntry entry))
            {
                return entry;
            }

            return null;
        }

        [Serializable]
        public class EmoteEntry
        {
            [SerializeField] private string _emoteName;
            [SerializeField] private EmoteType _emoteType;
            [SerializeField] private Sprite _emoteSprite;

            public string EmoteName => _emoteName;
            public EmoteType EmoteType => _emoteType;
            public Sprite EmoteSprite => _emoteSprite;
        }
    }

    public enum EmoteType
    {
        Alert,
        Anger,
        Bars,
        Cash,
        Circle,
        Cloud,
        Cross,
        Dots1,
        Dots2,
        Dots3,
        Drop,
        Drops,
        Exclamation,
        Exclamations,
        FaceAngry,
        FaceHappy,
        FaceSad,
        Heart,
        HeartBroken,
        Hearts,
        Idea,
        Laugh,
        Music,
        Question,
        Sleep,
        Sleeps,
        Star,
        Stars,
        Swirl,
        Blank
    }
}
