using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    class SampleInstance
    {
        static void Main(string[] args)
        {
            string appShortName = "vulkansamples_instance";
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

            Instance instance = new Instance(createInfo);

            instance.Destroy();
        }
    }
}