using System;
using System.Runtime.InteropServices;

namespace Vulkan.SharpLang.Examples
{
	public static class VulkanHelper
	{
		[DllImport("msvcrt.dll", SetLastError = false)]
		static extern IntPtr memcpy(IntPtr dest, IntPtr src, int length);
		[DllImport("kernel32.dll")]
		static extern void CopyMemory(IntPtr dest, IntPtr src, uint length);

		public static QueueFlags ToQueueFlags(GraphicsQueueType value)
		{
			switch(value)
			{
				case GraphicsQueueType.Compute:
					return QueueFlags.Compute;
				case GraphicsQueueType.Graphics:
					return QueueFlags.Graphics;
				case GraphicsQueueType.SparseBinding:
					return QueueFlags.SparseBinding;
				case GraphicsQueueType.Transfer:
					return QueueFlags.Transfer;
				default:
					throw new NotImplementedException();
			}
		}

		public static GraphicsQueueType ToGraphicsQueueType(QueueFlags value)
		{
			switch (value)
			{
				case QueueFlags.Compute:
					return GraphicsQueueType.Compute;
				case QueueFlags.Graphics:
					return GraphicsQueueType.Graphics;
				case QueueFlags.SparseBinding:
					return GraphicsQueueType.SparseBinding;
				case QueueFlags.Transfer:
					return GraphicsQueueType.Transfer;
				default:
					throw new NotImplementedException();
			}
		}

		public static ShaderStageFlags ToShaderStageFlags(ShaderType value)
		{
			switch (value)
			{
				case ShaderType.Vertex:
					return ShaderStageFlags.Vertex;
				case ShaderType.Pixel:
					return ShaderStageFlags.Fragment;
				default:
					throw new NotImplementedException();
			}
		}

		public static ShaderType ToShaderStageFlags(ShaderStageFlags value)
		{
			switch (value)
			{
				case ShaderStageFlags.Vertex:
					return ShaderType.Vertex;
				case ShaderStageFlags.Fragment:
					return ShaderType.Pixel;
				default:
					throw new NotImplementedException();
			}
		}

		public static Extent2D ToExtend2D(Extent3D value)
		{
			return new Extent2D
			{
				Width = value.Width,
				Height = value.Height,
			};
		}

		public static Extent3D ToExtend3D(Extent2D value, uint depth = 1)
		{
			return new Extent3D
			{
				Width = value.Width,
				Height = value.Height,
				Depth = depth,
			};
		}

		public static Rect2D ToRect2D(Extent2D extent, int x = 0, int y = 0)
		{
			return new Rect2D
			{
				Extent = extent,
				Offset = new Offset2D { X = x, Y = y },
			};
		}

		public static Rect2D ToRect2D(Extent2D extent, Offset2D offset)
		{
			return new Rect2D
			{
				Extent = extent,
				Offset = offset,
			};
		}

		public static int SizeOf<T>()
		{
			Type type = typeof(T);

			if (type.IsEnum)
			{
				return Marshal.SizeOf(Enum.GetUnderlyingType(type));
			}
			if (type.IsValueType)
			{
				return Marshal.SizeOf(type);
			}
			return Marshal.SizeOf(type);
		}

		public static void MemCopy(IntPtr source, IntPtr dest, int size)
		{
			CopyMemory(dest, source, (uint)size);
		}
	}
}
