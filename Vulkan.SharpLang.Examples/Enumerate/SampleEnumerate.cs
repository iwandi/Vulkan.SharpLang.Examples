using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    class SampleEnumerate
    {
        static void Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            Instance instance = sample.InitInstance("vulkansamples_enumerate");

            PhysicalDevice[] gpus = instance.EnumeratePhysicalDevices();

            instance.Destroy();
        }
    }
}
