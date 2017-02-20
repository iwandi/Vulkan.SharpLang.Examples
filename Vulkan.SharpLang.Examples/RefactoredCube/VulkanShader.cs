using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanShader : IShader
	{
		ShaderModule module;
		PipelineShaderStageCreateInfo info;

		public VulkanShader(ShaderModule module, PipelineShaderStageCreateInfo info)
		{
			this.module = module;
			this.info = info;
		}

		public void Destroy(Device device)
		{
			if(module != null)
			{
				device.DestroyShaderModule(module);
				module = null;
			}
		}
	}
}
