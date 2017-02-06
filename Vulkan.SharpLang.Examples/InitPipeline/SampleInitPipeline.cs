using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	class SampleInitPipeline
	{
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
			sample.InitVertexBuffer();
			sample.InitDescriptorAndPipelineLayout();
			sample.InitDescriptorPool();
			sample.InitDescriptorSet();
			sample.InitShaders();

			/* VULKAN_KEY_START */

			/* VULKAN_KEY_END */
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
