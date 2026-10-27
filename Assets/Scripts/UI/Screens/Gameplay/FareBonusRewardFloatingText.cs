using TMPro;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class FareBonusRewardFloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        [Header("Motion (pixels / second)")]
        [SerializeField] private Vector2 _horizontalSpeed = new Vector2(-120f, 120f);
        [SerializeField] private Vector2 _verticalSpeed = new Vector2(250f, 400f);
        [SerializeField] private float _gravity = -600f;
        [SerializeField] private float _lifetime = 1.2f;

        private Vector2 _velocity;
        private float _scale;
        private float _time;

        public void Play(int reward, Vector3 screenPos, float canvasScale)
        {
            _scale = canvasScale; // keeps motion consistent with Canvas Scaler
            _text.SetText("+{0}", reward);
            transform.position = screenPos;

            _velocity = new Vector2(
                Random.Range(_horizontalSpeed.x, _horizontalSpeed.y),
                Random.Range(_verticalSpeed.x, _verticalSpeed.y)) * _scale;

            _time = 0f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _time += dt;
            float n = _time / _lifetime;

            if (n >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            //* claude that part so idk what is going on
            // Arc: move, then pull down with gravity
            transform.position += (Vector3)(_velocity * dt);
            _velocity.y += _gravity * _scale * dt;

            // Pop scale: 0.5 -> 1.2 -> 1.0 in the first 30%
            float pop = n < 0.15f
                ? Mathf.Lerp(0.5f, 1.2f, n / 0.15f)
                : Mathf.Lerp(1.2f, 1f, Mathf.Clamp01((n - 0.15f) / 0.15f));
            transform.localScale = Vector3.one * pop;

            // Fade out over the last 40%
            _text.alpha = n < 0.6f ? 1f : 1f - (n - 0.6f) / 0.4f;
        }
    }
}