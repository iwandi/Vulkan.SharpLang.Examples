using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanMemoryManager
	{
		public Device device;

		PhysicalDeviceMemoryProperties memoryProperties;

		internal VulkanMemoryManager(PhysicalDevice gpu, Device device)
		{
			this.device = device;

			memoryProperties = gpu.GetMemoryProperties();
		}

		uint GetMemoryTypeIndex(uint typeBits, MemoryPropertyFlags requirementsMask)
		{
			// Search memtypes to find first index with those properties
			for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
			{
				if ((typeBits & 1) == 1)
				{
					// Type is available, does it match user properties?
					if ((memoryProperties.MemoryTypes[i].PropertyFlags & requirementsMask) == requirementsMask)
					{
						return i;
					}
				}
				typeBits >>= 1;
			}
			throw new Exception(string.Format("Unable to GetMemoryTypeIndex for {0} {1}", typeBits, requirementsMask));
		}

		// TODO : manage allocations 
		public VulkanAlloc Alloc(MemoryRequirements memReqs, MemoryPropertyFlags flags)
		{
			return new VulkanAlloc(device.AllocateMemory(new MemoryAllocateInfo
			{
				AllocationSize = memReqs.Size,
				MemoryTypeIndex = GetMemoryTypeIndex(memReqs.MemoryTypeBits, flags),
			}));
		}

		public void Free(VulkanAlloc alloc)
		{
			device.FreeMemory(alloc.Memory);
		}
	}
}
