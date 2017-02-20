using System;
using System.Collections.Generic;
using Vulkan;
using Vulkan.Windows;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanGraphics : IGraphics
	{
		List<string> deviceExtensions;
		List<string> deviceLayers;

		Instance instance;
		PhysicalDevice gpu;
		Device device;
		List<VulkanQueue> queues = new List<VulkanQueue>();

		VulkanQueue queueGraphics;
		VulkanQueue queueCompute;
		VulkanQueue queueTransfer;

		VulkanMemoryManager memory;


		public IGraphicsQueue QueueGraphics { get { return queueGraphics; } }
		public IGraphicsQueue QueueCompute { get { return queueCompute; } }
		public IGraphicsQueue QueueTransfer { get { return queueTransfer; } }

		public void Init(AppInfo info)
		{
			// TODO : Check if extentions Exists
			List<string> extensions = new List<string>();
			extensions.Add("VK_KHR_surface");
			extensions.Add("VK_KHR_win32_surface");
			List<string> layers = new List<string>();

			if (info.Debug)
			{
				extensions.Add("VK_EXT_debug_report");
				layers.Add("VK_LAYER_LUNARG_standard_validation");
			}

			deviceExtensions = new List<string>();
			deviceExtensions.Add("VK_KHR_swapchain");
			deviceLayers = new List<string>();

			if (info.Debug)
			{
				deviceLayers.Add("VK_LAYER_LUNARG_standard_validation");
			}

			instance = new Instance(new InstanceCreateInfo
			{
				ApplicationInfo = new ApplicationInfo
				{
					ApplicationName = info.Title,
					ApplicationVersion = info.Version,
					EngineName = info.EngineTitle,
					EngineVersion = info.Version,
					ApiVersion = Version.Make(1, 0, 39),
				},
				EnabledExtensionNames = extensions.ToArray(),
				EnabledLayerNames = layers.ToArray(),
			});
		}

		VulkanQueue CreateQueue(GraphicsQueueType type)
		{
			VulkanQueue queue = new VulkanQueue(type);
			queues.Add(queue);
			return queue;
		}

		public void InitDevice()
		{
			gpu = SelectPhysicalDevice();
			queueGraphics = CreateQueue(GraphicsQueueType.Graphics); 
			queueCompute = CreateQueue(GraphicsQueueType.Compute);
			queueTransfer = CreateQueue(GraphicsQueueType.Transfer);
			device = CreateDevice(gpu);
			InitQueues();

			memory = new VulkanMemoryManager(gpu, device);
		}

		PhysicalDevice SelectPhysicalDevice()
		{
			// TODO : better weay to select the best gpu
			PhysicalDevice[] gpus = instance.EnumeratePhysicalDevices();
			PhysicalDevice gpu = gpus[0];
			return gpu;
		}

		Device CreateDevice(PhysicalDevice gpu)
		{
			// TODO : we need a presentationSurface a this point

			DeviceQueueCreateInfoFactory queueFactory = new DeviceQueueCreateInfoFactory(gpu);			
;
			for(int i = 0; i < queues.Count; i++)
			{
				VulkanQueue queue = queues[i];
				queueFactory.AddQueue(queue);
			}

			Device device = gpu.CreateDevice(new DeviceCreateInfo
			{
				QueueCreateInfos = queueFactory.QueueCreateInfos,
				EnabledExtensionNames = deviceExtensions.ToArray(),
				EnabledLayerNames = deviceLayers.ToArray(),
			});			
			return device;
		}

		void InitQueues()
		{
			for (int i = 0; i < queues.Count; i++)
			{
				VulkanQueue queue = queues[i];
				queue.InitQueue(device);
			}
		}

		public void Destroy()
		{
			queueGraphics.Destroy(device);
			queueCompute.Destroy(device);
			queueTransfer.Destroy(device);

			device.Destroy();
			instance.Destroy();

			queues.Clear();
			device = null;
			gpu = null;
			instance = null;
		}

		public ISwapChain CreateSwapChain(IWindow window)
		{
			WindowsWindow wWindow = window as WindowsWindow;
			if (wWindow != null)
			{
				VulkanSwapChain swapChain =  new VulkanSwapChain(instance.CreateWin32SurfaceKHR(new Win32SurfaceCreateInfoKhr
				{
					Hinstance = System.Runtime.InteropServices.Marshal.GetHINSTANCE(this.GetType().Module),
					Hwnd = wWindow.Handle,
				}));
				swapChain.Init(gpu, device, memory, queueGraphics, (uint)window.Info.ContentWidth, (uint)window.Info.ContentHeight);

				return swapChain;
			}
			else
			{
				throw new Exception("invalid window type");
			}
		}

		public void DestroySwapChain(ISwapChain swapChain)
		{
			VulkanSwapChain vkSwapChain = swapChain as VulkanSwapChain;
			if (vkSwapChain != null)
			{
				vkSwapChain.Destroy(instance, device, memory);
				vkSwapChain = null;
			}
		}

		public IPipelineState CreatePipelineState()
		{
			return new VulkanPipelineState(device);
		}

		public void DestroyPipelineState(IPipelineState pipelineState)
		{
			VulkanPipelineState vkPupelineState = pipelineState as VulkanPipelineState;
			if(vkPupelineState != null)
			{
				vkPupelineState.Destroy();
			}
		}

		public IBuffer<T> CreateVertexBuffer<T>(int lenght) where T : struct
		{
			return new VulkanDataBuffer<T>(device, memory, (uint)lenght, BufferUsageFlags.VertexBuffer);
		}

		public IBuffer<T> CreateUniformBuffer<T>() where T : struct
		{
			return new VulkanDataBuffer<T>(device, memory, 1, BufferUsageFlags.UniformBuffer);
		}

		public void DestroyBuffer(IBuffer buffer)
		{
			VulkanDataBuffer vkBuffer = buffer as VulkanDataBuffer;
			if (vkBuffer != null)
			{
				vkBuffer.Destroy();
				vkBuffer = null;
			}
		}

		public uint[] LoadSpvFile(string path)
		{
			byte[] data = System.IO.File.ReadAllBytes(path);
			uint[] spv = new uint[data.Length / 4];

			System.Buffer.BlockCopy(data, 0, spv, 0, data.Length);

			return spv;
		}

		public IShader LoadShader(string fileName, string function, ShaderType type)
		{
			uint[] spv = LoadSpvFile(fileName);
			ShaderModule module = device.CreateShaderModule(new ShaderModuleCreateInfo
			{
				Code = spv,
			});
			PipelineShaderStageCreateInfo info = new PipelineShaderStageCreateInfo
			{
				Stage = VulkanHelper.ToShaderStageFlags(type),
				Name = function,
				Module = module,
			};
			return new VulkanShader(module, info);
		}

		public void DestroyShader(IShader shader)
		{
			VulkanShader vkShader = shader as VulkanShader;
			if(vkShader != null)
			{
				vkShader.Destroy(device);
			}
		}

		protected class DeviceQueueCreateInfoFactory
		{
			Dictionary<QueueFlags, DeviceQueueCreateInfo> queueCreateInfos = new Dictionary<QueueFlags, DeviceQueueCreateInfo>();

			public DeviceQueueCreateInfo[] QueueCreateInfos
			{
				get
				{
					DeviceQueueCreateInfo[] array = new DeviceQueueCreateInfo[queueCreateInfos.Count];					
					queueCreateInfos.Values.CopyTo(array, 0);
					foreach (DeviceQueueCreateInfo info in array)
					{
						info.QueuePriorities = new float[info.QueueCount];
					}
					return array;
				}
			}

			QueueFamilyProperties[] queueFamilyProperties;

			public DeviceQueueCreateInfoFactory(QueueFamilyProperties[] queueFamilyProperties)
			{
				this.queueFamilyProperties = queueFamilyProperties;
			}

			public DeviceQueueCreateInfoFactory(PhysicalDevice gpu) : this(gpu.GetQueueFamilyProperties())
			{

			}

			public void AddQueue(VulkanQueue queue)
			{
				QueueFlags target = queue.VulkanType &~ QueueFlags.SparseBinding;
				DeviceQueueCreateInfo info;
				// re-use existing family with same usage flags
				if (queueCreateInfos.TryGetValue(target, out info))
				{
					var prop = queueFamilyProperties[info.QueueFamilyIndex];
					if (prop.QueueCount > info.QueueCount)
					{
						queue.SetFamilyIndex(info.QueueFamilyIndex, info.QueueCount);
						info.QueueCount++;
					}
					else
					{
						// reuse first 
						queue.SetFamilyIndex(info.QueueFamilyIndex, 0);
					}
				}
				else
				{
					bool optimalSet = false;
					uint posibleIndex = uint.MaxValue;
					int posibleBitCount = int.MaxValue; 

					for (int i = 0; i < queueFamilyProperties.Length; i++)
					{
						var prop = queueFamilyProperties[i];
						QueueFlags propFlags = prop.QueueFlags & ~QueueFlags.SparseBinding;
						// Scan for Optimal
						if (propFlags == target)
						{
							uint familyIndex = (uint)i;
							queueCreateInfos.Add(target, new DeviceQueueCreateInfo
							{
								QueueCount = 1,
								QueueFamilyIndex = familyIndex,
							});
							queue.SetFamilyIndex(familyIndex, 0);
							optimalSet = true;
							break;
						}
						// Scan for Best Possible
						else if((propFlags & target) == target)
						{
							int bitCount = BitCount(propFlags);
							if (bitCount < posibleBitCount)
							{
								posibleIndex = (uint)i;
								posibleBitCount = bitCount;
							}
						}
					}

					if(!optimalSet)
					{
						if (posibleIndex != uint.MaxValue)
						{
							queueCreateInfos.Add(target, new DeviceQueueCreateInfo
							{
								QueueCount = 1,
								QueueFamilyIndex = posibleIndex,
							});
							queue.SetFamilyIndex(posibleIndex, 0);
						}
						else
						{
							throw new Exception("Unable to find QueueFamilyIndex for Flags : " + target);
						}
					}
				}
			}

			int BitCount(QueueFlags flags)
			{
				int i = 0;
				if((flags & QueueFlags.Graphics) == QueueFlags.Graphics)
				{
					i++;
				}
				if ((flags & QueueFlags.Compute) == QueueFlags.Compute)
				{
					i++;
				}
				if ((flags & QueueFlags.Transfer) == QueueFlags.Transfer)
				{
					i++;
				}
				return i;
			}
		}
	}
}
