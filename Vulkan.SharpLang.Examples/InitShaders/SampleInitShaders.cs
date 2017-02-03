using System;
using System.Diagnostics;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	class SampleInitShaders
	{
		static void Main(string[] args)
		{
			SampleInstance sample = new SampleInstance();
			sample.InitGlobalLayerProperties();
			Instance instance = sample.InitInstance("Initialize Shaders Sample");
			sample.InitEnumerateDevice();
			sample.InitQueueFamilyIndex();
			Device device = sample.InitDevice();

			PipelineShaderStageCreateInfo vertInfo = new PipelineShaderStageCreateInfo
			{
				Stage = ShaderStageFlags.Vertex,
				Name = "main",
			};
			
			uint[] vertSpv = sample.LoadSpvFile("basicShader.vert.spv");

			ShaderModule vertModule = device.CreateShaderModule(new ShaderModuleCreateInfo
			{
				Code = vertSpv,
			});

			PipelineShaderStageCreateInfo fragInfo = new PipelineShaderStageCreateInfo
			{
				Stage = ShaderStageFlags.Fragment,
				Name = "main",
			};
			
			uint[] fragSpv = sample.LoadSpvFile("basicShader.frag.spv");

			ShaderModule fragModule = device.CreateShaderModule(new ShaderModuleCreateInfo
			{
				Code = fragSpv,
			});

			device.DestroyShaderModule(vertModule);
			device.DestroyShaderModule(fragModule);

			device.Destroy();
			instance.Destroy();
		}
	}
}
