using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanAlloc
	{
		DeviceMemory memory;
		public DeviceMemory Memory { get { return memory; } }

		DeviceSize offset;
		public DeviceSize Offset {  get { return offset; } }

		internal VulkanAlloc(DeviceMemory memory)
		{
			this.memory = memory;
			this.offset = (DeviceSize)0;
		}

		internal VulkanAlloc(DeviceMemory memory, DeviceSize offset)
		{
			this.memory = memory;
			this.offset = offset;
		}
	}
}
