using System;
using System.Diagnostics;
using Vulkan;


namespace Vulkan.SharpLang.Examples
{
    class SampleInitCommandBuffer
    {
        static void Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            Instance instance = sample.InitInstance("Command Buffer Sample");

            sample.InitEnumerateDevice();
            Device device = sample.InitDevice();
            sample.InitQueueFamilyIndex();

            CommandPoolCreateInfo cmdPoolInfo = new CommandPoolCreateInfo
            {
                QueueFamilyIndex = sample.GraphicsQueueFamilyIndex,
                Flags = 0,
            };

            CommandPool cmdPool = device.CreateCommandPool(cmdPoolInfo);

            CommandBufferAllocateInfo cmd = new CommandBufferAllocateInfo
            {
                CommandPool = cmdPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            };

            CommandBuffer[] cmdBufs = device.AllocateCommandBuffers(cmd);

            device.FreeCommandBuffers(cmdPool, cmdBufs);
            device.DestroyCommandPool(cmdPool);
            device.Destroy();
            instance.Destroy();
        }
    }
}
