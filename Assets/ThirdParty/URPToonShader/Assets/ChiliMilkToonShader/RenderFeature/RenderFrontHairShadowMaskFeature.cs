using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class RenderFrontHairShadowMaskFeature : ScriptableRendererFeature
{
    private RenderFrontHairShadowMaskPass renderFrontHairMaskPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderFrontHairMaskPass);
    }

    public override void Create()
    {
        renderFrontHairMaskPass = new RenderFrontHairShadowMaskPass();
        renderFrontHairMaskPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    class RenderFrontHairShadowMaskPass : ScriptableRenderPass
    {
        static readonly int maskId = Shader.PropertyToID("_HairShadowMask");
        static readonly string keyword = "_HAIRSHADOWMASK";
        readonly ShaderTagId maskTag = new ShaderTagId("HairShadowMask");
        RTHandle maskHandle;

        [System.Obsolete]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var desc = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.R16);
            RenderingUtils.ReAllocateHandleIfNeeded(ref maskHandle, desc, FilterMode.Point, name: "_HairShadowMask");
            ConfigureTarget(maskHandle);
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            CoreUtils.SetKeyword(cmd, keyword, true);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            DrawingSettings drawingSettings = CreateDrawingSettings(maskTag, ref renderingData, SortingCriteria.CommonOpaque);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.DisableShaderKeyword(keyword);
        }

        public void Dispose()
        {
            maskHandle?.Release();
        }
    }
}
