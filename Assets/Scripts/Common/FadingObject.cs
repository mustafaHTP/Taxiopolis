using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    public class FadingObject : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private List<Material> _materials;

        public bool CanFade { get; private set; }
        public float InitialAlpha { get; private set; }
        public List<Material> Materials => _materials;

        public void SetSurfaceTransparent()
        {
            foreach (var mat in Materials)
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3000;

                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                mat.SetInt("_ZWrite", 0);

                // Render face = Both
                mat.SetInt("_Cull", 0);

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
            }
        }

        public void SetSurfaceOpaque()
        {
            foreach (var mat in Materials)
            {
                mat.SetFloat("_Surface", 0); // Opaque
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.renderQueue = 2000;

                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);

                mat.SetInt("_ZWrite", 1);

                mat.SetInt("_Cull", 2);

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }

        private void Awake()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>();
            }

            if (_materials == null || _materials.Count == 0)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    var currentRenderer = _renderers[i];
                    for (int j = 0; j < currentRenderer.materials.Length; ++j)
                    {
                        var currentMaterial = currentRenderer.materials[j];
                        if (currentMaterial.HasProperty("_Color"))
                        {
                            _materials.Add(currentMaterial);
                        }
                    }
                }

                CanFade = _materials.Count > 0;
            }

            if(_materials.Count > 0)
            {
                InitialAlpha = _materials[0].color.a;
            }
        }
    }
}
