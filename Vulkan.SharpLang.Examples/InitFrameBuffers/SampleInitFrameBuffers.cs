using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	class SampleInitFrameBuffers
	{
		static void Main(string[] args)
		{
			SampleInstance sample = new SampleInstance();
			sample.InitGlobalLayerProperties();
			sample.InitInstanceeExtensionNames();
			sample.InitDeviceExtensionNames();
			Instance instance = sample.InitInstance("Init Framebuffer Sample");
			sample.InitEnumerateDevice();
			sample.InitConnection();
			sample.InitWindowSize(50, 50);
			sample.InitWindow();
			sample.InitSwapChainExtension();
			Device device = sample.InitDevice();
			sample.InitCommandPool();
			sample.InitCommandBuffer();
			sample.ExecuteBeginCommandBuffer();
			sample.InitDeviceQueue();
			sample.InitSwapChain();
			sample.InitDepthBuffer();
			sample.InitRenderPass(true);

			/* VULKAN_KEY_START */
			ImageView[] attachments = new ImageView[2];
			attachments[0] = sample.Buffers[0].view;
			attachments[1] = sample.DepthView;

			FramebufferCreateInfo fbInfo = new FramebufferCreateInfo
			{
				RenderPass = sample.RenderPass,
				Attachments = attachments,
				Width = sample.Width,
				Height = sample.Height,
				Layers = 1,
			};

			Framebuffer[] frameBuffers = new Framebuffer[sample.Buffers.Length];

			for(int i = 0; i < sample.Buffers.Length; i++)
			{
				attachments[0] = sample.Buffers[i].view;
				frameBuffers[i] = device.CreateFramebuffer(fbInfo);
			}

			sample.ExecuteEndCommandBuffer();
			sample.ExecuteQueueCommandBuffer();
			/* VULKAN_KEY_END */

			foreach(Framebuffer frameBuffer in frameBuffers)
			{
				device.DestroyFramebuffer(frameBuffer);
			}

			sample.DestroyRenderPass();
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
