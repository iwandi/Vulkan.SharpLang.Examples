using System;
using Vulkan;
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
    }
}
