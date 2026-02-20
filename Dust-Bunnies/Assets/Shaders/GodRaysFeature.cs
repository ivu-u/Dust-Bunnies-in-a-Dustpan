using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using Unity.VisualScripting;

public class GodRaysFeature : ScriptableRendererFeature {
    public enum DownSample { off = 1, half = 2, third = 3, quarter = 4}
    [System.Serializable]
    public class Settings {
        public DownSample downsampling;
        public enum Stage { raymarch, gaussianBlur, full };

        [Space(10)]
        public Stage stage;
        public float intensity = 1;
        public float scattering = 0;
        public float steps = 24;
        public float maxDistance = 75;
        public float jitter = 250;

        public bool debugDepth = false;

        [System.Serializable]
        public class GaussBlur {
            public float amount;
            public float samples;
        }

        public GaussBlur gaussBlur = new GaussBlur();
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public Settings settings = new Settings();

    RTHandle renderTextureHandle;
    private static readonly int Scattering = Shader.PropertyToID("_Scattering");
    private static readonly int Steps = Shader.PropertyToID("_Steps");
    private static readonly int JitterVolumetric = Shader.PropertyToID("_JitterVolumetric");
    private static readonly int MaxDistance = Shader.PropertyToID("_MaxDistance");
    private static readonly int Intensity = Shader.PropertyToID("_Intensity");
    private static readonly int GaussSamples = Shader.PropertyToID("_GaussSamples");
    private static readonly int GaussAmount = Shader.PropertyToID("_GaussAmount");
    private static readonly int Boost = Shader.PropertyToID("_Boost");
    private static readonly int DepthSteps = Shader.PropertyToID("_DepthSteps");
    private static readonly int DepthMaxDistance = Shader.PropertyToID("_DepthMaxDistance");
    private static readonly int ColorJitterMultiplier = Shader.PropertyToID("_ColorJitterMultiplier");
    private static readonly int contrastProperty = Shader.PropertyToID("_Contrast");
    private static readonly int brightnessProperty = Shader.PropertyToID("_Brightness");
    private static readonly int middleValueProperty = Shader.PropertyToID("_MiddleValue");
    private static readonly int axis = Shader.PropertyToID("_Axis");

    Pass pass;

    /// <inheritdoc/>
    public override void Create() {
        pass = new Pass("God Rays");
        name = "God Rays";
        pass.settings = settings;

        // Configures where the render pass should be injected.
        pass.renderPassEvent = settings.renderPassEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(pass);
        
    }
    class Pass : ScriptableRenderPass
    {
        public Settings settings;
        private RTHandle source;
        RTHandle tempTexture;
        RTHandle lowResDepthRT;
        RTHandle tempTexture3;

        private string profilerTag;

        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        public Pass(string profilerTag) { 
            this.profilerTag = profilerTag;
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            // SAFE access to camera color target handle (in pass scope)
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            var desc = renderingData.cameraData.cameraTargetDescriptor;

            int divider = (int)settings.downsampling;
            desc.width /= divider;
            desc.height /= divider;

            desc.colorFormat = RenderTextureFormat.DefaultHDR;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            RenderingUtils.ReAllocateHandleIfNeeded(ref tempTexture, desc, name: "_GodRaysTemp");
            RenderingUtils.ReAllocateHandleIfNeeded(ref lowResDepthRT, desc, name: "_GodRaysLowResDepth");
            RenderingUtils.ReAllocateHandleIfNeeded(ref tempTexture3, desc, name: "_GodRaysTemp3");

            // Pick the target you actually want to render into for this pass.
            // Often you'd target tempTexture (then blit back to source in Execute).
            ConfigureTarget(tempTexture);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        // NOTE: This method is part of the compatibility rendering path, please use the Render Graph API above instead.
        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            cmd.Clear();

            if (settings.debugDepth) {
                cmd.Blit(source, source, settings.material, 4); // show depth
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                return;
            }

            try {
                settings.material.SetFloat("_Scattering", settings.scattering);
                settings.material.SetFloat("_Steps", settings.steps);
                settings.material.SetFloat("_JitterVolumetric", settings.jitter);
                settings.material.SetFloat("_MaxDistance", settings.maxDistance);
                settings.material.SetFloat("_Intensity", settings.intensity);
                settings.material.SetFloat("_GaussSamples", settings.gaussBlur.samples);
                settings.material.SetFloat("_GaussAmount", settings.gaussBlur.amount);

                switch (settings.stage) 
                {
                    case Settings.Stage.raymarch:
                        cmd.Blit(source, tempTexture);
                        cmd.Blit(tempTexture, source, settings.material, 0);
                        break;

                    case Settings.Stage.gaussianBlur:
                        cmd.Blit(source, tempTexture, settings.material, 0);
                        cmd.Blit(tempTexture, lowResDepthRT, settings.material, 1);
                        cmd.Blit(lowResDepthRT, source, settings.material, 2);
                        break;
                    case Settings.Stage.full:
                        //raymarch
                        cmd.Blit(source, tempTexture, settings.material, 0);
                        //bilateral blu X, we use the lowresdepth render texture for other things too, it is just a name
                        cmd.Blit(tempTexture, lowResDepthRT, settings.material, 1);
                        //bilateral blur Y
                        cmd.Blit(lowResDepthRT, tempTexture, settings.material, 2);
                        //save it in a global texture
                        cmd.SetGlobalTexture("_volumetricTexture", tempTexture);
                        //downsample depth
                        cmd.Blit(source, lowResDepthRT, settings.material, 4);
                        cmd.SetGlobalTexture("_LowResDepth", lowResDepthRT);
                        //upsample and composite
                        cmd.Blit(source, tempTexture3, settings.material, 3);
                        cmd.Blit(tempTexture3, source);
                        break;
                }

                context.ExecuteCommandBuffer(cmd);
            }
            catch {
                Debug.LogError("Error: Memory Leak in God Rays");
            }

            cmd.Clear();
            CommandBufferPool.Release(cmd);

        }

    }
}
