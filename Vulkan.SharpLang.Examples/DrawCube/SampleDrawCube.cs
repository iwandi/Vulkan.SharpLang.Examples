using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	class SampleDrawCube
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
			Instance instance = sample.InitInstance("Draw Cube");
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
			sample.InitDescriptorAndPipelineLayout();
			sample.InitRenderPass(true);
			sample.InitShaders("basicShader.vert.spv", "basicShader.frag.spv");
			sample.InitFrameBuffers(true);
			sample.InitVertexBuffer(VbSolidFaceColorsData, (uint)VbSolidFaceColorsData.Length * Vertex.Size, Vertex.Size, false);
			sample.InitDescriptorPool();
			sample.InitDescriptorSet();
			sample.InitPipelineCache();
			sample.InitPipeline(true);

			/* VULKAN_KEY_START */

			/* VULKAN_KEY_END */
			// TODO Destroy Semaphore
			// TODO Destroy Fence

			sample.DestroyPipeline();
			sample.DestroyPipelineCache();
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

			Console.ReadLine();
		}
	}
}
