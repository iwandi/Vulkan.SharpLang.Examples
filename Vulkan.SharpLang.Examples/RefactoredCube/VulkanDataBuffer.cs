using System;
using System.Runtime.InteropServices;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanDataBuffer : IBuffer
	{
		protected Device device;
		protected VulkanMemoryManager memoryManager;

		protected DataInfo info;
		protected Buffer buffer;
		protected VulkanAlloc alloc;

		protected void InitBuffer(BufferUsageFlags usage)
		{
			buffer = device.CreateBuffer(new BufferCreateInfo
			{
				Usage = usage,
				SharingMode = SharingMode.Exclusive,
				Size = info.TotalSize,
			});

			alloc = memoryManager.Alloc(device.GetBufferMemoryRequirements(buffer), MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent);
		}

		protected virtual void InitDataInfo(uint count)
		{
			throw new NotImplementedException();
		}

		public void CopyToBuffer(object data, uint offset, uint size)
		{
			IntPtr pDest = device.MapMemory(alloc.Memory, alloc.Offset + offset, size);
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				IntPtr pData = handle.AddrOfPinnedObject();
				VulkanHelper.MemCopy(pData, pDest, (int)size);
			}
			finally
			{
				if (handle.IsAllocated)
				{
					handle.Free();
				}
			}
			device.UnmapMemory(alloc.Memory);
		}

		public void Destroy()
		{
			memoryManager.Free(alloc);
			device.DestroyBuffer(buffer);
		}

		protected class DataInfo
		{
			public uint ElementSize;
			public uint Elements;
			public uint TotalSize { get { return ElementSize * Elements; } }
		}
	}

	public class VulkanDataBuffer<T> : VulkanDataBuffer, IBuffer, IBuffer<T> where T : struct
	{
		public VulkanDataBuffer(Device device, VulkanMemoryManager memoryManager, uint count, BufferUsageFlags usage)
		{
			this.device = device;
			this.memoryManager = memoryManager;
			InitDataInfo(count);
			InitBuffer(usage);
		}

		protected override void InitDataInfo(uint count)
		{
			info = new DataInfo
			{
				ElementSize = (uint)VulkanHelper.SizeOf<T>(),
				Elements = count,
			};
		}

		public void Write(T data, int index)
		{
			CopyToBuffer(data, info.ElementSize * (uint)index, info.ElementSize);
		}

		public void Write(T[] data, int sourceIndex, int targetIndex, int count)
		{
			// TODO 
		}

		public void Write(T[] data)
		{
			Write(data, 0, (uint)data.Length);
		}

		public void Write(T[] data, uint index, uint count)
		{
			CopyToBuffer(data[0], info.ElementSize * index, count * info.ElementSize);
		}

		public void CopyToBuffer(T data, uint offset, uint size)
		{
			IntPtr pDest = device.MapMemory(alloc.Memory, alloc.Offset + offset, size);
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				IntPtr pData = handle.AddrOfPinnedObject();
				VulkanHelper.MemCopy(pData, pDest, (int)size);
			}
			finally
			{
				if (handle.IsAllocated)
				{
					handle.Free();
				}
			}
			device.UnmapMemory(alloc.Memory);
		}
	}
}
