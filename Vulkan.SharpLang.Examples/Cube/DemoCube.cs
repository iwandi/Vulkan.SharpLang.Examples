using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	class DemoCube
	{
		struct Vertex
		{
			public const uint Size = sizeof(float) * 8;

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

		const uint FENCE_TIMEOUT = 100000000;
		static void Main(string[] args)
		{

			SampleInstance sample = new SampleInstance();
			sample.InitGlobalLayerProperties();
			sample.InitInstanceeExtensionNames();
			sample.InitDeviceExtensionNames();
			Instance instance = sample.InitInstance("Cube");
			sample.InitEnumerateDevice();
			sample.InitWindowSize(500, 500);
			sample.InitConnection();
			sample.InitWindow();
			sample.InitSwapChainExtension();
			Device device = sample.InitDevice();

			sample.InitCommandPool();
			CommandBuffer cmd = sample.InitCommandBuffer();
			sample.ExecuteBeginCommandBuffer();
			Queue queue = sample.InitDeviceQueue();
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

			bool run = true;

			ClearValue[] clearValues = new ClearValue[]
			{
				new ClearValue
				{
					Color = new ClearColorValue (new float[] { 0.2f, 0.2f, 0.3f, 0.2f }),
				},
				new ClearValue
				{
					DepthStencil = new ClearDepthStencilValue
					{
						Depth = 1f,
						Stencil = 0,
					},
				},
			};

			Fence drawFence = device.CreateFence(new FenceCreateInfo { });

			Semaphore imageAcquiredSemaphore = device.CreateSemaphore(new SemaphoreCreateInfo { });

			int i = 0;
			while (run)
			{
				sample.CurrentBuffer = device.AcquireNextImageKHR(sample.SwapChain, uint.MaxValue, imageAcquiredSemaphore);

				RenderPassBeginInfo rpBegin = new RenderPassBeginInfo
				{
					RenderPass = sample.RenderPass,
					Framebuffer = sample.FrameBuffers[sample.CurrentBuffer],
					RenderArea = new Rect2D
					{
						Extent = new Extent2D { Width = sample.Width, Height = sample.Height },
						Offset = new Offset2D { X = 0, Y = 0 },
					},
					ClearValues = clearValues,
				};

				cmd.Reset();
				device.ResetFence(drawFence);


				sample.ExecuteBeginCommandBuffer();
				cmd.CmdBeginRenderPass(rpBegin, SubpassContents.Inline);
				cmd.CmdBindPipeline(PipelineBindPoint.Graphics, sample.Pipeline);
				cmd.CmdBindDescriptorSets(PipelineBindPoint.Graphics, sample.PipelineLayout, 0, sample.DescSet, null); // TODO : check firstSet
				cmd.CmdBindVertexBuffers(0, new Buffer[] { sample.VertexBuffer }, new DeviceSize[] { 0 });

				sample.InitViewPorts();
				sample.InitScissors();

				cmd.CmdDraw(12 * 3, 1, 0, 0);
				cmd.CmdEndRenderPass();

				cmd.End();


				SubmitInfo[] submitInfo = new SubmitInfo[]
				{
					new SubmitInfo
					{
						WaitSemaphores = new Semaphore[] { imageAcquiredSemaphore },
						WaitDstStageMask = new PipelineStageFlags[] { PipelineStageFlags.ColorAttachmentOutput },
						CommandBuffers = new CommandBuffer[] { cmd },
					}
				};

				/* Queue the command buffer for execution */
				queue.Submit(submitInfo, drawFence);

				/* Now present the image in the window */
				PresentInfoKhr present = new PresentInfoKhr
				{
					Swapchains = new SwapchainKhr[] { sample.SwapChain },
					ImageIndices = new uint[] { sample.CurrentBuffer },
				};

				//device.WaitForFences(new Fence[] { drawFence }, true, uint.MaxValue);
				bool check = true;
				do
				{
					try
					{
						device.WaitForFences(new Fence[] { drawFence }, true, FENCE_TIMEOUT);
						check = false;
					}
					catch (ResultException ex)
					{
						if (ex.Result != Result.Timeout)
						{
							Console.WriteLine(ex.Message);
							Console.WriteLine(ex.StackTrace);
							check = false;
						}
					}
				}
				while (check);

				queue.PresentKHR(present);

				System.Windows.Forms.Application.DoEvents();

				i++;
				if(i > 3)
				{
					run = false;
				}
			}

			/* VULKAN_KEY_END */
			device.DestroySemaphore(imageAcquiredSemaphore);
			device.DestroyFence(drawFence);

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
