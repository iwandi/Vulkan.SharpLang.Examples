using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	class SampleInitPipeline
	{
		struct Vertex
		{
			public const uint Size = sizeof(float) * 4;

			float posX, posY, posZ, posW; // Position data
			float r, g, b, a;             // Color

			public Vertex(float x, float y, float z,
				float r, float g, float b)
			{
				posX = x;
				posY = y;
				posZ = z;
				posW = 1;

				this.r = r;
				this.g = g;
				this.b = b;
				a = 1;
			}
		};

		static readonly Vertex[] VbSolidFaceColorsData = new Vertex[] {
			//red face
			new Vertex(-1f,-1f, 1f, 1f, 0f, 0f),
			new Vertex(-1f, 1f, 1f, 1f, 0f, 0f),
			new Vertex( 1f,-1f, 1f, 1f, 0f, 0f),
			new Vertex( 1f,-1f, 1f, 1f, 0f, 0f),
			new Vertex(-1f, 1f, 1f, 1f, 0f, 0f),
			new Vertex( 1f, 1f, 1f, 1f, 0f, 0f),
			//green face
			new Vertex(-1f,-1f,-1f, 0f, 1f, 0f),
			new Vertex( 1f,-1f,-1f, 0f, 1f, 0f),
			new Vertex(-1f, 1f,-1f, 0f, 1f, 0f),
			new Vertex(-1f, 1f,-1f, 0f, 1f, 0f),
			new Vertex( 1f,-1f,-1f, 0f, 1f, 0f),
			new Vertex( 1f, 1f,-1f, 0f, 1f, 0f),
			//blue face
			new Vertex(-1f, 1f, 1f, 0f, 0f, 1f),
			new Vertex(-1f,-1f, 1f, 0f, 0f, 1f),
			new Vertex(-1f, 1f,-1f, 0f, 0f, 1f),
			new Vertex(-1f, 1f,-1f, 0f, 0f, 1f),
			new Vertex(-1f,-1f, 1f, 0f, 0f, 1f),
			new Vertex(-1f,-1f,-1f, 0f, 0f, 1f),
			//yellow face
			new Vertex( 1f, 1f, 1f, 1f, 1f, 0f),
			new Vertex( 1f, 1f,-1f, 1f, 1f, 0f),
			new Vertex( 1f,-1f, 1f, 1f, 1f, 0f),
			new Vertex( 1f,-1f, 1f, 1f, 1f, 0f),
			new Vertex( 1f, 1f,-1f, 1f, 1f, 0f),
			new Vertex( 1f,-1f,-1f, 1f, 1f, 0f),
			//magenta face
			new Vertex( 1f, 1f, 1f, 1f, 0f, 1f),
			new Vertex(-1f, 1f, 1f, 1f, 0f, 1f),
			new Vertex( 1f, 1f,-1f, 1f, 0f, 1f),
			new Vertex( 1f, 1f,-1f, 1f, 0f, 1f),
			new Vertex(-1f, 1f, 1f, 1f, 0f, 1f),
			new Vertex(-1f, 1f,-1f, 1f, 0f, 1f),
			//cyan face
			new Vertex( 1f,-1f, 1f, 0f, 1f, 1f),
			new Vertex( 1f,-1f,-1f, 0f, 1f, 1f),
			new Vertex(-1f,-1f, 1f, 0f, 1f, 1f),
			new Vertex(-1f,-1f, 1f, 0f, 1f, 1f),
			new Vertex( 1f,-1f,-1f, 0f, 1f, 1f),
			new Vertex(-1f,-1f,-1f, 0f, 1f, 1f),
		};

		static void Main(string[] args)
		{
			SampleInstance sample = new SampleInstance();
			sample.InitGlobalLayerProperties();
			sample.InitInstanceeExtensionNames();
			sample.InitDeviceExtensionNames();
			Instance instance = sample.InitInstance("Graphics Pipeline Sample");
			sample.InitEnumerateDevice();
			sample.InitWindowSize(500, 500);
			sample.InitConnection();
			sample.InitWindow();
			sample.InitSwapChainExtension();
			Device device = sample.InitDevice();
			sample.InitCommandPool();
			sample.InitCommandBuffer();
			sample.ExecuteBeginCommandBuffer();
			sample.InitDeviceQueue();
			sample.InitSwapChain();
			sample.InitDepthBuffer();
			sample.InitUniformBuffer();
			sample.InitRenderPass(true);
			sample.InitFrameBuffers(true);
			sample.InitVertexBuffer(VbSolidFaceColorsData, (uint)VbSolidFaceColorsData.Length * Vertex.Size, Vertex.Size, false);
			sample.InitDescriptorAndPipelineLayout();
			sample.InitDescriptorPool();
			sample.InitDescriptorSet();
			sample.InitShaders("basicShader.vert.spv", "basicShader.frag.spv");

			/* VULKAN_KEY_START */
			DynamicState[] dynamicStateEnables = new DynamicState[]
			{
				DynamicState.Viewport,
				DynamicState.Scissor,
			};

			PipelineDynamicStateCreateInfo dynamicState = new PipelineDynamicStateCreateInfo
			{
				DynamicStates = dynamicStateEnables,		
			};

			PipelineVertexInputStateCreateInfo vi = new PipelineVertexInputStateCreateInfo
			{
				VertexBindingDescriptions = sample.ViBinding,
				VertexAttributeDescriptions = sample.ViAttribs,
			};

			PipelineInputAssemblyStateCreateInfo ia = new PipelineInputAssemblyStateCreateInfo
			{
				PrimitiveRestartEnable = false,
				Topology = PrimitiveTopology.TriangleList,
			};

			PipelineRasterizationStateCreateInfo rs = new PipelineRasterizationStateCreateInfo
			{
				PolygonMode = PolygonMode.Fill,
				CullMode = CullModeFlags.Back,
				FrontFace = FrontFace.Clockwise,
				DepthClampEnable = true,
				LineWidth = 1f,
			};

			PipelineColorBlendAttachmentState[] attState = new PipelineColorBlendAttachmentState[]
			{
				new PipelineColorBlendAttachmentState
				{
					AlphaBlendOp = BlendOp.Add,
					ColorBlendOp = BlendOp.Add,
					SrcColorBlendFactor = BlendFactor.Zero,
					DstColorBlendFactor = BlendFactor.Zero,
					SrcAlphaBlendFactor = BlendFactor.Zero,
					DstAlphaBlendFactor = BlendFactor.Zero,
				},
			};

			PipelineColorBlendStateCreateInfo cb = new PipelineColorBlendStateCreateInfo
			{
				Attachments = attState,
				LogicOpEnable = false,
				LogicOp = LogicOp.NoOp,
				BlendConstants = new float[] { 1f, 1f, 1f, 1f },
			};

			PipelineViewportStateCreateInfo vp = new PipelineViewportStateCreateInfo
			{
				ViewportCount = 1,
				ScissorCount = 1,
			};

			StencilOpState stencilOpState = new StencilOpState
			{
				FailOp = StencilOp.Keep,
				PassOp = StencilOp.Keep,
				CompareOp = CompareOp.Always,
				DepthFailOp = StencilOp.Keep,
			};
			PipelineDepthStencilStateCreateInfo ds = new PipelineDepthStencilStateCreateInfo
			{
				DepthTestEnable = true,
				DepthWriteEnable = true,
				DepthCompareOp = CompareOp.LessOrEqual,
				Back = stencilOpState,
				Front = stencilOpState,
			};

			PipelineMultisampleStateCreateInfo ms = new PipelineMultisampleStateCreateInfo
			{
				RasterizationSamples = SampleCountFlags.Count1,
				MinSampleShading = 0,				
			};

			Pipeline pipeline = device.CreateGraphicsPipelines(null,
				new GraphicsPipelineCreateInfo[]
				{
					new GraphicsPipelineCreateInfo
					{
						Layout =  sample.PipelineLayout,
						VertexInputState = vi,
						InputAssemblyState = ia,
						RasterizationState = rs,
						ColorBlendState = cb,
						MultisampleState = ms,
						DynamicState = dynamicState,
						ViewportState = vp,
						DepthStencilState = ds,
						Stages = sample.ShaderStages,
						RenderPass = sample.RenderPass,
						Subpass = 0,
					}
				})[0];

			/* VULKAN_KEY_END */
			device.DestroyPipeline(pipeline);

			sample.DestroyDescriptorPool();
			sample.DestroyVertexBuffer();
			sample.DestroyFrameBuffers();
			sample.DestroyShaders();
			sample.DestroyRenderPass();
			sample.DestroyDescriptorAndPipelineLayout();
			sample.DestroyUniformBuffer();
			sample.DestroyDepthBuffer();
			sample.DestroySwapChain();
			sample.DestroyCommandBuffer();
			sample.DestroyCommandPool();
			device.Destroy();
			sample.DestroyWindow();
			instance.Destroy();
		}
	}
}
