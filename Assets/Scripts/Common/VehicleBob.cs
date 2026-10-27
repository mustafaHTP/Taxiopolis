using DG.Tweening;
using UnityEngine;

namespace Game.Common
{
    public class VehicleBob : MonoBehaviour
    {
        [SerializeField] private Transform _body;
        [SerializeField] private float _bobHeight;
        [SerializeField] private float _bobDuration;
        [SerializeField] private Ease _bobEase;

        private void Start()
        {
            _body
                .DOLocalMoveY(_body.localPosition.y + _bobHeight, _bobDuration)
                .SetEase(_bobEase)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
