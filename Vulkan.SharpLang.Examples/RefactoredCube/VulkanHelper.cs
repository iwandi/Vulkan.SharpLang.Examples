using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulkan.SharpLang.Examples
{
	public static class VulkanHelper
	{
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
	}
}
