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
            return Vk.EnumerateInstanceLayerProperties();
        }

        public static ExtensionProperties[] EnumerateInstanceExtensionProperties(string name)
        {
            return Vk.EnumerateInstanceExtensionProperties(name);
        }
    }

    public static class Extentions
    {
        // Instance
        public static PhysicalDevice[] EnumeratePhysicalDevices(this Instance inst)
        {
            return Vk.EnumeratePhysicalDevices(inst);
        }

        public static void Destroy(this Instance inst, AllocationCallbacks alloc)
        {
            Vk.DestroyInstance(inst, alloc);
        }

        // PhysicalDevice

        public static FormatProperties GetFormatProperties(this PhysicalDevice obj,  Format format)
        {
            return Vk.GetPhysicalDeviceFormatProperties(obj, format);
        }

        public static LayerProperties[] EnumerateDeviceLayerProperties(this PhysicalDevice obj)
        {
            return Vk.EnumerateDeviceLayerProperties(obj);
        }

        public static ExtensionProperties[] EnumerateDeviceExtensionProperties(this PhysicalDevice obj, string name)
        {
            return Vk.EnumerateDeviceExtensionProperties(obj, name);
        }

        public static Device CreateDevice(this PhysicalDevice obj, DeviceCreateInfo info, AllocationCallbacks alloc)
        {
            return Vk.CreateDevice(obj, info, alloc);
        }

        public static PhysicalDeviceProperties GetProperties(this PhysicalDevice obj)
        {
            return Vk.GetPhysicalDeviceProperties(obj);
        }

        public static QueueFamilyProperties[] GetQueueFamilyProperties(this PhysicalDevice obj)
        {
            return Vk.GetPhysicalDeviceQueueFamilyProperties(obj);
        }

        public static PhysicalDeviceMemoryProperties GetMemoryProperties(this PhysicalDevice obj)
        {
            return Vk.GetPhysicalDeviceMemoryProperties(obj);
        }

        public static PhysicalDeviceFeatures GetFeatures(this PhysicalDevice obj)
        {
            return Vk.GetPhysicalDeviceFeatures(obj);
        }

        /*public static void Destroy(this PhysicalDevice obj, object alloc)
        {
            throw new NotImplementedException();
        }*/

        // Device

        public static void Destroy(this Device obj, AllocationCallbacks alloc)
        {
            Vk.DestroyDevice(obj, alloc);
        }
    }
}
#endif
