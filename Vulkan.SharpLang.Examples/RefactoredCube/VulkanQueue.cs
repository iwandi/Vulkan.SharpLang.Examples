using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanQueue : IGraphicsQueue
	{
		public GraphicsQueueType Type { get; set; }
		internal QueueFlags VulkanType { get; set; }

		uint familyIndex;
		uint subIndex;

		Device device;

		Queue queue;
		CommandPool pool;
		CommandBuffer cmd;

		public Queue Queue { get { return queue; } }
		public CommandPool Pool { get { return pool; } }
		public CommandBuffer Cmd { get { return cmd; } }

		Fence submitFence;
		public Fence SubmitFence { get { return submitFence; } }

		public VulkanQueue(GraphicsQueueType type)
		{
			Type = type;
			VulkanType = VulkanHelper.ToQueueFlags(type);
		}

		internal void SetFamilyIndex(uint familyIndex, uint subIndex)
		{
			this.familyIndex = familyIndex;
			this.subIndex = subIndex;
		}

		internal void InitQueue(Device device)
		{
			this.device = device;

			queue = device.GetQueue(familyIndex, subIndex);

			pool = device.CreateCommandPool(new CommandPoolCreateInfo
			{
				QueueFamilyIndex = familyIndex,
				Flags = CommandPoolCreateFlags.ResetCommandBuffer,
			});

			cmd = device.AllocateCommandBuffers(new CommandBufferAllocateInfo
			{
				CommandPool = pool,
				Level = CommandBufferLevel.Primary,
				CommandBufferCount = 1,				
			})[0];

			cmd.Begin(new CommandBufferBeginInfo
			{
				InheritanceInfo = new CommandBufferInheritanceInfo (),
			});

			submitFence = device.CreateFence(new FenceCreateInfo { });
		}

		public void Reset()
		{
			Reset(device);
		}

		public void Reset(Device device)
		{
			cmd.Reset();
			device.ResetFence(submitFence);
			cmd.Begin(new CommandBufferBeginInfo
			{
				InheritanceInfo = new CommandBufferInheritanceInfo { },
			});
		}

		public void Submit(ISwapChain swapChain)
		{
			VulkanSwapChain vkSwapChain = swapChain as VulkanSwapChain;
			if(vkSwapChain != null)
			{
				Submit(vkSwapChain.ImageAcquiredSemaphore);
			}
		}

		public void Submit(Semaphore imageAcquiredSemaphore)
		{
			cmd.End();

			queue.Submit(new SubmitInfo
			{
				WaitSemaphores = new Semaphore[] { imageAcquiredSemaphore },
				WaitDstStageMask = new PipelineStageFlags[] {  PipelineStageFlags.ColorAttachmentOutput },
				CommandBuffers = new CommandBuffer[] { cmd },
			}, submitFence);
		}

		public void Destroy(Device device)
		{
			if (submitFence != null)
			{
				device.DestroyFence(submitFence);
				submitFence = null;
			}

			if (cmd != null)
			{
				device.FreeCommandBuffer(pool, cmd);
				cmd = null;
			}

			if (pool != null)
			{
				device.DestroyCommandPool(pool);
				pool = null;
			}
		}

		public void SetPipelineState(IPipelineState pipeline)
		{

		}

		public void Bind(IBuffer buffer)
		{

		}

		public void Draw(IBuffer vertexBuffer)
		{

		}
	}
}
