using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	class SampleInitVertexBuffer
	{
		[DllImport("msvcrt.dll", SetLastError = false)]
		static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);
		[DllImport("kernel32.dll")]
		static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

		struct Vertex
		{
			public const int Size = sizeof(float) * 4;

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
			Instance instance = sample.InitInstance("Vertex Buffer Sample");
			sample.InitEnumerateDevice();
			sample.InitWindowSize(500, 500);
			sample.InitConnection();
			sample.InitWindow();
			sample.InitSwapChainExtension();
			Device device = sample.InitDevice();
			sample.InitCommandPool();
			CommandBuffer cmd = sample.InitCommandBuffer();
			sample.ExecuteBeginCommandBuffer();
			sample.InitDeviceQueue();
			sample.InitSwapChain();
			sample.InitDepthBuffer();
			sample.InitRenderPass(true);
			sample.InitFrameBuffers(true);

			/* VULKAN_KEY_START */
			/*
			 * Set up a vertex buffer:
			 * - Create a buffer
			 * - Map it and write the vertex data into it
			 * - Bind it using vkCmdBindVertexBuffers
			 * - Later, at pipeline creation,
			 * -      fill in vertex input part of the pipeline with relevent data
			 */

			int vertexBufferSize = Vertex.Size * VbSolidFaceColorsData.Length;

			Buffer vertexBuffer = device.CreateBuffer(new BufferCreateInfo
			{
				Usage = BufferUsageFlags.VertexBuffer,
				Size = vertexBufferSize,
				SharingMode = SharingMode.Exclusive,
			});

			MemoryRequirements memReqs = device.GetBufferMemoryRequirements(vertexBuffer);			

			uint memoryTypeIndex;
			Debug.Assert(sample.MemoryTypeFromProperties(memReqs.MemoryTypeBits, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out memoryTypeIndex));

			MemoryAllocateInfo allocInfo = new MemoryAllocateInfo
			{
				AllocationSize = memReqs.Size,
				MemoryTypeIndex = memoryTypeIndex,
			};

			DeviceMemory vertexBufferMem = device.AllocateMemory(allocInfo);

			IntPtr pData = device.MapMemory(vertexBufferMem, 0, memReqs.Size);
			GCHandle dataHandle = GCHandle.Alloc(VbSolidFaceColorsData, GCHandleType.Pinned);
			try
			{
				IntPtr pDataHandle = dataHandle.AddrOfPinnedObject();
				CopyMemory(pData, pDataHandle, (uint)vertexBufferSize);
			}
			finally
			{
				if(dataHandle.IsAllocated)
				{
					dataHandle.Free();
				}
			}
			
			device.UnmapMemory(vertexBufferMem);

			device.BindBufferMemory(vertexBuffer, vertexBufferMem, 0);

			// Skip some stuf not related to vulkan

			/* We cannot bind the vertex buffer until we begin a renderpass */
			ClearValue[] clearValues = new ClearValue[]
			{
				new ClearValue
				{
					Color = new ClearColorValue( new float[] { 0.2f, 0.2f, 0.2f, 0.2f } ),
				},
				new ClearValue
				{
					DepthStencil = new ClearDepthStencilValue
					{
						Depth = 1.0f,
						Stencil = 0,
					},
				}
			};

			Semaphore imageAcquiredSemaphore = device.CreateSemaphore(new SemaphoreCreateInfo { });

			uint currentBuffer = device.AcquireNextImageKHR(sample.SwapChain, uint.MaxValue, imageAcquiredSemaphore);
			sample.CurrentBuffer = currentBuffer;

			cmd.CmdBeginRenderPass(new RenderPassBeginInfo
			{
				RenderPass = sample.RenderPass,
				Framebuffer = sample.FrameBuffers[sample.CurrentBuffer],
				RenderArea = new Rect2D
				{
					Offset = new Offset2D { X = 0, Y = 0},
					Extent = new Extent2D {  Width = sample.Width, Height = sample.Height }
				},
				ClearValues = clearValues,
			}, SubpassContents.Inline);

			cmd.CmdBindVertexBuffers(0, new Buffer[] { vertexBuffer }, new DeviceSize[] { 0 });

			cmd.CmdEndRenderPass();

			sample.ExecuteEndCommandBuffer();
			sample.ExecuteQueueCommandBuffer();
			/* VULKAN_KEY_END */

			device.DestroySemaphore(imageAcquiredSemaphore);
			device.DestroyBuffer(vertexBuffer);
			device.FreeMemory(vertexBufferMem);

			sample.DestroyFrameBuffers();
			sample.DestroyRenderPass();
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
