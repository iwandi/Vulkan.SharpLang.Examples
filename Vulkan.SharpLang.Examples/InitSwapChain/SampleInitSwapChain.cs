using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    class SampleInitSwapChain
    {
        static void Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            sample.InitGlobalLayerProperties();
            sample.InitInstanceeExtensionNames();
            sample.InitDeviceExtensionNames();
            sample.InitInstance("Swapchain Initialization Sample");
            sample.InitEnumerateDevice();
            sample.InitWindowSize(50, 50);
            sample.InitConnection();
            sample.InitWindow();
        }
    }
}
