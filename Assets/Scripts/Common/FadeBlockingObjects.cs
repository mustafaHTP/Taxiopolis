using Game.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    public class FadeBlockingObjects : MonoBehaviour
    {
        [SerializeField] private LayerMask _blockingLayerMask;
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset;
        [SerializeField] private float _fadedAlpha = 0.5f;
        [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private bool _useFadeCurve = false;
        [SerializeField] private AnimationCurve _fadeCurve;

        private readonly Dictionary<FadingObject, Coroutine> _runningFadingCoroutines = new();
        private readonly List<FadingObject> _fadingObjectsBlockingView = new();

        private RaycastHit[] _hitObjects;

        private void OnDrawGizmos()
        {
            if (_target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(Camera.main.transform.position, _target.position + _targetOffset);
            }
        }

        private void Start()
        {
            StartCoroutine(CheckForObjects());
        }

        private IEnumerator CheckForObjects()
        {
            while (true)
            {
                Vector3 raycastDirection = (_target.position + _targetOffset) - Camera.main.transform.position;
                float raycastDistance = raycastDirection.magnitude;

                _hitObjects =
                    //Physics.RaycastAll(
                    //    Camera.main.transform.position,
                    //    raycastDirection.normalized,
                    //    raycastDistance,
                    //    _blockingLayerMask);
                    Physics.SphereCastAll(
                        Camera.main.transform.position,
                        0.5f, // Sphere radius
                        raycastDirection.normalized,
                        raycastDistance,
                        _blockingLayerMask);

                if (_hitObjects != null && _hitObjects.Length > 0)
                {
                    for (int i = 0; i < _hitObjects.Length; i++)
                    {
                        var hitObject = _hitObjects[i].collider.gameObject;
                        if (hitObject.TryGetComponent(out FadingObject fadingObject) && fadingObject.CanFade)
                        {
                            if (!_fadingObjectsBlockingView.Contains(fadingObject))
                            {
                                if(_runningFadingCoroutines.TryGetValue(fadingObject, out Coroutine fadingCoroutine)
                                    && fadingCoroutine != null)
                                {
                                    StopCoroutine(fadingCoroutine);
                                }

                                _fadingObjectsBlockingView.Add(fadingObject);
                                _runningFadingCoroutines[fadingObject] =
                                    StartCoroutine(FadeObjectOut(fadingObject));

                                LogX.LogDebug($"Started fading out object: {fadingObject.gameObject.name}", this);
                            }
                        }
                    }
                }

                CheckForObjectsNoLongerBeingHit();

                Array.Clear(_hitObjects, 0, _hitObjects.Length);

                yield return null;
            }
        }

        private void CheckForObjectsNoLongerBeingHit()
        {
            for (int i = 0; i < _fadingObjectsBlockingView.Count; i++)
            {
                var fadingObjectBlockingView = _fadingObjectsBlockingView[i];
                bool isObjectStillBeingHit = false;
                for (int j = 0; j < _hitObjects.Length; j++)
                {
                    var hitObject = _hitObjects[j].collider.gameObject;
                    if (hitObject.TryGetComponent(out FadingObject fadingObject))
                    {
                        if (fadingObject == fadingObjectBlockingView)
                        {
                            isObjectStillBeingHit = true;
                            break;
                        }
                    }
                }

                if (!isObjectStillBeingHit)
                {
                    if (_runningFadingCoroutines.TryGetValue(fadingObjectBlockingView, out Coroutine runningCoroutine)
                        && runningCoroutine != null)
                    {
                        StopCoroutine(runningCoroutine);
                    }

                    _runningFadingCoroutines[fadingObjectBlockingView] =
                        StartCoroutine(FadeObjectIn(fadingObjectBlockingView));
                    _fadingObjectsBlockingView.Remove(fadingObjectBlockingView);
                }
            }
        }

        private IEnumerator FadeObjectOut(FadingObject fadingObject)
        {
            fadingObject.SetSurfaceTransparent();

            float elapsedTime = 0f;
            while (elapsedTime < _fadeDuration)
            {
                float normalizedTime = Mathf.Clamp01(elapsedTime / _fadeDuration);
                float curveValue = _fadeCurve.Evaluate(normalizedTime);
                float lerpT = _useFadeCurve ? curveValue : normalizedTime;

                for (int i = 0; i < fadingObject.Materials.Count; i++)
                {
                    fadingObject.Materials[i].color = new Color(
                            fadingObject.Materials[i].color.r,
                            fadingObject.Materials[i].color.g,
                            fadingObject.Materials[i].color.b,
                            Mathf.Lerp(fadingObject.InitialAlpha, _fadedAlpha, lerpT)
                        );
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            for (int i = 0; i < fadingObject.Materials.Count; i++)
            {
                fadingObject.Materials[i].color = new Color(
                        fadingObject.Materials[i].color.r,
                        fadingObject.Materials[i].color.g,
                        fadingObject.Materials[i].color.b,
                        _fadedAlpha
                    );
            }
        }

        private IEnumerator FadeObjectIn(FadingObject fadingObject)
        {
            fadingObject.SetSurfaceOpaque();

            float elapsedTime = 0f;
            while (elapsedTime < _fadeDuration)
            {
                float normalizedTime = Mathf.Clamp01(elapsedTime / _fadeDuration);
                float curveValue = _fadeCurve.Evaluate(normalizedTime);
                float lerpT = _useFadeCurve ? curveValue : normalizedTime;

                for (int i = 0; i < fadingObject.Materials.Count; i++)
                {
                    fadingObject.Materials[i].color = new Color(
                            fadingObject.Materials[i].color.r,
                            fadingObject.Materials[i].color.g,
                            fadingObject.Materials[i].color.b,
                            Mathf.Lerp(_fadedAlpha, fadingObject.InitialAlpha, lerpT)
                        );
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            for (int i = 0; i < fadingObject.Materials.Count; i++)
            {
                fadingObject.Materials[i].color = new Color(
                        fadingObject.Materials[i].color.r,
                        fadingObject.Materials[i].color.g,
                        fadingObject.Materials[i].color.b,
                        fadingObject.InitialAlpha
                    );
            }
        }
    }
}