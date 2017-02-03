#define VK_USE_PLATFORM_WIN32_KHR
using System;
using System.Collections.Generic;
using System.IO;
using Vulkan;

#if VulkanSharp
using Vulkaninfo.VulkanSharp;
#elif Tanagra
using Vulkan.Managed;
using Vulkaninfo.Tanagra;
#endif

#if VK_USE_PLATFORM_WIN32_KHR
using System.Windows.Forms;
using Vulkan.Windows;
#endif

// TODO : vulkan
//          - Flag enum same type as in api QueueFlags is int but in api is uint
//          - Flag enum with null Entry
//          - Use enum in api
//          - PhysicalDeviceMemoryProperties.MemoryTpyes is not a array
//          - PhysicalDeviceMemoryProperties.MemoryHeapCount is not a array
//          - DeviceCreateInfo.QueueCreateInfoCount shuld not be exposed
//          - PhysicalDeviceProperties.PipelineCacheUUID is a array with 16 elements


namespace Vulkaninfo
{
    public class InfoGenerator
    {
        public string ApplicationName { get; set; }
        public uint ApplicationVersion { get; set; }
        public string EngineName { get; set; }
        public uint EnginveVersion { get; set; }
        public string[] KnownExtensions { get; set; }
        public string[] KnownDeviceExtensions { get; set; }

        public InfoGenerator()
        {
			string name = "FORMAT_ASTC12X12_SRGB_BLOCK";

			string newName = GetVkName(name);

			ApplicationName = "vulkaninfo";
            ApplicationVersion = 1;
            EngineName = ApplicationName;
            EnginveVersion = ApplicationVersion;
            KnownExtensions = new string[]
            {
                "VK_KHR_surface",
#if VK_USE_PLATFORM_ANDROID_KHR
        "VK_KHR_android_surface",
#endif
#if VK_USE_PLATFORM_MIR_KHR
        "VK_KHR_mir_surface",
#endif
#if VK_USE_PLATFORM_WAYLAND_KHR
        "VK_KHR_wayland_surface",
#endif
#if VK_USE_PLATFORM_WIN32_KHR
        "VK_KHR_win32_surface",
#endif
#if VK_USE_PLATFORM_XCB_KHR
        "VK_KHR_xcb_surface",
#endif
#if VK_USE_PLATFORM_XLIB_KHR
        "VK_KHR_xlib_surface",
#endif
            };
            KnownDeviceExtensions = new string[]
            {
                "VK_KHR_swapchain"
            };
        }

		IEnumerable<Format> ListFormats(AppDev dev)
		{
			bool pvrtc = false;
			foreach(ExtensionProperties extensionProp in dev.Gpu.DeviceExtensions)
			{
				if(extensionProp.ExtensionName == "VK_IMG_format_pvrtc")
				{
					pvrtc = true;
					break;
				}
			}

            foreach (Format f in Enum.GetValues(typeof(Format)))
			{
				int fi = (int)f;
				if(fi >= (int)Format.Pvrtc12BppUnormBlockImg && fi <= (int)Format.Pvrtc24BppSrgbBlockImg)
				{
					if (pvrtc)
					{
						yield return f;
					}
				}
				else
				{
					yield return f;
				}
			}
		}

        void AppDevInitFormats(AppDev dev)
        {
			Dictionary<Format, FormatProperties> formatList = new Dictionary<Format, FormatProperties>();
            foreach (Format f in ListFormats(dev))
            {
				formatList.Add(f, dev.Gpu.Obj.GetFormatProperties(f));
            }

			dev.FormatProbs = formatList;
        }

        void ExtractVersion(uint version, out uint major, out uint minor, out uint patch)
        {
            major = version >> 22;
            minor = version >> 11 & 0x3ff;
            patch = version & 0xfff;
        }

        AppDev AppDevInit(AppGpu gpu)
        {
            DeviceCreateInfo info = new DeviceCreateInfo
            {
                //QueueCreateInfoCount = 0, // TODO : this sould not be 
                QueueCreateInfos = new DeviceQueueCreateInfo[0],
                //EnabledLayerCount = 0,
                EnabledLayerNames = new string[0],
            };

            // Scan layers
            List<LayerExtensionList> layers = new List<LayerExtensionList>();
            LayerProperties[] layerProperties = gpu.Obj.EnumerateDeviceLayerProperties();
            if (layerProperties != null)
            {
                foreach (LayerProperties layer in layerProperties)
                {
                    LayerExtensionList layerExtList = new LayerExtensionList
                    {
                        LayerProperties = layer,
                        ExtensionProperties = gpu.Obj.EnumerateDeviceExtensionProperties(layer.LayerName),
                    };
                    if (layerExtList.ExtensionProperties == null)
                    {
                        layerExtList.ExtensionProperties = new ExtensionProperties[0];
                    }
                    layers.Add(layerExtList);
                }
            }

            ExtensionProperties[] extensions = gpu.Obj.EnumerateDeviceExtensionProperties("");

            foreach (string knownExtName in KnownDeviceExtensions)
            {
                bool extensionFound = false;
                foreach (ExtensionProperties extention in extensions)
                {
                    if (extention.ExtensionName == knownExtName)
                    {
                        extensionFound = true;
                        break;
                    }
                }

                if (!extensionFound)
                {
                    throw new Exception("Cannot find extension: " + knownExtName);
                }
            }

            gpu.DeviceLayers = layers.ToArray();
            gpu.DeviceExtensions = extensions;

            //info.QueueCreateInfoCount = (uint)gpu.QueueReqs.Length;
            info.QueueCreateInfos = gpu.QueueReqs;

            info.EnabledExtensionNames = KnownDeviceExtensions;
            //info.EnabledExtensionCount = (uint)KnownDeviceExtensions.Length;

            Device device = gpu.Obj.CreateDevice(info, null);

            return new AppDev
            {
                Gpu = gpu,
                Obj = device,
            };
        }
        
        void AppDevDestroy(AppDev dev)
        {
            dev.Obj.Destroy(null);
        }

        AppInstance AppCreateInstance(uint apiVersion)
        {
            ApplicationInfo appInfo = new ApplicationInfo
            {
                ApplicationName = ApplicationName,
                ApplicationVersion = ApplicationVersion,
                EngineName = EngineName,
                EngineVersion = EnginveVersion,
                ApiVersion = apiVersion,
            };

            InstanceCreateInfo createInfo = new InstanceCreateInfo
            {
                ApplicationInfo = appInfo,
                //EnabledLayerCount = 0,
                //EnabledExtensionCount = 0,
            };

            // Scan layers
            List<LayerExtensionList> layers = new List<LayerExtensionList>();
            LayerProperties[] layerProperties = Commands.EnumerateInstanceLayerProperties();
            if (layerProperties != null)
            {
                foreach (LayerProperties layer in layerProperties)
                {
                    LayerExtensionList layerExtList = new LayerExtensionList
                    {
                        LayerProperties = layer,
                        ExtensionProperties = Commands.EnumerateInstanceExtensionProperties(layer.LayerName),
                    };
                    if (layerExtList.ExtensionProperties == null)
                    {
                        layerExtList.ExtensionProperties = new ExtensionProperties[0];
                    }
                    layers.Add(layerExtList);
                }
            }

            ExtensionProperties[] extensions = Commands.EnumerateInstanceExtensionProperties("");
            if(extensions == null)
            {
                extensions = new ExtensionProperties[0];
            }

            foreach (string knownExtName in KnownExtensions)
            {
                bool extensionFound = false;
                foreach (ExtensionProperties extention in extensions)
                {
                    if (extention.ExtensionName == knownExtName)
                    {
                        extensionFound = true;
                        break;
                    }
                }

                if (!extensionFound)
                {
                    throw new Exception("Cannot find extension: " + knownExtName);
                }
            }

            createInfo.EnabledExtensionNames = KnownExtensions;
            //createInfo.EnabledExtensionCount = (uint)KnownExtensions.Length;

            // TODO : Register debug callback

#if VulkanSharp
            Instance instance = new Instance(createInfo);
#elif Tanagra
            Instance instance = Vk.CreateInstance(createInfo, null);
#endif

            return new AppInstance
            {
                Instance = instance,
                Layers = layers.ToArray(),
                Extensions = extensions,
            };
        }

        void AppDestroyInstance(AppInstance instance)
        {
            // TODO : Check if we need to free some structs

            instance.Instance.Destroy(null);
        }

        AppGpu AppGpuInit(uint id, PhysicalDevice obj)
        {
            // TODO : Limits

            AppGpu gpu = new AppGpu
            {
                Id = id,
                Obj = obj,
                Props = obj.GetProperties(),
                QueueProps = obj.GetQueueFamilyProperties(),
                MemoryProps = obj.GetMemoryProperties(),
                Features = obj.GetFeatures(),
                Limits = new PhysicalDeviceLimits(),
            };

            gpu.QueueReqs = new DeviceQueueCreateInfo[gpu.QueueProps.Length];
            for (uint i = 0; i < gpu.QueueProps.Length; i++)
            {
                uint queueCount = gpu.QueueProps[i].QueueCount;
                DeviceQueueCreateInfo queueReq = new DeviceQueueCreateInfo
                { 
                    QueueFamilyIndex = i,
                    //QueueCount = queueCount,
                    QueuePriorities = new float[queueCount],
                };
                gpu.QueueReqs[i] = queueReq;
            }

            gpu.Device = AppDevInit(gpu);            
            AppDevInitFormats(gpu.Device);

            return gpu;
        }

        void AppGpuDestroy(AppGpu gpu)
        {
            AppDevDestroy(gpu.Device);

            // TODO : Check if we need to free some structs
        }

#if VK_USE_PLATFORM_WIN32_KHR
		void AppCreateWin32Window(AppInstance instance)
		{
			instance.Connection = System.Runtime.InteropServices.Marshal.GetHINSTANCE(this.GetType().Module);

			instance.Window = new Form
			{
				Name = ApplicationName,
				Text = ApplicationName,
				Width = instance.Width,
				Height = instance.Height,
			};
		}

		void AppDestroyWin32Window(AppInstance instance)
		{
			instance.Window.Close();
			instance.Window = null;
			instance.Connection = IntPtr.Zero;
		}

		void AppCreateWin32Surface(AppInstance instance)
		{
			instance.Surface = instance.Instance.CreateWin32SurfaceKHR(new Win32SurfaceCreateInfoKhr
			{
				Hinstance = instance.Connection,
				Hwnd = instance.Window.Handle,
			});
		}
#endif

		void AppDestroySurface(AppInstance instance)
		{
			instance.Instance.DestroySurfaceKHR(instance.Surface);
			instance.Surface = null;
		}

		int AppDumpSurfaceFormats(AppInstance instance, AppGpu gpu, StreamWriter output)
		{
			SurfaceFormatKhr[] surfaceFormats = gpu.Obj.GetSurfaceFormatsKHR(instance.Surface);

			output.WriteLine("Formats:\t\tcount = {0}", surfaceFormats.Length);
			foreach (SurfaceFormatKhr surfaceFormat in surfaceFormats)
			{
				output.WriteLine("\t{0}", GetVkName(surfaceFormat.Format.ToString()));
			}

			return surfaceFormats.Length;
		}

		int AppDumpSurfacePresentModes(AppInstance instance, AppGpu gpu, StreamWriter output)
		{
			PresentModeKhr[] presentModes = gpu.Obj.GetSurfacePresentModesKHR(instance.Surface);

			output.WriteLine("Present Modes:\t\tcount = {0}", presentModes.Length);
			foreach (PresentModeKhr presentMode in presentModes)
			{
				output.WriteLine("\t{0}", GetVkName(presentMode.ToString(),"", "_KHR"));
			}

			return presentModes.Length;
		}

		static System.Text.StringBuilder s_vkSb = new System.Text.StringBuilder();
		readonly string[] s_dividersLead = new string[]
		{
			"IMAGE", "BIT", "ATTACHMENT", "BLEND",
			"SRC", "DST", "FILTER", "LINEAR",
			"TEXEL", "BUFFER", "SNORM", "UNORM",
			"USCALED", "SSCALED", "UINT", "SRGB",
			"SINT", "STENCIL", "PACK", "ATOMIC",
			"SFLOAT", "UFLOAT", "BLOCK", 
			"D24", "GPU"

			// "RGB" collides with SRGB

		};

		readonly string[] s_dividersTail = new string[]
		{
			"UNORM", "ETC2", "EAC", "ASTC",
			"BC1", "SFLOAT", "HOST", "DEVICE"
		};

		string GetVkName(string name, string prefix = "", string surfix = "")
        {
			name = prefix + name.ToUpper() +surfix;

			// scan for lead _
			foreach(string div in s_dividersLead)
			{
				int i = 0;
				do
				{
					i = name.IndexOf(div, i);
					if (i > 0)
					{
						if (name[i - 1] != '_')
						{
							name = name.Insert(i, "_");
						}
						i++;
					}
					else
					{
						break;
					}
				}
				while (i < name.Length);
			}

			// scan for tail _
			foreach (string div in s_dividersTail)
			{
				int i = 0;
				do
				{
					i = name.IndexOf(div, i);
					if (i > 0)
					{
						int pos = i + div.Length;
						if (name.Length > pos && name[pos] != '_')
						{
							name = name.Insert(pos, "_");
						}
						i++;
					}
					else
					{
						break;
					}
				}
				while (i < name.Length);
			}


			// Scan for x 
			int j = 0; 
			do
			{
				j = name.IndexOf('X', j);
				if (j > 0)
				{
					if (name.Length > j+1)
					{
						bool leadNumer = char.IsDigit(name[j - 1]);
						bool tailNumer = char.IsDigit(name[j + 1]);

						if(leadNumer && tailNumer)
						{
							name = name.Remove(j, 1).Insert(j, "x");
						}
					}
					j++;
				}
				else
				{
					break;
				}
			}
			while (j < name.Length);

			/*s_vkSb.Clear();

			//s_vkSb.Append(prefix);
			s_vkSb.Append(name);
			s_vkSb.Append(surfix);

			return s_vkSb.ToString();*/

			return name;
        }

		string GetVkEnumName(object enumObj)
		{
			string name = enumObj.ToString();

			name = name.ToUpper();
			name = name.Replace(", ", " | ");
			name = name.Replace("SPARSEBINDING", "SPARSE");

			return name;
		}

		void VkListFlags(object enumObj, string linePrefix, string namePrefix, string nameSurfix, StreamWriter output)
		{
			if ((int)enumObj == 0)
			{
				output.Write(linePrefix);
				output.WriteLine("None");
			}
			else
			{
				string[] flags = enumObj.ToString().Split(',');
				foreach (string flag in flags)
				{
					output.Write(linePrefix);
					output.WriteLine(GetVkName(flag.Trim(), namePrefix, nameSurfix));
				}
			}
		}

		string VkFormatValue( string format, object value)
		{
			string valueFormated = string.Format(format, value);
			if(!valueFormated.StartsWith("-"))
			{
				valueFormated = " " + valueFormated;
			}
			return valueFormated;
		}

        void AppDevDumpFormatProps(AppDev dev, Format fmt, StreamWriter output)
        {
            FormatProperties props = dev.FormatProbs[fmt];

            Feature[] features = new Feature[3];
            features[0].Name = "linearTiling   FormatFeatureFlags";
            features[0].Flags = (FormatFeatureFlags)props.LinearTilingFeatures;
            features[1].Name = "optimalTiling  FormatFeatureFlags";
            features[1].Flags = (FormatFeatureFlags)props.OptimalTilingFeatures;
            features[2].Name = "bufferFeatures FormatFeatureFlags";
            features[2].Flags = (FormatFeatureFlags)props.BufferFeatures;

			output.WriteLine();
            output.Write("{0}:", GetVkName(fmt.ToString(), "FORMAT_"));

			foreach (Feature feature in features)
            {
                output.Write("\n\t{0}:", feature.Name);
                if(feature.Flags == 0)
                {
                    output.Write("\n\t\tNone");
                }
                else
                {
                    foreach(FormatFeatureFlags flag in Enum.GetValues(typeof(FormatFeatureFlags)))
                    {
                        if ((feature.Flags & flag) == flag)
                        {
                            string name = GetVkName(flag.ToString(), "VK_FORMAT_FEATURE_", "_BIT");
                            output.Write("\n\t\t{0}", name);
                        }
                    }
                }
            }
            output.WriteLine();
        }

        void AppDevDump(AppDev dev, StreamWriter output)
        {
			output.WriteLine("Format Properties:");
			output.WriteLine("==================");
			foreach (Format fmt in ListFormats(dev))
            {
                AppDevDumpFormatProps(dev, fmt, output);
            }
        }

        void AppGpuDumpFeatures(AppGpu gpu, StreamWriter output)
        {
            PhysicalDeviceFeatures features = gpu.Features;

            output.WriteLine("VkPhysicalDeviceFeatures:");
            output.WriteLine("=========================");

			output.WriteLine("\trobustBufferAccess                      ={0}", VkFormatValue("{0}", features.RobustBufferAccess));
            output.WriteLine("\tfullDrawIndexUint32                     ={0}", VkFormatValue("{0}", features.FullDrawIndexUint32));
            output.WriteLine("\timageCubeArray                          ={0}", VkFormatValue("{0}", features.ImageCubeArray));
            output.WriteLine("\tindependentBlend                        ={0}", VkFormatValue("{0}", features.IndependentBlend));
            output.WriteLine("\tgeometryShader                          ={0}", VkFormatValue("{0}", features.GeometryShader));
            output.WriteLine("\ttessellationShader                      ={0}", VkFormatValue("{0}", features.TessellationShader));
            output.WriteLine("\tsampleRateShading                       ={0}", VkFormatValue("{0}", features.SampleRateShading));
            output.WriteLine("\tdualSrcBlend                            ={0}", VkFormatValue("{0}", features.DualSrcBlend));
            output.WriteLine("\tlogicOp                                 ={0}", VkFormatValue("{0}", features.LogicOp));
            output.WriteLine("\tmultiDrawIndirect                       ={0}", VkFormatValue("{0}", features.MultiDrawIndirect));
            output.WriteLine("\tdrawIndirectFirstInstance               ={0}", VkFormatValue("{0}", features.DrawIndirectFirstInstance));
            output.WriteLine("\tdepthClamp                              ={0}", VkFormatValue("{0}", features.DepthClamp));
            output.WriteLine("\tdepthBiasClamp                          ={0}", VkFormatValue("{0}", features.DepthBiasClamp));
            output.WriteLine("\tfillModeNonSolid                        ={0}", VkFormatValue("{0}", features.FillModeNonSolid));
            output.WriteLine("\tdepthBounds                             ={0}", VkFormatValue("{0}", features.DepthBounds));
            output.WriteLine("\twideLines                               ={0}", VkFormatValue("{0}", features.WideLines));
            output.WriteLine("\tlargePoints                             ={0}", VkFormatValue("{0}", features.LargePoints));
            output.WriteLine("\ttextureCompressionETC2                  ={0}", VkFormatValue("{0}", features.TextureCompressionEtc2));
#if VulkanSharp
			output.WriteLine("\ttextureCompressionASTC_LDR              ={0}", VkFormatValue("{0}", features.TextureCompressionAstcLdr));
#elif Tanagra
            output.WriteLine("\ttextureCompressionASTC_LDR              ={0}", VkFormatValue("{0}", features.TextureCompressionASTC_LDR));
#endif
			output.WriteLine("\ttextureCompressionBC                    ={0}", VkFormatValue("{0}", features.TextureCompressionBc));
            output.WriteLine("\tocclusionQueryPrecise                   ={0}", VkFormatValue("{0}", features.OcclusionQueryPrecise));
            output.WriteLine("\tpipelineStatisticsQuery                 ={0}", VkFormatValue("{0}", features.PipelineStatisticsQuery));
            output.WriteLine("\tvertexSideEffects                       ={0}", VkFormatValue("{0}", features.VertexPipelineStoresAndAtomics));
            output.WriteLine("\ttessellationSideEffects                 ={0}", VkFormatValue("{0}", features.FragmentStoresAndAtomics));
            output.WriteLine("\tgeometrySideEffects                     ={0}", VkFormatValue("{0}", features.ShaderTessellationAndGeometryPointSize));
            output.WriteLine("\tshaderImageGatherExtended               ={0}", VkFormatValue("{0}", features.ShaderImageGatherExtended));
            output.WriteLine("\tshaderStorageImageExtendedFormats       ={0}", VkFormatValue("{0}", features.ShaderStorageImageExtendedFormats));
            output.WriteLine("\tshaderStorageImageMultisample           ={0}", VkFormatValue("{0}", features.ShaderStorageImageMultisample));
            output.WriteLine("\tshaderStorageImageReadWithoutFormat     ={0}", VkFormatValue("{0}", features.ShaderStorageImageReadWithoutFormat));
            output.WriteLine("\tshaderStorageImageWriteWithoutFormat    ={0}", VkFormatValue("{0}", features.ShaderStorageImageWriteWithoutFormat));
            output.WriteLine("\tshaderUniformBufferArrayDynamicIndexing ={0}", VkFormatValue("{0}", features.ShaderUniformBufferArrayDynamicIndexing));
            output.WriteLine("\tshaderSampledImageArrayDynamicIndexing  ={0}", VkFormatValue("{0}", features.ShaderSampledImageArrayDynamicIndexing));
            output.WriteLine("\tshaderStorageBufferArrayDynamicIndexing ={0}", VkFormatValue("{0}", features.ShaderStorageBufferArrayDynamicIndexing));
            output.WriteLine("\tshaderStorageImageArrayDynamicIndexing  ={0}", VkFormatValue("{0}", features.ShaderStorageImageArrayDynamicIndexing));
            output.WriteLine("\tshaderClipDistance                      ={0}", VkFormatValue("{0}", features.ShaderClipDistance));
            output.WriteLine("\tshaderCullDistance                      ={0}", VkFormatValue("{0}", features.ShaderCullDistance));
            output.WriteLine("\tshaderFloat64                           ={0}", VkFormatValue("{0}", features.ShaderFloat64));
            output.WriteLine("\tshaderInt64                             ={0}", VkFormatValue("{0}", features.ShaderInt64));
            output.WriteLine("\tshaderInt16                             ={0}", VkFormatValue("{0}", features.ShaderInt16));
            output.WriteLine("\tshaderResourceResidency                 ={0}", VkFormatValue("{0}", features.ShaderResourceResidency));
            output.WriteLine("\tshaderResourceMinLod                    ={0}", VkFormatValue("{0}", features.ShaderResourceMinLod));
            output.WriteLine("\talphaToOne                              ={0}", VkFormatValue("{0}", features.AlphaToOne));
            output.WriteLine("\tsparseBinding                           ={0}", VkFormatValue("{0}", features.SparseBinding));
            output.WriteLine("\tsparseResidencyBuffer                   ={0}", VkFormatValue("{0}", features.SparseResidencyBuffer));
            output.WriteLine("\tsparseResidencyImage2D                  ={0}", VkFormatValue("{0}", features.SparseResidencyImage2D));
            output.WriteLine("\tsparseResidencyImage3D                  ={0}", VkFormatValue("{0}", features.SparseResidencyImage3D));
            output.WriteLine("\tsparseResidency2Samples                 ={0}", VkFormatValue("{0}", features.SparseResidency2Samples));
            output.WriteLine("\tsparseResidency4Samples                 ={0}", VkFormatValue("{0}", features.SparseResidency4Samples));
            output.WriteLine("\tsparseResidency8Samples                 ={0}", VkFormatValue("{0}", features.SparseResidency8Samples));
            output.WriteLine("\tsparseResidency16Samples                ={0}", VkFormatValue("{0}", features.SparseResidency16Samples));
            output.WriteLine("\tsparseResidencyAliased                  ={0}", VkFormatValue("{0}", features.SparseResidencyAliased));
            output.WriteLine("\tvariableMultisampleRate                 ={0}", VkFormatValue("{0}", features.VariableMultisampleRate));
            output.WriteLine("\tinheritedQueries                        ={0}", VkFormatValue("{0}", features.InheritedQueries));
        }

        void AppDumpSparseProps(PhysicalDeviceSparseProperties sparseProps, StreamWriter output)
        {
            output.WriteLine("\tVkPhysicalDeviceSparseProperties:");
            output.WriteLine("\t---------------------------------");

            output.WriteLine("\t\tresidencyStandard2DBlockShape            ={0}", VkFormatValue("{0}", sparseProps.ResidencyStandard2DBlockShape));
            output.WriteLine("\t\tresidencyStandard2DMultisampleBlockShape ={0}", VkFormatValue("{0}", sparseProps.ResidencyStandard2DMultisampleBlockShape));
            output.WriteLine("\t\tresidencyStandard3DBlockShape            ={0}", VkFormatValue("{0}", sparseProps.ResidencyStandard3DBlockShape));
            output.WriteLine("\t\tresidencyAlignedMipSize                  ={0}", VkFormatValue("{0}", sparseProps.ResidencyAlignedMipSize));
            output.WriteLine("\t\tresidencyNonResidentStrict               ={0}", VkFormatValue("{0}", sparseProps.ResidencyNonResidentStrict));
        }

        void AppDumpLimits(PhysicalDeviceLimits limits, StreamWriter output)
        {
            output.WriteLine("\tVkPhysicalDeviceLimits:");
            output.WriteLine("\t-----------------------");

            output.WriteLine("\t\tmaxImageDimension1D                     ={0}", VkFormatValue("{0}", limits.MaxImageDimension1D));
            output.WriteLine("\t\tmaxImageDimension2D                     ={0}", VkFormatValue("{0}", limits.MaxImageDimension2D));
            output.WriteLine("\t\tmaxImageDimension3D                     ={0}", VkFormatValue("{0}", limits.MaxImageDimension3D));
            output.WriteLine("\t\tmaxImageDimensionCube                   ={0}", VkFormatValue("{0}", limits.MaxImageDimensionCube));
            output.WriteLine("\t\tmaxImageArrayLayers                     ={0}", VkFormatValue("{0}", limits.MaxImageArrayLayers));
            output.WriteLine("\t\tmaxTexelBufferElements                  ={0}", VkFormatValue("0x{0:x}", limits.MaxTexelBufferElements));
            output.WriteLine("\t\tmaxUniformBufferRange                   ={0}", VkFormatValue("0x{0:x}", limits.MaxUniformBufferRange));
            output.WriteLine("\t\tmaxStorageBufferRange                   ={0}", VkFormatValue("0x{0:x}", limits.MaxStorageBufferRange));
            output.WriteLine("\t\tmaxPushConstantsSize                    ={0}", VkFormatValue("{0}", limits.MaxPushConstantsSize));
            output.WriteLine("\t\tmaxMemoryAllocationCount                ={0}", VkFormatValue("{0}", limits.MaxMemoryAllocationCount));
            output.WriteLine("\t\tmaxSamplerAllocationCount               ={0}", VkFormatValue("{0}", limits.MaxSamplerAllocationCount));
            output.WriteLine("\t\tbufferImageGranularity                  ={0}", VkFormatValue("0x{0:x}", limits.BufferImageGranularity));
            output.WriteLine("\t\tsparseAddressSpaceSize                  ={0}", VkFormatValue("0x{0:x}", (ulong)limits.SparseAddressSpaceSize));
            output.WriteLine("\t\tmaxBoundDescriptorSets                  ={0}", VkFormatValue("{0}", limits.MaxBoundDescriptorSets));
            output.WriteLine("\t\tmaxPerStageDescriptorSamplers           ={0}", VkFormatValue("{0}", limits.MaxPerStageDescriptorSamplers));
            output.WriteLine("\t\tmaxPerStageDescriptorUniformBuffers     ={0}", VkFormatValue("{0}", limits.MaxPerStageDescriptorUniformBuffers));
            output.WriteLine("\t\tmaxPerStageDescriptorStorageBuffers     ={0}", VkFormatValue("{0}", limits.MaxPerStageDescriptorStorageBuffers));
            output.WriteLine("\t\tmaxPerStageDescriptorSampledImages      ={0}", VkFormatValue("{0}", limits.MaxPerStageDescriptorSampledImages));
            output.WriteLine("\t\tmaxPerStageDescriptorStorageImages      ={0}", VkFormatValue("{0}", limits.MaxPerStageDescriptorStorageImages));
            output.WriteLine("\t\tmaxPerStageDescriptorInputAttachments   ={0}", VkFormatValue("{0}", limits.MaxPerStageDescriptorInputAttachments));
            output.WriteLine("\t\tmaxPerStageResources                    ={0}", VkFormatValue("{0}", limits.MaxPerStageResources));
            output.WriteLine("\t\tmaxDescriptorSetSamplers                ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetSamplers));
            output.WriteLine("\t\tmaxDescriptorSetUniformBuffers          ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetUniformBuffers));
            output.WriteLine("\t\tmaxDescriptorSetUniformBuffersDynamic   ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetUniformBuffersDynamic));
            output.WriteLine("\t\tmaxDescriptorSetStorageBuffers          ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetStorageBuffers));
            output.WriteLine("\t\tmaxDescriptorSetStorageBuffersDynamic   ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetStorageBuffersDynamic));
            output.WriteLine("\t\tmaxDescriptorSetSampledImages           ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetSampledImages));
            output.WriteLine("\t\tmaxDescriptorSetStorageImages           ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetStorageImages));
            output.WriteLine("\t\tmaxDescriptorSetInputAttachments        ={0}", VkFormatValue("{0}", limits.MaxDescriptorSetInputAttachments));
            output.WriteLine("\t\tmaxVertexInputAttributes                ={0}", VkFormatValue("{0}", limits.MaxVertexInputAttributes));
            output.WriteLine("\t\tmaxVertexInputBindings                  ={0}", VkFormatValue("{0}", limits.MaxVertexInputBindings));
            output.WriteLine("\t\tmaxVertexInputAttributeOffset           ={0}", VkFormatValue("0x{0:x}", limits.MaxVertexInputAttributeOffset));
            output.WriteLine("\t\tmaxVertexInputBindingStride             ={0}", VkFormatValue("0x{0:x}", limits.MaxVertexInputBindingStride));
            output.WriteLine("\t\tmaxVertexOutputComponents               ={0}", VkFormatValue("{0}", limits.MaxVertexOutputComponents));
            output.WriteLine("\t\tmaxTessellationGenerationLevel          ={0}", VkFormatValue("{0}", limits.MaxTessellationGenerationLevel));
            output.WriteLine("\t\tmaxTessellationPatchSize                        ={0}", VkFormatValue("{0}", limits.MaxTessellationPatchSize));
            output.WriteLine("\t\tmaxTessellationControlPerVertexInputComponents  ={0}", VkFormatValue("{0}", limits.MaxTessellationControlPerVertexInputComponents));
            output.WriteLine("\t\tmaxTessellationControlPerVertexOutputComponents ={0}", VkFormatValue("{0}", limits.MaxTessellationControlPerVertexOutputComponents));
            output.WriteLine("\t\tmaxTessellationControlPerPatchOutputComponents  ={0}", VkFormatValue("{0}", limits.MaxTessellationControlPerPatchOutputComponents));
            output.WriteLine("\t\tmaxTessellationControlTotalOutputComponents     ={0}", VkFormatValue("{0}", limits.MaxTessellationControlTotalOutputComponents));
            output.WriteLine("\t\tmaxTessellationEvaluationInputComponents        ={0}", VkFormatValue("{0}", limits.MaxTessellationEvaluationInputComponents));
            output.WriteLine("\t\tmaxTessellationEvaluationOutputComponents       ={0}", VkFormatValue("{0}", limits.MaxTessellationEvaluationOutputComponents));
            output.WriteLine("\t\tmaxGeometryShaderInvocations            ={0}", VkFormatValue("{0}", limits.MaxGeometryShaderInvocations));
            output.WriteLine("\t\tmaxGeometryInputComponents              ={0}", VkFormatValue("{0}", limits.MaxGeometryInputComponents));
            output.WriteLine("\t\tmaxGeometryOutputComponents             ={0}", VkFormatValue("{0}", limits.MaxGeometryOutputComponents));
            output.WriteLine("\t\tmaxGeometryOutputVertices               ={0}", VkFormatValue("{0}", limits.MaxGeometryOutputVertices));
            output.WriteLine("\t\tmaxGeometryTotalOutputComponents        ={0}", VkFormatValue("{0}", limits.MaxGeometryTotalOutputComponents));
            output.WriteLine("\t\tmaxFragmentInputComponents              ={0}", VkFormatValue("{0}", limits.MaxFragmentInputComponents));
            output.WriteLine("\t\tmaxFragmentOutputAttachments            ={0}", VkFormatValue("{0}", limits.MaxFragmentOutputAttachments));
            output.WriteLine("\t\tmaxFragmentDualSrcAttachments           ={0}", VkFormatValue("{0}", limits.MaxFragmentDualSrcAttachments));
            output.WriteLine("\t\tmaxFragmentCombinedOutputResources      ={0}", VkFormatValue("{0}", limits.MaxFragmentCombinedOutputResources));
            output.WriteLine("\t\tmaxComputeSharedMemorySize              ={0}", VkFormatValue("0x{0:x}", limits.MaxComputeSharedMemorySize));
#if VulkanSharp
            output.WriteLine("\t\tmaxComputeWorkGroupCount[0]             ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupCount[0]));
            output.WriteLine("\t\tmaxComputeWorkGroupCount[1]             ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupCount[1]));
            output.WriteLine("\t\tmaxComputeWorkGroupCount[2]             ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupCount[2]));
#elif Tanagra
            output.WriteLine("\t\tmaxComputeWorkGroupCount[0]             ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupCount.X));
            output.WriteLine("\t\tmaxComputeWorkGroupCount[1]             ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupCount.Y));
            output.WriteLine("\t\tmaxComputeWorkGroupCount[2]             ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupCount.Z));
#endif
			output.WriteLine("\t\tmaxComputeWorkGroupInvocations          ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupInvocations));
#if VulkanSharp
            output.WriteLine("\t\tmaxComputeWorkGroupSize[0]              ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupSize[0]));
            output.WriteLine("\t\tmaxComputeWorkGroupSize[1]              ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupSize[1]));
            output.WriteLine("\t\tmaxComputeWorkGroupSize[2]              ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupSize[2]));
#elif Tanagra
            output.WriteLine("\t\tmaxComputeWorkGroupSize[0]              ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupSize.X));
            output.WriteLine("\t\tmaxComputeWorkGroupSize[1]              ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupSize.Y));
            output.WriteLine("\t\tmaxComputeWorkGroupSize[2]              ={0}", VkFormatValue("{0}", limits.MaxComputeWorkGroupSize.Z));
#endif
			output.WriteLine("\t\tsubPixelPrecisionBits                   ={0}", VkFormatValue("{0}", limits.SubPixelPrecisionBits));
            output.WriteLine("\t\tsubTexelPrecisionBits                   ={0}", VkFormatValue("{0}", limits.SubTexelPrecisionBits));
            output.WriteLine("\t\tmipmapPrecisionBits                     ={0}", VkFormatValue("{0}", limits.MipmapPrecisionBits));
            output.WriteLine("\t\tmaxDrawIndexedIndexValue                ={0}", VkFormatValue("{0}", limits.MaxDrawIndexedIndexValue));
            output.WriteLine("\t\tmaxDrawIndirectCount                    ={0}", VkFormatValue("{0}", limits.MaxDrawIndirectCount));
            output.WriteLine("\t\tmaxSamplerLodBias                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.MaxSamplerLodBias, 6)));
            output.WriteLine("\t\tmaxSamplerAnisotropy                    ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.MaxSamplerAnisotropy, 6)));
            output.WriteLine("\t\tmaxViewports                            ={0}", VkFormatValue("{0}", limits.MaxViewports));
#if VulkanSharp
            output.WriteLine("\t\tmaxViewportDimensions[0]                ={0}", VkFormatValue("{0}", limits.MaxViewportDimensions[0]));
            output.WriteLine("\t\tmaxViewportDimensions[1]                ={0}", VkFormatValue("{0}", limits.MaxViewportDimensions[1]));
            output.WriteLine("\t\tviewportBoundsRange[0]                  ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.ViewportBoundsRange[0], 6)));
            output.WriteLine("\t\tviewportBoundsRange[1]                  ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.ViewportBoundsRange[1], 6)));
#elif Tanagra
            output.WriteLine("\t\tmaxViewportDimensions[0]                ={0}", VkFormatValue("0x{0:x}", limits.MaxViewportDimensions.X));
            output.WriteLine("\t\tmaxViewportDimensions[1]                ={0}", VkFormatValue("0x{0:x}", limits.MaxViewportDimensions.Y));
            output.WriteLine("\t\tviewportBoundsRange[0]                  ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.ViewportBoundsRange.Min, 6)));
            output.WriteLine("\t\tviewportBoundsRange[1]                  ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.ViewportBoundsRange.Max, 6)));
#endif
			output.WriteLine("\t\tviewportSubPixelBits                    ={0}", VkFormatValue("{0}", limits.ViewportSubPixelBits));
            output.WriteLine("\t\tminMemoryMapAlignment                   ={0}", VkFormatValue("{0}", limits.MinMemoryMapAlignment));
            output.WriteLine("\t\tminTexelBufferOffsetAlignment           ={0}", VkFormatValue("0x{0:x}", limits.MinTexelBufferOffsetAlignment));
            output.WriteLine("\t\tminUniformBufferOffsetAlignment         ={0}", VkFormatValue("0x{0:x}", (ulong)limits.MinUniformBufferOffsetAlignment));
            output.WriteLine("\t\tminStorageBufferOffsetAlignment         ={0}", VkFormatValue("0x{0:x}", (ulong)limits.MinStorageBufferOffsetAlignment));
            output.WriteLine("\t\tminTexelOffset                          ={0}", VkFormatValue("{0}", limits.MinTexelOffset));
            output.WriteLine("\t\tmaxTexelOffset                          ={0}", VkFormatValue("{0}", limits.MaxTexelOffset));
            output.WriteLine("\t\tminTexelGatherOffset                    ={0}", VkFormatValue("{0}", limits.MinTexelGatherOffset));
            output.WriteLine("\t\tmaxTexelGatherOffset                    ={0}", VkFormatValue("{0}", limits.MaxTexelGatherOffset));
            output.WriteLine("\t\tminInterpolationOffset                  ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.MinInterpolationOffset, 6)));
            output.WriteLine("\t\tmaxInterpolationOffset                  ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.MaxInterpolationOffset, 6)));
            output.WriteLine("\t\tsubPixelInterpolationOffsetBits         ={0}", VkFormatValue("{0}", limits.SubPixelInterpolationOffsetBits));
            output.WriteLine("\t\tmaxFramebufferWidth                     ={0}", VkFormatValue("{0}", limits.MaxFramebufferWidth));
            output.WriteLine("\t\tmaxFramebufferHeight                    ={0}", VkFormatValue("{0}", limits.MaxFramebufferHeight));
            output.WriteLine("\t\tmaxFramebufferLayers                    ={0}", VkFormatValue("{0}", limits.MaxFramebufferLayers));
            output.WriteLine("\t\tframebufferColorSampleCounts            ={0}", VkFormatValue("{0}", (int)limits.FramebufferColorSampleCounts));
            output.WriteLine("\t\tframebufferDepthSampleCounts            ={0}", VkFormatValue("{0}", (int)limits.FramebufferDepthSampleCounts));
            output.WriteLine("\t\tframebufferStencilSampleCounts          ={0}", VkFormatValue("{0}", (int)limits.FramebufferStencilSampleCounts));
			output.WriteLine("\t\tframebufferNoAttachmentsSampleCounts    ={0}", VkFormatValue("{0}", (int)limits.FramebufferNoAttachmentsSampleCounts));
			output.WriteLine("\t\tmaxColorAttachments                     ={0}", VkFormatValue("{0}", limits.MaxColorAttachments));
            output.WriteLine("\t\tsampledImageColorSampleCounts           ={0}", VkFormatValue("{0}", (int)limits.SampledImageColorSampleCounts));
            output.WriteLine("\t\tsampledImageDepthSampleCounts           ={0}", VkFormatValue("{0}", (int)limits.SampledImageDepthSampleCounts));
            output.WriteLine("\t\tsampledImageStencilSampleCounts         ={0}", VkFormatValue("{0}", (int)limits.SampledImageStencilSampleCounts));
            output.WriteLine("\t\tsampledImageIntegerSampleCounts         ={0}", VkFormatValue("{0}", (int)limits.SampledImageIntegerSampleCounts));
            output.WriteLine("\t\tstorageImageSampleCounts                ={0}", VkFormatValue("{0}", (int)limits.StorageImageSampleCounts));
            output.WriteLine("\t\tmaxSampleMaskWords                      ={0}", VkFormatValue("{0}", limits.MaxSampleMaskWords));
            output.WriteLine("\t\ttimestampComputeAndGraphics             ={0}", VkFormatValue("{0}", Convert.ToInt32(limits.TimestampComputeAndGraphics)));
            output.WriteLine("\t\ttimestampPeriod                         ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.TimestampPeriod, 6)));
            output.WriteLine("\t\tmaxClipDistances                        ={0}", VkFormatValue("{0}", limits.MaxClipDistances));
            output.WriteLine("\t\tmaxCullDistances                        ={0}", VkFormatValue("{0}", limits.MaxCullDistances));
            output.WriteLine("\t\tmaxCombinedClipAndCullDistances         ={0}", VkFormatValue("{0}", limits.MaxCombinedClipAndCullDistances));
			output.WriteLine("\t\tdiscreteQueuePriorities                 ={0}", VkFormatValue("{0}", limits.DiscreteQueuePriorities));
#if VulkanSharp
			output.WriteLine("\t\tpointSizeRange[0]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.PointSizeRange[0], 6)));
            output.WriteLine("\t\tpointSizeRange[1]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.PointSizeRange[1], 6)));
            output.WriteLine("\t\tlineWidthRange[0]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.LineWidthRange[0], 6)));
            output.WriteLine("\t\tlineWidthRange[1]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.LineWidthRange[1], 6)));
#elif Tanagra
            output.WriteLine("\t\tpointSizeRange[0]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.PointSizeRange.Min, 6)));
            output.WriteLine("\t\tpointSizeRange[1]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.PointSizeRange.Max, 6)));
            output.WriteLine("\t\tlineWidthRange[0]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.LineWidthRange.Min, 6)));
            output.WriteLine("\t\tlineWidthRange[1]                       ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.LineWidthRange.Max, 6)));
#endif
			output.WriteLine("\t\tpointSizeGranularity                    ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.PointSizeGranularity, 6)));
            output.WriteLine("\t\tlineWidthGranularity                    ={0}", VkFormatValue("{0:0.000000}", Math.Round(limits.LineWidthGranularity, 6)));
            output.WriteLine("\t\tstrictLines                             ={0}", VkFormatValue("{0}", Convert.ToInt32(limits.StrictLines)));
            output.WriteLine("\t\tstandardSampleLocations                 ={0}", VkFormatValue("{0}", Convert.ToInt32(limits.StandardSampleLocations)));
            output.WriteLine("\t\toptimalBufferCopyOffsetAlignment        ={0}", VkFormatValue("0x{0:x}", limits.OptimalBufferCopyOffsetAlignment));
            output.WriteLine("\t\toptimalBufferCopyRowPitchAlignment      ={0}", VkFormatValue("0x{0:x}", limits.OptimalBufferCopyRowPitchAlignment));
            output.WriteLine("\t\tnonCoherentAtomSize                     ={0}", VkFormatValue("0x{0:x}", (ulong)limits.NonCoherentAtomSize));
        }

        void AppGpuDumpProps(AppGpu gpu, StreamWriter output)
        {
            PhysicalDeviceProperties props = gpu.Props;

			uint major, minor, patch;

			ExtractVersion(props.ApiVersion, out major, out minor, out patch);

			output.WriteLine("VkPhysicalDeviceProperties:");
            output.WriteLine("===========================");
            output.WriteLine("\tapiVersion     = 0x{0:x}  ({1}.{2}.{3})", props.ApiVersion, major, minor, patch);
            output.WriteLine("\tdriverVersion  = {0} (0x{0:x})", props.DriverVersion);
            output.WriteLine("\tvendorID       = 0x{0:x}", props.VendorId);
            output.WriteLine("\tdeviceID       = 0x{0:x}", props.DeviceId);
            output.WriteLine("\tdeviceType     = {0}", GetVkName(props.DeviceType.ToString()));
            output.WriteLine("\tdeviceName     = {0}", props.DeviceName);

            AppDumpLimits(props.Limits, output);
            AppDumpSparseProps(props.SparseProperties, output);;
        }

        void AppDumpExtensions(string ident, string layerName, ExtensionProperties[] extensionProperties, StreamWriter output)
        {
			if(extensionProperties == null)
			{
				extensionProperties = new ExtensionProperties[0];
			}
            if (!string.IsNullOrEmpty(layerName))
            {
                output.Write("{0}{1} Extensions", ident, layerName);
            }
            else
            {
                output.Write("Extensions");
            }

            output.WriteLine("\tcount = {0}", extensionProperties.Length);
            foreach (ExtensionProperties extProp in extensionProperties)
            {
                output.Write("{0}\t", ident);
                output.WriteLine("{0,-36}: extension revision {1,2}", extProp.ExtensionName, extProp.SpecVersion);
            }
        }

		bool HasExtension(string extensionName, ExtensionProperties[] extensionProperties) 
		{
			foreach(ExtensionProperties prop in extensionProperties)
			{
				if(prop.ExtensionName == extensionName)
				{
					return true;
				}
			}
			return false;
		}

		void AppGpuDumpQueuProps(AppGpu gpu, uint id, StreamWriter output)
        {
            QueueFamilyProperties props = gpu.QueueProps[id];

            output.WriteLine("VkQueueFamilyProperties[{0}]:", id);
            output.WriteLine("===========================");
			/*output.WriteLine("\tqueueFlags         = {0}{1}{2}",
                   ((QueueFlags)props.QueueFlags & QueueFlags.Graphics) == QueueFlags.Graphics ? 'G' : '.',
                   ((QueueFlags)props.QueueFlags & QueueFlags.Compute) == QueueFlags.Compute ? 'C' : '.',
                   ((QueueFlags)props.QueueFlags & QueueFlags.Transfer) == QueueFlags.Transfer ? 'D' : '.');
                   //((QueueFlags)props.QueueFlags & QueueFlags.SparseBinding) == QueueFlags.SparseBinding ? 'S' : '.'); // Add this option, not pressent in original*/
			output.WriteLine("\tqueueFlags         = {0}", GetVkEnumName(props.QueueFlags));
            output.WriteLine("\tqueueCount         = {0}", GetVkEnumName(props.QueueCount));
            output.WriteLine("\ttimestampValidBits = {0}", GetVkEnumName(props.TimestampValidBits));
            output.WriteLine("\tminImageTransferGranularity = ({0}, {1}, {2})",
                   props.MinImageTransferGranularity.Width,
                   props.MinImageTransferGranularity.Height,
                   props.MinImageTransferGranularity.Depth);
        }

        void AppGpuDumpMemoryProps(AppGpu gpu, StreamWriter output)
        {
            PhysicalDeviceMemoryProperties props = gpu.MemoryProps;

            output.WriteLine("VkPhysicalDeviceMemoryProperties:");
            output.WriteLine("=================================");
            output.WriteLine("\tmemoryTypeCount       = {0}", props.MemoryTypeCount);
            for(uint i = 0; i < props.MemoryTypeCount; i++)
            {
                MemoryType memoryType = props.MemoryTypes[i];

                output.WriteLine("\tmemoryTypes[{0}] : ", i);
				output.WriteLine("\t\theapIndex     = {0}", memoryType.HeapIndex);
				output.WriteLine("\t\tpropertyFlags = 0x{0:x}:", (ulong)memoryType.PropertyFlags);

				VkListFlags(memoryType.PropertyFlags, "\t\t\t", "VK_MEMORY_PROPERTY_", "_BIT", output);
			}
			output.WriteLine();
			output.WriteLine("\tmemoryHeapCount       = {0}", props.MemoryHeapCount);

            for (uint i = 0; i < props.MemoryHeapCount; i++)
            {
                MemoryHeap memoryHeap = props.MemoryHeaps[i];

                output.WriteLine("\tmemoryHeaps[{0}] : ", i);
                output.WriteLine("\t\tsize          = {0} (0x{0:x})", (ulong)memoryHeap.Size);
				output.WriteLine("\t\tflags: ");
				VkListFlags(memoryHeap.Flags, "			", "VK_MEMORY_HEAP_", "_BIT", output);
			}
        }

        void AppGpuDump(AppGpu gpu, StreamWriter output)
        {
            output.WriteLine("Device Properties and Extensions :");
            output.WriteLine("==================================");
            output.WriteLine("GPU{0}", gpu.Id);
            AppGpuDumpProps(gpu, output);
            output.WriteLine();
            AppDumpExtensions("", "Device", gpu.DeviceExtensions, output);
			
            foreach (LayerExtensionList layerInfo in gpu.DeviceLayers)
            {
                uint major, minor, patch;

                ExtractVersion(layerInfo.LayerProperties.SpecVersion, out major, out minor, out patch);
                string specVersion = string.Format("{0}.{1}.{2}", major, minor, patch);
                string layerVersion = string.Format("{0}", layerInfo.LayerProperties.ImplementationVersion);

                output.WriteLine("\t{0} ({1}) Vulkan version {2}, layer version {3}",
                   layerInfo.LayerProperties.LayerName,
                   layerInfo.LayerProperties.Description, specVersion,
                   layerVersion);

               AppDumpExtensions("\t", layerInfo.LayerProperties.LayerName, layerInfo.ExtensionProperties, output);
            }

            output.WriteLine();

			for (uint i = 0; i < gpu.QueueProps.Length; i++)
            {
                AppGpuDumpQueuProps(gpu, i, output);
                output.WriteLine();
            }

            AppGpuDumpMemoryProps(gpu, output);
            output.WriteLine();
            AppGpuDumpFeatures(gpu, output);
            output.WriteLine();
            AppDevDump(gpu.Device, output);
        }

        public void DumpInfo(StreamWriter output)
        {
#if VulkanSharp
            uint apiVersion = Vulkan.Version.Make(1, 0, 39);
#elif Tanagra
            uint apiVersion = new Vulkan.Version(1, 0, 39);
#endif

            DumpHeader(apiVersion, output);

            AppInstance instance = AppCreateInstance(apiVersion);
            output.WriteLine("Instance Extensions:");
            output.WriteLine("====================");
            AppDumpExtensions("", "Instance", instance.Extensions, output);

			PhysicalDevice[] objs = instance.Instance.EnumeratePhysicalDevices();
			AppGpu[] gpus = new AppGpu[objs.Length];
			for (uint i = 0; i < objs.Length; i++)
			{
				gpus[i] = AppGpuInit(i, objs[i]);
			}

			output.WriteLine();
			output.WriteLine();
            output.WriteLine("Layers: count = {0}", instance.Layers.Length);
			output.WriteLine("=======");
			foreach (LayerExtensionList layer in instance.Layers)
			{
				LayerProperties layerProp = layer.LayerProperties;

				uint major, minor, patch;

				ExtractVersion(layerProp.SpecVersion, out major, out minor, out patch);
				string specVersion = string.Format("{0}.{1}.{2}", major, minor, patch);
				string layerVersion = string.Format("{0}", layerProp.ImplementationVersion);

				output.WriteLine("{0} ({1}) Vulkan version {2}, layer version {3}",
					layerProp.LayerName, layerProp.Description,
					specVersion, layerVersion);

				AppDumpExtensions("\t", "Layer", layer.ExtensionProperties, output);

				output.WriteLine("\tDevices 	count = {0}", gpus.Length);
				for (uint i = 0; i < gpus.Length; i++)
				{
					AppGpu gpu = gpus[i];
					string layerName = layer.LayerProperties.LayerName;
					ExtensionProperties[] props = gpu.Obj.EnumerateDeviceExtensionProperties(layerName);

					output.WriteLine("\t\tGPU id       : {0} ({1})", i, gpu.Props.DeviceName);					
					AppDumpExtensions("\t\t", "Layer-Device", props, output);
				}
				output.WriteLine();
			}

			output.WriteLine("Presentable Surfaces:");
			output.WriteLine("=====================");

			int format_count = 0;
			int present_mode_count = 0;

			instance.Width = 512;
			instance.Width = 512;

#if VK_USE_PLATFORM_WIN32_KHR
			const string VK_KHR_WIN32_SURFACE_EXTENSION_NAME = "VK_KHR_win32_surface";

			if (HasExtension(VK_KHR_WIN32_SURFACE_EXTENSION_NAME, instance.Extensions))
			{
				AppCreateWin32Window(instance);

				for (uint i = 0; i < gpus.Length; i++)
				{
					AppCreateWin32Surface(instance);

					output.WriteLine("GPU id       : {0} ({1})", i, gpus[i].Props.DeviceName);
					output.WriteLine("Surface type : {0}", VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
					format_count += AppDumpSurfaceFormats(instance, gpus[i], output);
					present_mode_count += AppDumpSurfacePresentModes(instance, gpus[i], output);

					AppDestroySurface(instance);
				}

				AppDestroyWin32Window(instance);
			}
#endif

			if (format_count <= 0 && present_mode_count <= 0)
			{
				output.WriteLine("None found\n");
			}

			output.WriteLine();
			output.WriteLine();

			for (uint i = 0; i < objs.Length; i++)
            {
                //gpus[i] = AppGpuInit(i, objs[i]);
                AppGpuDump(gpus[i], output);
                output.WriteLine();
                output.WriteLine();
            }

            for (uint i = 0; i < gpus.Length; i++)
            {
                AppGpuDestroy(gpus[i]);
            }

            AppDestroyInstance(instance);
            output.Flush();
        }

        void DumpHeader(uint apiVersion, StreamWriter output)
        {
            uint major, minor, patch;

            ExtractVersion(apiVersion, out major, out minor, out patch);

            output.WriteLine("===========");
            output.WriteLine("VULKAN INFO");
            output.WriteLine("===========\n");
            output.WriteLine("Vulkan API Version: {0}.{1}.{2}\n", major, minor, patch);
        }

        class AppInstance // app_instance 
        {
            public Instance Instance;
            public LayerExtensionList[] Layers;
            public ExtensionProperties[] Extensions;

			public int Width;
			public int Height;

			public SurfaceKhr Surface;

#if VK_USE_PLATFORM_WIN32_KHR
			public IntPtr Connection;
			public Form Window;
#endif
		}

        class LayerExtensionList // layer_extension_list 
        {
            public LayerProperties LayerProperties;
            public ExtensionProperties[] ExtensionProperties;
        }

        class AppGpu // app_gpu 
        {
            public uint Id;
            public PhysicalDevice Obj;
            public PhysicalDeviceProperties Props;

            public QueueFamilyProperties[] QueueProps;
            public DeviceQueueCreateInfo[] QueueReqs;

            public PhysicalDeviceMemoryProperties MemoryProps;
            public PhysicalDeviceFeatures Features;
            public PhysicalDeviceLimits Limits;

            public LayerExtensionList[] DeviceLayers;
            public ExtensionProperties[] DeviceExtensions;

            public AppDev Device;
        }

        class AppDev // app_dev 
        {
            public AppGpu Gpu;
            public Device Obj;
			//public FormatProperties[] FormatProbs; /*VK_FORMAT_RANGE_SIZE*/
			public Dictionary<Format, FormatProperties> FormatProbs;
        }

        struct Feature
        {
            public string Name;
            public FormatFeatureFlags Flags;
        }       
    }
}
