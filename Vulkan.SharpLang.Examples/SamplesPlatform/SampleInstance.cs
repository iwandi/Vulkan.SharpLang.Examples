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

        public PhysicalDevice[] InitEnumerateDevice()
        {
            PhysicalDevice[] gpus = instance.EnumeratePhysicalDevices();
            return gpus;
        }
    }
}
