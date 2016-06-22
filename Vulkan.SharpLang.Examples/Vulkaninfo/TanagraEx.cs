#if Tanagra
using System;
using System.Collections.Generic;
using Vulkan;
using Vulkan.Managed;

namespace Vulkaninfo.Tanagra
{

    public static class Commands
    {
        public static LayerProperties[] EnumerateInstanceLayerProperties()
        {
            throw new NotImplementedException();
        }

        public static ExtensionProperties[] EnumerateInstanceExtensionProperties(string name)
        {
            throw new NotImplementedException();
        }
    }

    public static class Extentions
    {
        // Instance
        public static PhysicalDevice[] EnumeratePhysicalDevices(this Instance inst)
        {
            throw new NotImplementedException();
        }

        public static void Destroy(this Instance inst, object alloc)
        {
            throw new NotImplementedException();
        }

        // PhysicalDevice

        public static FormatProperties GetFormatProperties(this PhysicalDevice obj,  Format format)
        {
            throw new NotImplementedException();
        }

        public static LayerProperties[] EnumerateDeviceLayerProperties(this PhysicalDevice obj)
        {
            throw new NotImplementedException();
        }

        public static ExtensionProperties[] EnumerateDeviceExtensionProperties(this PhysicalDevice obj, string name)
        {
            throw new NotImplementedException();
        }

        public static Device CreateDevice(this PhysicalDevice obj, DeviceCreateInfo info, object alloc)
        {
            throw new NotImplementedException();
        }

        public static PhysicalDeviceProperties GetProperties(this PhysicalDevice obj)
        {
            throw new NotImplementedException();
        }

        public static QueueFamilyProperties[] GetQueueFamilyProperties(this PhysicalDevice obj)
        {
            throw new NotImplementedException();
        }

        public static PhysicalDeviceMemoryProperties GetMemoryProperties(this PhysicalDevice obj)
        {
            throw new NotImplementedException();
        }

        public static PhysicalDeviceFeatures GetFeatures(this PhysicalDevice obj)
        {
            throw new NotImplementedException();
        }

        /*public static void Destroy(this PhysicalDevice obj, object alloc)
        {
            throw new NotImplementedException();
        }*/

        // Device

        public static void Destroy(this Device obj, object alloc)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
