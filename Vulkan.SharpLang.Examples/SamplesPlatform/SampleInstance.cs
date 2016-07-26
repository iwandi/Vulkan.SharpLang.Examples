using System;
using Vulkan;
using Vulkan.Windows;
using System.Diagnostics;
using System.Windows.Forms;

namespace Vulkan.SharpLang.Examples
{
    public class SampleInstance
    {
        class LayerPropertiesInfo 
        {
            public Vulkan.LayerProperties Properties;
            public ExtensionProperties[] Extensions;
        }

        LayerProperties[] instanceLayerProperties;

        public void InitGlobalLayerProperties()
        {
            LayerProperties[] props = Commands.EnumerateInstanceLayerProperties();
            instanceLayerProperties = new LayerProperties[props.Length];
            foreach (LayerProperties prop in props)
            {
                LayerPropertiesInfo layerProps = new LayerPropertiesInfo();
                layerProps.Properties = prop;
                layerProps.Extensions = Commands.EnumerateInstanceExtensionProperties(prop.LayerName);
            }
        }

        string[] instanceExtensionNames = new string[0];

        public void InitInstanceeExtensionNames()
        {
            instanceExtensionNames = new string[]
            {
                "VK_KHR_surface",
                "VK_KHR_win32_surface"
            };
        }

        string appShortName;
        Instance instance;
        public Instance Instance {  get { return instance; } }

        public Instance InitInstance(string appShortName)
        {
            this.appShortName = appShortName;
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
                EnabledExtensionNames = instanceExtensionNames,
            };

            instance = new Instance(createInfo);
            return instance;
        }
        
        PhysicalDevice gpu;
        public PhysicalDevice Gpu { get { return gpu; } }

        QueueFamilyProperties[] queueProps;
        public QueueFamilyProperties[] QueueProps {  get { return queueProps; } }
        public int QueueCount {  get { return queueProps.Length; } }
        PhysicalDeviceMemoryProperties memoryProperties;
        PhysicalDeviceProperties gpuProps;

        uint graphicsQueueFamilyIndex;
        public uint GraphicsQueueFamilyIndex {  get { return graphicsQueueFamilyIndex; } }

        public PhysicalDevice InitEnumerateDevice() 
        {
            PhysicalDevice[] gpus = instance.EnumeratePhysicalDevices();
            gpu = gpus[0];

            queueProps = gpu.GetQueueFamilyProperties();
            memoryProperties = gpu.GetMemoryProperties();
            gpuProps = gpu.GetProperties();

            return gpu;
        }

        string[] deviceExtensionNames = new string[0];

        public void InitDeviceExtensionNames()
        {
            deviceExtensionNames = new string[]
            {
                "VK_KHR_swapchain",
            };
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
                EnabledLayerNames = new string[0],
                EnabledExtensionNames = deviceExtensionNames, 
            };

            device = gpu.CreateDevice(info);
            return device;
        }

        public void InitQueueFamilyIndex()
        {
            queueProps = gpu.GetQueueFamilyProperties();

            bool found = false;
            for(uint i = 0; i < queueProps.Length; i++)
            {
                if((queueProps[i].QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics)
                {
                    graphicsQueueFamilyIndex = i;
                    found = true;
                    break;
                }
            }
            Debug.Assert(found);
        }

        uint width;
        public uint Width { get { return width; } }
        uint height;
        public uint Height { get { return height; } }

        public void InitWindowSize(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }

        public void InitConnection()
        {
            // Noting on windows
        }

        IntPtr connection;
        public IntPtr Connection {  get { return connection; } }
        Form window; // use intPtr or keep using form ?

        public IntPtr InitWindow()
        {
            connection = System.Runtime.InteropServices.Marshal.GetHINSTANCE(this.GetType().Module);

            window = new Form
            {
                Name = appShortName,
                Text = appShortName,
                Width = (int)width,
                Height = (int)height,                
            };

            window.Show();
            return window.Handle;
        }

        public void DestroyWindow()
        {
            window.Close();
            window.Dispose();
            window = null;
        }

        SurfaceKhr surface;
        SurfaceFormatKhr[] surfFormats;
        Format format;

        public void InitSwapChainExtension()
        {
            Win32SurfaceCreateInfoKhr createInfo = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = connection,
                Hwnd = window.Handle,
            };

            surface = instance.CreateWin32SurfaceKHR(createInfo);

            // Iterate over each queue to learn whether it supports presenting:
            bool[] supportsPresent = new bool[QueueCount];
            for (uint i = 0; i < supportsPresent.Length; i++)
            {
                supportsPresent[i] = gpu.GetSurfaceSupportKHR(i, surface);
            }

            // Search for a graphics queue and a present queue in the array of queue
            // families, try to find one that supports both
            uint graphicsQueueNodeIndex = uint.MaxValue;
            for (uint i = 0; i < QueueCount; i++)
            {
                if ((queueProps[i].QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics &&
                    supportsPresent[i])
                {
                    graphicsQueueNodeIndex = i;
                    break;
                }
            }

            // Generate error if could not find a queue that supports both a graphics
            // and present
            if (graphicsQueueNodeIndex == uint.MaxValue)
            {
                Console.WriteLine("Could not find a queue that supports both graphics and present");
                throw new Exception("Could not find a queue that supports both graphics and present");
            }

            graphicsQueueFamilyIndex = graphicsQueueNodeIndex;

            // Get the list of VkFormats that are supported:
            surfFormats = gpu.GetSurfaceFormatsKHR(surface);

            // If the format list includes just one entry of VK_FORMAT_UNDEFINED,
            // the surface has no preferred format.  Otherwise, at least one
            // supported format will be returned.
            if (surfFormats.Length == 1 && surfFormats[0].Format == Format.Undefined)
            {
                format = Format.B8g8r8a8Unorm;
            }
            else
            {
                Debug.Assert(surfFormats.Length >= 1);
                format = surfFormats[0].Format;
            }
        }

        public bool MemoryTypeFromProperties(uint typeBits, MemoryPropertyFlags requirementsMask, out uint typeIndex)
        {
            // Search memtypes to find first index with those properties
            for(uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if ((typeBits & 1) == 1)
                {
                    // Type is available, does it match user properties?
                    if ((memoryProperties.MemoryTypes[i].PropertyFlags & requirementsMask) == requirementsMask)
                    {
                        typeIndex = i;
                        return true;
                    }
                }
                typeBits >>= 1;
            }
            typeIndex = 0;
            return false;
        }
    }
}
