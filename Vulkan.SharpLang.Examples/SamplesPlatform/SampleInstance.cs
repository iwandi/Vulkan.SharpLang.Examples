using System;
using Vulkan;
using Vulkan.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace Vulkan.SharpLang.Examples
{
    public class SampleInstance
    {
        const ulong FENCE_TIMEOUT =  100000000;

        class LayerPropertiesInfo 
        {
            public Vulkan.LayerProperties Properties;
            public ExtensionProperties[] Extensions;
        }

        LayerProperties[] instanceLayerProperties;

        public void InitGlobalLayerProperties()
        {
            LayerProperties[] props = Commands.EnumerateInstanceLayerProperties();
            instanceLayerProperties = new LayerProperties[props.Length];
            foreach (LayerProperties prop in props)
            {
                LayerPropertiesInfo layerProps = new LayerPropertiesInfo();
                layerProps.Properties = prop;
                layerProps.Extensions = Commands.EnumerateInstanceExtensionProperties(prop.LayerName);
            }
        }

        string[] instanceExtensionNames = new string[0];
        string[] instanceLayerNames = new string[0];

        public void InitInstanceeExtensionNames()
        {
            instanceExtensionNames = new string[]
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface",
                "VK_EXT_debug_report",
            };

            instanceLayerNames = new string[]
            {
                "VK_LAYER_LUNARG_standard_validation",
            };
        }

        string appShortName;
        Instance instance;
        public Instance Instance {  get { return instance; } }

        public Instance InitInstance(string appShortName)
        {
            this.appShortName = appShortName;
            uint apiVersion = Version.Make(1, 0, 0);

            ApplicationInfo appInfo = new ApplicationInfo
            {
                ApplicationName = appShortName,
                ApplicationVersion = 1,
                EngineName = appShortName,
                EngineVersion = 1,
                ApiVersion = apiVersion,
            };

            InstanceCreateInfo createInfo = new InstanceCreateInfo
            {
                ApplicationInfo = appInfo,
                EnabledExtensionNames = instanceExtensionNames,
                EnabledLayerNames = instanceLayerNames,
            };

            instance = new Instance(createInfo);
            return instance;
        }
        
        PhysicalDevice gpu;
        public PhysicalDevice Gpu { get { return gpu; } }

        QueueFamilyProperties[] queueProps;
        public QueueFamilyProperties[] QueueProps {  get { return queueProps; } }
        public int QueueCount {  get { return queueProps.Length; } }
        PhysicalDeviceMemoryProperties memoryProperties;
        PhysicalDeviceProperties gpuProps;

        uint graphicsQueueFamilyIndex;
        public uint GraphicsQueueFamilyIndex {  get { return graphicsQueueFamilyIndex; } }

        public PhysicalDevice InitEnumerateDevice() 
        {
            PhysicalDevice[] gpus = instance.EnumeratePhysicalDevices();
            gpu = gpus[0];

            queueProps = gpu.GetQueueFamilyProperties();
            memoryProperties = gpu.GetMemoryProperties();
            gpuProps = gpu.GetProperties();

            return gpu;
        }

        string[] deviceExtensionNames = new string[0];
        string[] deviceLayerNames = new string[0];

        public void InitDeviceExtensionNames()
        {
            deviceExtensionNames = new string[]
            {
                "VK_KHR_swapchain",
            };

            deviceLayerNames = new string[]
            {
                "VK_LAYER_LUNARG_standard_validation",
            };
        }

        Device device;
        public Device Device {  get { return device; } }

        public Device InitDevice()
        {
            DeviceQueueCreateInfo queueInfo = new DeviceQueueCreateInfo
            {
                QueueCount = 1,
                QueueFamilyIndex = graphicsQueueFamilyIndex,
                QueuePriorities = new float[] { 0.0f },
            };

            DeviceCreateInfo info = new DeviceCreateInfo
            {
                QueueCreateInfos = new DeviceQueueCreateInfo[] { queueInfo },
                EnabledLayerNames = deviceLayerNames,
                EnabledExtensionNames = deviceExtensionNames, 
            };

            device = gpu.CreateDevice(info);
            return device;
        }

        public void InitQueueFamilyIndex()
        {
            queueProps = gpu.GetQueueFamilyProperties();

            bool found = false;
            for(uint i = 0; i < queueProps.Length; i++)
            {
                if((queueProps[i].QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics)
                {
                    graphicsQueueFamilyIndex = i;
                    found = true;
                    break;
                }
            }
            Debug.Assert(found);
        }

        uint width;
        public uint Width { get { return width; } }
        uint height;
        public uint Height { get { return height; } }

        public void InitWindowSize(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }

        public void InitConnection()
        {
            // Noting on windows
        }

        IntPtr connection;
        public IntPtr Connection {  get { return connection; } }
        Form window; // use intPtr or keep using form ?

        public IntPtr InitWindow()
        {
            connection = System.Runtime.InteropServices.Marshal.GetHINSTANCE(this.GetType().Module);

			Size size = new Size((int)width, (int)height);

			window = new Form
			{
				Name = appShortName,
				Text = appShortName,
				/*Width = size.Width,
				Height = size.Height,
				AutoSize = false,
				MinimumSize = size,
				MaximumSize = size,*/
				ClientSize = size,
			};			

            window.Show();

			return window.Handle;
        }

        public void DestroyWindow()
        {
            window.Close();
            window.Dispose();
            window = null;
        }

        SurfaceKhr surface;
        SurfaceFormatKhr[] surfFormats;
        Format format;

        public Format Format { get { return format; } }
		
		public void InitSwapChainExtension()
        {
            Win32SurfaceCreateInfoKhr createInfo = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = connection,
                Hwnd = window.Handle,
            };

            surface = instance.CreateWin32SurfaceKHR(createInfo);

            // Iterate over each queue to learn whether it supports presenting:
            bool[] supportsPresent = new bool[QueueCount];
            for (uint i = 0; i < supportsPresent.Length; i++)
            {
                supportsPresent[i] = gpu.GetSurfaceSupportKHR(i, surface);
            }

            // Search for a graphics queue and a present queue in the array of queue
            // families, try to find one that supports both
            uint graphicsQueueNodeIndex = uint.MaxValue;
            for (uint i = 0; i < QueueCount; i++)
            {
                if ((queueProps[i].QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics &&
                    supportsPresent[i])
                {
                    graphicsQueueNodeIndex = i;
                    break;
                }
            }

            // Generate error if could not find a queue that supports both a graphics
            // and present
            if (graphicsQueueNodeIndex == uint.MaxValue)
            {
                Console.WriteLine("Could not find a queue that supports both graphics and present");
                throw new Exception("Could not find a queue that supports both graphics and present");
            }

            graphicsQueueFamilyIndex = graphicsQueueNodeIndex;

            // Get the list of VkFormats that are supported:
            surfFormats = gpu.GetSurfaceFormatsKHR(surface);

            // If the format list includes just one entry of VK_FORMAT_UNDEFINED,
            // the surface has no preferred format.  Otherwise, at least one
            // supported format will be returned.
            if (surfFormats.Length == 1 && surfFormats[0].Format == Format.Undefined)
            {
                format = Format.B8G8R8A8Unorm;
            }
            else
            {
                Debug.Assert(surfFormats.Length >= 1);
                format = surfFormats[0].Format;
            }
        }

		public void DestroySwapChainExtension()
		{
			instance.DestroySurfaceKHR(surface);
			surface = null;
		}

		public class SwapChainBuffer
        {
            public Image Image;
            public ImageView view;
        }

		Extent2D swapChainExtend;
		SwapchainKhr swapChain;
        SwapChainBuffer[] buffers;
        uint currentBuffer;

        public SwapchainKhr SwapChain { get { return swapChain; } }
        public SwapChainBuffer[] Buffers { get { return buffers; } }
        public uint CurrentBuffer { get { return currentBuffer; } set { currentBuffer = value; } }

        public SwapchainKhr InitSwapChain()
        {
            SurfaceCapabilitiesKhr surfCapabilities = gpu.GetSurfaceCapabilitiesKHR(surface);
            PresentModeKhr[] presentModes = gpu.GetSurfacePresentModesKHR(surface);

            swapChainExtend = new Extent2D();
            // width and height are either both -1, or both not -1.
            if (surfCapabilities.CurrentExtent.Width == uint.MaxValue)
            {
                // If the surface size is undefined, the size is set to
                // the size of the images requested.
                swapChainExtend.Width = Width;
                swapChainExtend.Height = Height;
            }
            else
            {
                // If the surface size is defined, the swap chain size must match
                swapChainExtend = surfCapabilities.CurrentExtent;
            }

            // If mailbox mode is available, use it, as is the lowest-latency non-
            // tearing mode.  If not, try IMMEDIATE which will usually be available,
            // and is fastest (though it tears).  If not, fall back to FIFO which is
            // always available.
            PresentModeKhr swapChainPresentMode = PresentModeKhr.Fifo;
            foreach (PresentModeKhr presentMode in presentModes)
            {
                if (presentMode == PresentModeKhr.Mailbox)
                {
                    swapChainPresentMode = PresentModeKhr.Mailbox;
                    break;
                }
                else if (presentMode == PresentModeKhr.Immediate)
                {
                    swapChainPresentMode = PresentModeKhr.Immediate;
                    break;
                }
            }

            // Determine the number of VkImage's to use in the swap chain (we desire to
            // own only 1 image at a time, besides the images being displayed and
            // queued for display):
            uint desiredNumberOfSwapChainImages = surfCapabilities.MinImageCount + 1;
            if (surfCapabilities.MaxImageCount > 0 &&
                desiredNumberOfSwapChainImages > surfCapabilities.MaxImageCount)
            {
                desiredNumberOfSwapChainImages = surfCapabilities.MaxImageCount;
            }

            SurfaceTransformFlagsKhr preTransform;
            if ((surfCapabilities.SupportedTransforms & SurfaceTransformFlagsKhr.Identity) == SurfaceTransformFlagsKhr.Identity)
            {
                preTransform = SurfaceTransformFlagsKhr.Identity;
            }
            else
            {
                preTransform = surfCapabilities.CurrentTransform;
            }

			SwapchainCreateInfoKhr swapChainInfo = new SwapchainCreateInfoKhr
			{
				Surface = surface,
				MinImageCount = desiredNumberOfSwapChainImages,
				ImageFormat = format,
				ImageExtent = swapChainExtend,
				PreTransform = preTransform,
				CompositeAlpha = CompositeAlphaFlagsKhr.Opaque,
				ImageArrayLayers = 1,
				PresentMode = swapChainPresentMode,
				OldSwapchain = null,
                Clipped = true,
                ImageColorSpace = ColorSpaceKhr.SrgbNonlinear,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                ImageSharingMode = SharingMode.Exclusive,
                QueueFamilyIndices = new uint[0],
            };

            swapChain = device.CreateSwapchainKHR(swapChainInfo);
            Image[] swapChainImages = device.GetSwapchainImagesKHR(swapChain);

            buffers = new SwapChainBuffer[swapChainImages.Length];
            for (uint i = 0; i < swapChainImages.Length; i++)
            {
                SwapChainBuffer scBuffer = new SwapChainBuffer
                {
                    Image = swapChainImages[i],
                };

                ImageViewCreateInfo colorImageView = new ImageViewCreateInfo
                {
                    Flags = 0,
                    Image = swapChainImages[i],
                    ViewType = ImageViewType.View2D,
                    Format = format,
                    Components = new ComponentMapping
                    {
                        R = ComponentSwizzle.R,
                        G = ComponentSwizzle.G,
                        B = ComponentSwizzle.B,
                        A = ComponentSwizzle.A,
                    },
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.Color,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1,
                    },
                };

                scBuffer.view = device.CreateImageView(colorImageView);

                buffers[i] = scBuffer;
            }
            
            currentBuffer = 0;
            return swapChain;
        }

        public void DestroySwapChain()
        {
            foreach(SwapChainBuffer buffer in buffers)
            {
                device.DestroyImageView(buffer.view);
            }
            device.DestroySwapchainKHR(swapChain);
			instance.DestroySurfaceKHR(surface);
        }

        Queue queue;
        public Queue Queue {  get { return queue; } }

        public Queue InitDeviceQueue()
        {
            queue = device.GetQueue(graphicsQueueFamilyIndex, 0);
            return queue;
        }

        Format depthFormat;
        Image depthImage;
		public Image DepthImage {  get { return depthImage; } }
        ImageView depthView;
		public ImageView DepthView { get { return depthView; } }
		DeviceMemory depthMem;

        public Format DepthFormat {  get { return depthFormat; } }

        public void InitDepthBuffer()
        {
            if(this.depthFormat == Format.Undefined)
            {
                this.depthFormat = Format.D16Unorm;
            }

            Format depthFormat = this.depthFormat;
            FormatProperties props = gpu.GetFormatProperties(depthFormat);
            ImageTiling tiling;
            if ((props.LinearTilingFeatures & FormatFeatureFlags.DepthStencilAttachment) == FormatFeatureFlags.DepthStencilAttachment)
            {
                tiling = ImageTiling.Linear;
            }
            else if ((props.OptimalTilingFeatures & FormatFeatureFlags.DepthStencilAttachment) == FormatFeatureFlags.DepthStencilAttachment)
            {
                tiling = ImageTiling.Optimal;
            }
            else
            {
                /* Try other depth formats? */
                Console.WriteLine("VK_FORMAT_D16_UNORM Unsupported.");
                throw new Exception("VK_FORMAT_D16_UNORM Unsupported.");
            }

            ImageCreateInfo imageInfo = new ImageCreateInfo
            {
                Tiling = tiling,
                ImageType = ImageType.Image2D,
                Format = depthFormat,
                Extent = new Extent3D
                {
                    Width = swapChainExtend.Width,
                    Height = swapChainExtend.Height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCountFlags.Count1,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.DepthStencilAttachment,
                QueueFamilyIndices = new uint[0],
                SharingMode = SharingMode.Exclusive,
                Flags = 0,
            };

            MemoryAllocateInfo memAlloc = new MemoryAllocateInfo
            {
                AllocationSize = 0,
                MemoryTypeIndex = 0,
            };

            ImageAspectFlags aspectMask = ImageAspectFlags.Depth;
            if (depthFormat == Format.D16UnormS8Uint ||
                depthFormat == Format.D24UnormS8Uint ||
                depthFormat == Format.D32SfloatS8Uint)
            {
                aspectMask |= ImageAspectFlags.Stencil;
            }

            ImageViewCreateInfo viewInfo = new ImageViewCreateInfo
            {
                Image = null,
                Format = depthFormat,
                Components = new ComponentMapping
                {
                    R = ComponentSwizzle.R,
                    G = ComponentSwizzle.G,
                    B = ComponentSwizzle.B,
                    A = ComponentSwizzle.A,
                },
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = aspectMask,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
                ViewType = ImageViewType.View2D,
                Flags = 0,
            };

            /* Create image */
            depthImage = device.CreateImage(imageInfo);
            MemoryRequirements memReqs = device.GetImageMemoryRequirements(depthImage);
            memAlloc.AllocationSize = memReqs.Size;

            /* Use the memory properties to determine the type of memory required */
            uint memoryTypeIndex;
            bool pass = MemoryTypeFromProperties(memReqs.MemoryTypeBits, (MemoryPropertyFlags)0, out memoryTypeIndex);
            memAlloc.MemoryTypeIndex = memoryTypeIndex;

            Debug.Assert(pass);

            /* Allocate memory */
            depthMem = device.AllocateMemory(memAlloc);

            /* Bind memory */
            device.BindImageMemory(depthImage, depthMem, 0);

            /* Set the image layout to depth stencil optimal */
            SetImageLayout(depthImage, viewInfo.SubresourceRange.AspectMask, ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal);

            /* Create image view */
            viewInfo.Image = depthImage;
            depthView = device.CreateImageView(viewInfo);
        }

        public void DestroyDepthBuffer()
        {
            device.DestroyImageView(depthView);
            device.DestroyImage(depthImage);
            device.FreeMemory(depthMem);
        }

		RenderPass renderPass;
		public RenderPass RenderPass { get { return renderPass; } }

		public RenderPass InitRenderPass(bool includeDepth, bool clear = true, ImageLayout finalLayout = ImageLayout.PresentSrcKhr)
		{
			AttachmentDescription[] attachments = new AttachmentDescription[includeDepth ? 2 : 1];
			attachments[0] = new AttachmentDescription
			{
				Format = format,
				Samples = SampleCountFlags.Count1,
				LoadOp = clear ? AttachmentLoadOp.Clear : AttachmentLoadOp.DontCare,
				StoreOp = AttachmentStoreOp.Store,
				StencilLoadOp = AttachmentLoadOp.DontCare,
				StencilStoreOp = AttachmentStoreOp.DontCare,
				InitialLayout = ImageLayout.Undefined,
				FinalLayout = finalLayout,
				Flags = 0,

			};
			if(includeDepth)
			{
				attachments[1] = new AttachmentDescription
				{
					Format = depthFormat,
					Samples = SampleCountFlags.Count1,
					LoadOp = clear ? AttachmentLoadOp.Clear : AttachmentLoadOp.DontCare,
					StoreOp = AttachmentStoreOp.Store,
					StencilLoadOp = AttachmentLoadOp.Load,
					StencilStoreOp = AttachmentStoreOp.Store,
					InitialLayout = ImageLayout.Undefined,
					FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
					Flags = 0,
				};
			}

			AttachmentReference colorReference = new AttachmentReference
			{
				Attachment = 0,
				Layout = ImageLayout.ColorAttachmentOptimal,
			};			

			SubpassDescription subPass = new SubpassDescription
			{
				ColorAttachments = new AttachmentReference[] { colorReference },
				PipelineBindPoint = PipelineBindPoint.Graphics,
			};

			if(includeDepth)
			{
				AttachmentReference depthReference = new AttachmentReference
				{
					Attachment = 1,
					Layout = ImageLayout.DepthStencilAttachmentOptimal,
				};
				subPass.DepthStencilAttachment = depthReference;
			}

			renderPass = device.CreateRenderPass(new RenderPassCreateInfo
			{
				Attachments = attachments,
				Subpasses = new SubpassDescription[] { subPass },
			});

			return renderPass;
		}

		public void DestroyRenderPass()
		{
			device.DestroyRenderPass(renderPass);
			renderPass = null;
		}

        CommandPool cmdPool;

        public CommandPool InitCommandPool()
        {
            CommandPoolCreateInfo cmdPoolInfo = new CommandPoolCreateInfo
            {
                QueueFamilyIndex = graphicsQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.ResetCommandBuffer,
            };

            cmdPool = device.CreateCommandPool(cmdPoolInfo);
            return cmdPool;
        }

        public void DestroyCommandPool()
        {
            device.DestroyCommandPool(cmdPool);
        }

        CommandBuffer cmd;

        public CommandBuffer InitCommandBuffer()
        {
            CommandBufferAllocateInfo cmdInfo = new CommandBufferAllocateInfo
            {
                CommandPool = cmdPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            };

            CommandBuffer[] buffers =  device.AllocateCommandBuffers(cmdInfo);
            cmd = buffers[0];
            return cmd;
        }

        public void DestroyCommandBuffer()
        {
            device.FreeCommandBuffers(cmdPool, new CommandBuffer[] { cmd });
        }

        public void ExecuteBeginCommandBuffer()
        {
            CommandBufferBeginInfo cmdBufInfo = new CommandBufferBeginInfo
            {
                Flags = 0,
                InheritanceInfo = new CommandBufferInheritanceInfo(),
            };

            cmd.Begin(cmdBufInfo);
        }

        public void ExecuteEndCommandBuffer()
        {
            cmd.End();
        }

        public void ExecuteQueueCommandBuffer()
        {
			// TODO : fix this

            FenceCreateInfo fenceInfo = new FenceCreateInfo
            {
            };
            Fence drawFence = device.CreateFence(fenceInfo);

            PipelineStageFlags pipeStageFlags = PipelineStageFlags.ColorAttachmentOutput;
			SubmitInfo submitInfo = new SubmitInfo
			{
				//WaitSemaphores = new Semaphore[] { null }, // TODO sempafore from InitSwapChain
                WaitDstStageMask = new PipelineStageFlags[] { pipeStageFlags },
                CommandBuffers = new CommandBuffer[] { cmd },            
            };
			submitInfo.WaitSemaphoreCount = 0;

            queue.Submit(new SubmitInfo[] { submitInfo }, drawFence);

            Result res = Result.Timeout;
            while(res == Result.Timeout)
            {
                try
                {
                    device.WaitForFences(new Fence[] { drawFence }, true, FENCE_TIMEOUT);
                    break;
                }
                catch (ResultException re)
                {
                    if (re.Result == Result.Timeout)
                    {
                        res = re.Result;
                    }
                    else if( re.Result != Result.Success)
                    {
                        throw new Exception("Failed at device.WaitForFences", re);
                    }
                }
            }

            device.DestroyFence(drawFence);
        }

        public bool MemoryTypeFromProperties(uint typeBits, MemoryPropertyFlags requirementsMask, out uint typeIndex)
        {
            // Search memtypes to find first index with those properties
            for(uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if ((typeBits & 1) == 1)
                {
                    // Type is available, does it match user properties?
                    if ((memoryProperties.MemoryTypes[i].PropertyFlags & requirementsMask) == requirementsMask)
                    {
                        typeIndex = i;
                        return true;
                    }
                }
                typeBits >>= 1;
            }
            typeIndex = 0;
            return false;
        }

        public void SetImageLayout(Image image, ImageAspectFlags aspectMask, ImageLayout oldImageLayout, ImageLayout newImageLayout)
        {
            ImageMemoryBarrier imageMemoryBarrier = new ImageMemoryBarrier
            {
                SrcAccessMask = 0,
                DstAccessMask = 0,
                OldLayout = oldImageLayout,
                NewLayout = newImageLayout,
                SrcQueueFamilyIndex = 0,
                DstQueueFamilyIndex = 0,
                Image = image,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = aspectMask,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
            };

            if(oldImageLayout == ImageLayout.ColorAttachmentOptimal)
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.ColorAttachmentWrite;
            }

            if(newImageLayout == ImageLayout.TransferDstOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.TransferWrite;
            }

            if(newImageLayout == ImageLayout.TransferSrcOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.TransferRead;
            }

            if(oldImageLayout == ImageLayout.TransferDstOptimal)
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.TransferWrite;
            }

            if(oldImageLayout == ImageLayout.Preinitialized)
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.HostWrite;
            }

            if(newImageLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.ShaderRead;
            }

            if(newImageLayout == ImageLayout.ColorAttachmentOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.ColorAttachmentWrite;
            }

            if(newImageLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.DepthStencilAttachmentWrite;
            }

            PipelineStageFlags srcStages = PipelineStageFlags.TopOfPipe;
            PipelineStageFlags destStages = PipelineStageFlags.TopOfPipe;

            cmd.CmdPipelineBarrier(srcStages, destStages, (DependencyFlags)0, null, null, new ImageMemoryBarrier[] { imageMemoryBarrier });
        }

		public uint[] LoadSpvFile(string path)
		{
			byte[] data = System.IO.File.ReadAllBytes(path);
			uint[] spv = new uint[data.Length / 4];

			System.Buffer.BlockCopy(data, 0, spv, 0, data.Length);

			return spv;
		}
    }
}
