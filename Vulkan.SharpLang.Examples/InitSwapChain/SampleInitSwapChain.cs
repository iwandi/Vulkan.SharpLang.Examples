using System;
using System.Diagnostics;
using Vulkan;
using Vulkan.Windows;

namespace Vulkan.SharpLang.Examples
{
    class SampleInitSwapChain
    {
        static int Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            sample.InitGlobalLayerProperties();
            sample.InitInstanceeExtensionNames();
            sample.InitDeviceExtensionNames();
            Instance instance = sample.InitInstance("Swapchain Initialization Sample");
            PhysicalDevice gpu = sample.InitEnumerateDevice();
            sample.InitWindowSize(50, 50);
            sample.InitConnection();
            IntPtr windowHandle = sample.InitWindow();

            Win32SurfaceCreateInfoKhr createInfo = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = sample.Connection,
                Hwnd = windowHandle,
            };

            SurfaceKhr surface = instance.CreateWin32SurfaceKHR(createInfo);

            // Iterate over each queue to learn whether it supports presenting:
            bool[] supportsPresent = new bool[sample.QueueCount];
            for(uint i = 0; i < supportsPresent.Length; i++)
            {
                supportsPresent[i] = gpu.GetSurfaceSupportKHR(i, surface);
            }

            // Search for a graphics queue and a present queue in the array of queue
            // families, try to find one that supports both
            uint graphicsQueueNodeIndex = uint.MaxValue;
            for(uint i = 0; i < sample.QueueCount; i++)
            {
                if((sample.QueueProps[i].QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics &&
                    supportsPresent[i])
                {
                    graphicsQueueNodeIndex = i;
                    break;
                }
            }

            // Generate error if could not find a queue that supports both a graphics
            // and present
            if(graphicsQueueNodeIndex == uint.MaxValue)
            {
                Console.WriteLine("Could not find a queue that supports both graphics and present");
                return -1;
            }

            Device device = sample.InitDevice();

            // Get the list of VkFormats that are supported:
            SurfaceFormatKhr[] surfFormats = gpu.GetSurfaceFormatsKHR(surface);

            Format format;
            // If the format list includes just one entry of VK_FORMAT_UNDEFINED,
            // the surface has no preferred format.  Otherwise, at least one
            // supported format will be returned.
            if(surfFormats.Length == 1 && surfFormats[0].Format == Format.Undefined)
            {
                format = Format.B8G8R8A8Unorm;
            }
            else
            {
                Debug.Assert(surfFormats.Length >= 1);
                format = surfFormats[0].Format;
            }

            SurfaceCapabilitiesKhr surfCapabilities = gpu.GetSurfaceCapabilitiesKHR(surface);
            PresentModeKhr[] presentModes =  gpu.GetSurfacePresentModesKHR(surface);

            Extent2D swapChainExtend = new Extent2D();
            // width and height are either both -1, or both not -1.
            if (surfCapabilities.CurrentExtent.Width == uint.MaxValue)
            {
                // If the surface size is undefined, the size is set to
                // the size of the images requested.
                swapChainExtend.Width = sample.Width;
                swapChainExtend.Height = sample.Height;
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
            foreach(PresentModeKhr presentMode in presentModes)
            {
                if(presentMode == PresentModeKhr.Mailbox)
                {
                    swapChainPresentMode = PresentModeKhr.Mailbox;
                    break;
                }
                else if(presentMode == PresentModeKhr.Immediate)
                {
                    swapChainPresentMode = PresentModeKhr.Immediate;
                    break;
                }
            }

            // Determine the number of VkImage's to use in the swap chain (we desire to
            // own only 1 image at a time, besides the images being displayed and
            // queued for display):
            uint desiredNumberOfSwapChainImages = surfCapabilities.MinImageCount + 1;
            if(surfCapabilities.MaxImageCount > 0 && 
                desiredNumberOfSwapChainImages > surfCapabilities.MaxImageCount)
            {
                desiredNumberOfSwapChainImages = surfCapabilities.MaxImageCount;
            }

            SurfaceTransformFlagsKhr preTransform;
            if((surfCapabilities.SupportedTransforms & SurfaceTransformFlagsKhr.Identity) == SurfaceTransformFlagsKhr.Identity)
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

            SwapchainKhr swapChain = device.CreateSwapchainKHR(swapChainInfo);
            Image[] swapChainImages = device.GetSwapchainImagesKHR(swapChain);

            ImageView[] views = new ImageView[swapChainImages.Length];
            for (uint i = 0; i < swapChainImages.Length; i++)
            {
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

                views[i] = device.CreateImageView(colorImageView);                
            }

            /* VULKAN_KEY_END */
            
            /* Clean Up */
            foreach(ImageView view in views)
            {
                device.DestroyImageView(view);
            }
            device.DestroySwapchainKHR(swapChain);
            device.Destroy();
            sample.DestroyWindow();
            instance.Destroy();

            return 0;
        }
    }
}
