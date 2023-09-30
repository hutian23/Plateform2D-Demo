using System;
using UnityEngine;

namespace ET.Client.Platform
{
    [ExecuteInEditMode,ImageEffectAllowedInSceneView]
    public class PostEffectsController : MonoBehaviour
    {
        public Shader postShader;
        public Material postEffectMaterial;

        private RenderTexture postRenderTexture;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (postEffectMaterial == null)
            {
                postEffectMaterial = new Material(postShader);
            }
            if (postRenderTexture == null)
            {
                postRenderTexture = new RenderTexture(source.width, source.height, 0, source.format);
            }

            Graphics.Blit(source, postRenderTexture, postEffectMaterial, 0);
            Shader.SetGlobalTexture
                ("_GlobalRenderTexture", postRenderTexture);
            Graphics.Blit(source, destination);
        }
    }
}