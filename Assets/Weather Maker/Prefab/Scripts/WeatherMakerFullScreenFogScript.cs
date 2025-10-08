//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;

using System;
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    public enum WeatherMakerFogMode
    {
        None,
        Constant,
        Linear,
        Exponential,
        ExponentialSquared
    }

    public enum BlurShaderType
    {
        None,
        GaussianBlur7,
        GaussianBlur17
    }

    public class WeatherMakerFullScreenFogScript : WeatherMakerFogScript
    {
        [Header("Full Screen Fog")]
        [Tooltip("Material to render the fog full screen after it has been calculated")]
        public Material FogFullScreenMaterial;

        [Tooltip("Fog height. Set to 0 for unlimited height.")]
        [Range(0.0f, 1000.0f)]
        public float FogHeight = 0.0f;

        [Tooltip("Use sun for calculations in the sky.")]
        public bool SunEnabled = true;

        [Tooltip("Depth buffer less than far plane multiplied by this value will occlude the sun light through the fog.")]
        [Range(0.0f, 1.0f)]
        public float FarPlaneSunThreshold = 0.75f;

        [Tooltip("Render fog in this render queue for the command buffer.")]
        public CameraEvent FogRenderQueue = CameraEvent.AfterForwardAlpha;

        private CommandBuffer commandBuffer;

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            if (FogHeight > 0.0f)
            {
                FogMaterial.SetFloat("_FogHeight", FogHeight);
                if (FogNoiseHeightScale > 0.0f && FogNoiseHeight != null)
                {
                    FogMaterial.SetTexture("_FogNoiseHeight", FogNoiseHeight);
                    FogMaterial.EnableKeyword("ENABLE_FOG_HEIGHT_WITH_NOISE");
                    FogMaterial.DisableKeyword("ENABLE_FOG_HEIGHT");
                }
                else
                {
                    FogMaterial.EnableKeyword("ENABLE_FOG_HEIGHT");
                    FogMaterial.DisableKeyword("ENABLE_FOG_HEIGHT_WITH_NOISE");
                }
            }
            else
            {
                FogMaterial.DisableKeyword("ENABLE_FOG_HEIGHT");
                FogMaterial.DisableKeyword("ENABLE_FOG_HEIGHT_WITH_NOISE");
            }

            if (Sun == null)
            {
                FogMaterial.DisableKeyword("ENABLE_SUN");
            }
            else
            {
                if (Sun.intensity == 0.0f || !SunEnabled || (Sun.color.r == 0.0f && Sun.color.g == 0.0f && Sun.color.b == 0.0f))
                {
                    FogMaterial.DisableKeyword("ENABLE_SUN");
                }
                else
                {
                    FogMaterial.EnableKeyword("ENABLE_SUN");
                }
            }
        }

        private void CreateCommandBuffer()
        {
            RemoveCommandBuffer();
            commandBuffer = new CommandBuffer { name = "WeatherMakerFullScreenFogScript" };
            Camera.AddCommandBuffer(FogRenderQueue, commandBuffer);
            lastDownSampleScale = DownSampleScale;
            lastBlurShader = BlurShader;

            float scale = Mathf.Clamp(DownSampleScale, 0.25f, 1.0f);
            if (scale < 0.99f)
            {
                // scale is less than 1, create scaled down textures for depth and fog
                int width = (int)(Screen.width * scale);
                int height = (int)(Screen.height * scale);

                // render depth buffer to low res texture
                int depthRenderTargetId = Shader.PropertyToID("_CameraDepthTextureScaled");
                RenderTargetIdentifier downDepth = new RenderTargetIdentifier(depthRenderTargetId);
                commandBuffer.GetTemporaryRT(depthRenderTargetId, width, height, 0, FilterMode.Point, RenderTextureFormat.RFloat);
                commandBuffer.Blit((Texture2D)null, downDepth, FogDepthSampleMaterial);

                // render fog to low res texture, disable alpha blend
                int fogRenderTargetId = Shader.PropertyToID("_MainTex");
                RenderTargetIdentifier downScaledFog = new RenderTargetIdentifier(fogRenderTargetId);
                commandBuffer.GetTemporaryRT(fogRenderTargetId, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                FogMaterial.SetInt("_SrcBlendMode", (int)BlendMode.One);
                FogMaterial.SetInt("_DstBlendMode", (int)BlendMode.Zero);
                commandBuffer.Blit((Texture2D)null, downScaledFog, FogMaterial);

                // blur fog texture if scaled down
                if (FogBlurMaterial != null && BlurShader != BlurShaderType.None)
                {
                    // render fog on top of camera using alpha blend + blur
                    commandBuffer.Blit(downScaledFog, BuiltinRenderTextureType.CameraTarget, FogBlurMaterial);
                }
                else
                {
                    // render fog on top of camera using alpha blend
                    commandBuffer.Blit(downScaledFog, BuiltinRenderTextureType.CameraTarget, FogFullScreenMaterial);
                }

                // cleanup
                commandBuffer.ReleaseTemporaryRT(depthRenderTargetId);
                //commandBuffer.ReleaseTemporaryRT(depthNormalsRenderTargetId);
                commandBuffer.ReleaseTemporaryRT(fogRenderTargetId);
            }
            else if (FogBlurMaterial != null && BlurShader != BlurShaderType.None)
            {
                // render fog into render texture, then blur that to final result

                // create fog render target, draw fog to that
                int fogRenderTargetId = Shader.PropertyToID("_MainTex");
                RenderTargetIdentifier fogRenderTarget = new RenderTargetIdentifier(fogRenderTargetId);
                commandBuffer.GetTemporaryRT(fogRenderTargetId, Screen.width, Screen.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                FogMaterial.SetInt("_SrcBlendMode", (int)BlendMode.One);
                FogMaterial.SetInt("_DstBlendMode", (int)BlendMode.Zero);

                // draw fog into fog render texture at full scale
                commandBuffer.Blit((Texture2D)null, fogRenderTarget, FogMaterial);

                // render final result with alpha blend + blur on top of camera texture
                commandBuffer.Blit(fogRenderTarget, BuiltinRenderTextureType.CameraTarget, FogBlurMaterial);

                // cleanup
                commandBuffer.ReleaseTemporaryRT(fogRenderTargetId);
            }
            else
            {
                // render final image to camera target using transparent overlay
                FogMaterial.SetInt("_SrcBlendMode", (int)BlendMode.SrcAlpha);
                FogMaterial.SetInt("_DstBlendMode", (int)BlendMode.OneMinusSrcAlpha);
                commandBuffer.Blit((Texture2D)null, BuiltinRenderTextureType.CameraTarget, FogMaterial);
            }
        }

        private void RemoveCommandBuffer()
        {
            if (commandBuffer != null && Camera != null)
            {
                Camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
                commandBuffer.Release();
                commandBuffer = null;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (DownSampleScale != lastDownSampleScale || BlurShader != lastBlurShader)
            {
                CreateCommandBuffer();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveCommandBuffer();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CreateCommandBuffer();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            RemoveCommandBuffer();
        }
    }
}