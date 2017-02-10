using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanQueue //: IGraphicsQueue
	{
		public GraphicsQueueType Type { get; set; }
		internal QueueFlags VulkanType { get; set; }

		uint familyIndex;
		uint subIndex;

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
			device.GetQueue(familyIndex, subIndex);
		}
	}
}
