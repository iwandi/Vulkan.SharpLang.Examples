using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    public class SampleInstance
    {
        Instance instance;
        public Instance Instance {  get { return instance; } }

        public Instance InitInstance(string appShortName)
        {
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
            };

            instance = new Instance(createInfo);
            return instance;
        }

        PhysicalDevice[] gpus;
        public PhysicalDevice[] Gpus {  get {  return gpus; } }

        uint graphicsQueueFamilyIndex; // TODO : Set this
        public uint GraphicsQueueFamilyIndex {  get { return graphicsQueueFamilyIndex; } }

        public PhysicalDevice[] InitEnumerateDevice()
        {
            gpus = instance.EnumeratePhysicalDevices();
            return gpus;
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
                EnabledLayerNames = new string[0], // TODO Enabled Layers
                EnabledExtensionNames = new string[0], // TODO Enabled Extentions
            };

            device = gpus[0].CreateDevice(info);
            return device;
        }

        public void QueueFamilyIndex()
        {
            // TODO 
        }
    }
}
