using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    class SampleDepthBuffer
    {
        static void Main(string[] args)
        {
            /*
             * Make a depth buffer:
             * - Create an Image to be the depth buffer
             * - Find memory requirements
             * - Allocate and bind memory
             * - Set the image layout
             * - Create an attachment view
             */

            SampleInstance sample = new SampleInstance();
            sample.InitGlobalLayerProperties();
            sample.InitInstanceeExtensionNames();
            sample.InitDeviceExtensionNames();
            Instance instance = sample.InitInstance("Depth Buffer Sample");
            PhysicalDevice gpu = sample.InitEnumerateDevice();
            sample.InitWindowSize(500, 500);
            sample.InitConnection();
            sample.InitWindow();
            sample.InitSwapChainExtension();
            Device device = sample.InitDevice();

            /* VULKAN_KEY_START */
            const Format depthFormat = Format.D16Unorm;
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
                    Width = sample.Width,
                    Height = sample.Height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCountFlags.Count4, // TODO NUM_SAMPLES ???
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
                    AspectMask = ImageAspectFlags.Depth,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
                ViewType = ImageViewType.View2D,
                Flags = 0,
            };

            /* Create image */
            Image depthImage = device.CreateImage(imageInfo);
            MemoryRequirements memReqs = device.GetImageMemoryRequirements(depthImage);

            /* Use the memory properties to determine the type of memory required */
            uint memoryTypeIndex;
            bool pass = sample.MemoryTypeFromProperties(memReqs.MemoryTypeBits, (MemoryPropertyFlags)0, out memoryTypeIndex);
            memAlloc.MemoryTypeIndex = memoryTypeIndex;

            Debug.Assert(pass);

            /* Allocate memory */
            DeviceMemory depthMem = device.AllocateMemory(memAlloc);

            /* Bind memory */
            device.BindImageMemory(depthImage, depthMem, 0);

            /* Create image view */
            viewInfo.Image = depthImage;
            ImageView depthView = device.CreateImageView(viewInfo);

            /* VULKAN_KEY_END */

            /* Clean Up */

            device.DestroyImageView(depthView);
            device.DestroyImage(depthImage);
            device.FreeMemory(depthMem);
            device.Destroy();
            sample.DestroyWindow();
            instance.Destroy();            
        }
    }
}
