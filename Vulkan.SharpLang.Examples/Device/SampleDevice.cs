using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    class SampleDevice
    {
        static void Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            Instance instance = sample.InitInstance("vulkansamples_device");

            PhysicalDevice gpu = sample.InitEnumerateDevice();

            Debug.Assert(gpu != null);
            
            QueueFamilyProperties[] queueProps = gpu.GetQueueFamilyProperties();
            Debug.Assert(queueProps != null && queueProps.Length >= 1);

            bool found = false;
            foreach (QueueFamilyProperties prop in queueProps)
            {
                if((prop.QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics)
                {
                    found = true;
                    break;
                }
            }

            Debug.Assert(found);

            DeviceQueueCreateInfo queueInfo = new DeviceQueueCreateInfo
            {
                QueueCount = 1,
                QueueFamilyIndex = 0,
                QueuePriorities = new float[] { 0.0f },
            };

            DeviceCreateInfo info = new DeviceCreateInfo
            {
                QueueCreateInfos = new DeviceQueueCreateInfo[]{ queueInfo },
                EnabledLayerNames = new string[0],
                EnabledExtensionNames = new string[0],
            };

            Device device = gpu.CreateDevice(info);

            instance.Destroy();
        }
    }
}
