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
            sample.InitSwapChain();
            sample.InitDepthBuffer();



            device.Destroy();
            sample.DestroyWindow();
            instance.Destroy();
        }
    }
}
