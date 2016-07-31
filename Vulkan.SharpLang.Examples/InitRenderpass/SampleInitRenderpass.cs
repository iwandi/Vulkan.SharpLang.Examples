using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    class SampleInitRenderpass
    {
        static void Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            sample.InitGlobalLayerProperties();
            sample.InitInstanceeExtensionNames();
            sample.InitDeviceExtensionNames();
            Instance instance = sample.InitInstance("Renderpass Sample");
            sample.InitEnumerateDevice();
            sample.InitConnection();
            sample.InitWindowSize(50, 50);
            sample.InitWindow();
            sample.InitSwapChainExtension();
            Device device = sample.InitDevice();
            sample.InitCommandPool();
            sample.InitCommandBuffer();
            sample.ExecuteBeginCommandBuffer();
            sample.InitDeviceQueue();
            SwapchainKhr swapChain = sample.InitSwapChain();
            sample.InitDepthBuffer();

            /* VULKAN_KEY_START */

            // A semaphore (or fence) is required in order to acquire a
            // swapchain image to prepare it for use in a renderpass.
            // The semaphore is normally used to hold back the rendering
            // operation until the image is actually available.
            // But since this sample does not render, the semaphore
            // ends up being unused.

            Semaphore imageAcquiredSemaphore;
            SemaphoreCreateInfo imageAcquiredSemaphoreInfo = new SemaphoreCreateInfo
            {
            };
            imageAcquiredSemaphore = device.CreateSemaphore(imageAcquiredSemaphoreInfo);

            // Acquire the swapchain image in order to set its layout
            sample.CurrentBuffer = device.AcquireNextImageKHR(swapChain, uint.MaxValue, imageAcquiredSemaphore, new Fence());

            // Set the layout for the color buffer, transitioning it from
            // undefined to an optimal color attachment to make it usable in
            // a renderpass.
            // The depth buffer layout has already been set by init_depth_buffer().
            sample.SetImageLayout(sample.Buffers[sample.CurrentBuffer].Image, ImageAspectFlags.Color, ImageLayout.Undefined, ImageLayout.ColorAttachmentOptimal);

            /* Need attachments for render target and depth buffer */
            AttachmentDescription[] attachments = new AttachmentDescription[]
            {
                new AttachmentDescription
                {
                    Format = sample.Format,
                    Samples = SampleCountFlags.Count1,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.ColorAttachmentOptimal,
                    FinalLayout = ImageLayout.ColorAttachmentOptimal,
                    Flags = 0,

                },
                new AttachmentDescription
                {
                    Format = sample.DepthFormat,
                    Samples = SampleCountFlags.Count1,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.DontCare,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.DepthStencilAttachmentOptimal,
                    FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
                    Flags = 0,
                }
            };

            AttachmentReference colorReference = new AttachmentReference
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            AttachmentReference depthReference = new AttachmentReference
            {
                Attachment = 1,
                Layout = ImageLayout.DepthStencilAttachmentOptimal,
            };

            SubpassDescription subPass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                Flags = 0,
                InputAttachments = new AttachmentReference[0],
                ColorAttachments = new AttachmentReference[] { colorReference },
                DepthStencilAttachment = depthReference,
                PreserveAttachments = new uint[0],
            };

            RenderPassCreateInfo rpInfo = new RenderPassCreateInfo
            {
                Attachments = attachments,
                Subpasses = new SubpassDescription[] { subPass },
                Dependencies = new SubpassDependency[0],
            };

            RenderPass renderPass = device.CreateRenderPass(rpInfo);
            sample.ExecuteEndCommandBuffer();
            sample.ExecuteQueueCommandBuffer();
            /* VULKAN_KEY_END */

            device.DestroyRenderPass(renderPass);
            device.DestroySemaphore(imageAcquiredSemaphore);
            sample.DestroyDepthBuffer();
            sample.DestroySwapChain();
            sample.DestroyCommandBuffer();
            sample.DestroyCommandPool();
            device.Destroy();
            sample.DestroyWindow();
            instance.Destroy();
        }
    }
}
