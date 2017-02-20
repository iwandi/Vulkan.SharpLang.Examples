using System;
using System.Collections.Generic;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanPipelineState : IPipelineState
	{
		Device device;

		bool pipelineValid = false;
		Pipeline pipeline;
		public Pipeline Pipeline
		{
			get
			{
				if (pipelineValid)
				{
					pipeline = CreatePipeline(device);
				}
				return pipeline;
			}
		}

		IShader pixelShader;
		public IShader PixelShader
		{
			get { return pixelShader; }
			set { pixelShader = value; pipelineValid = false; }
		}

		IShader vertexShader;
		public IShader VertexShader
		{
			get { return vertexShader; }
			set { vertexShader = value; pipelineValid = false; }
		}

		public VulkanPipelineState(Device device)
		{
			this.device = device;
		}

		protected Pipeline CreatePipeline(Device device)
		{
			List<DescriptorSetLayoutBinding> layoutBindings = new List<DescriptorSetLayoutBinding>();

			// TODO make this dynamic
			layoutBindings.Add(new DescriptorSetLayoutBinding
			{
				DescriptorType = DescriptorType.UniformBuffer,
				DescriptorCount = 1,
				StageFlags = ShaderStageFlags.Vertex,
			});

			DescriptorSetLayout descLayout = device.CreateDescriptorSetLayout(new DescriptorSetLayoutCreateInfo
			{
				Bindings = layoutBindings.ToArray(),
			});

			PipelineLayout layout = device.CreatePipelineLayout(new PipelineLayoutCreateInfo
			{
				SetLayouts = new DescriptorSetLayout[]
				{
					descLayout,
				}
			});

			Pipeline pipeline = device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[]
			{
				new GraphicsPipelineCreateInfo
				{
					Layout = layout,
					DynamicState = new PipelineDynamicStateCreateInfo
					{
						DynamicStates = new DynamicState[] { DynamicState.Viewport, DynamicState.Scissor }
					},
					// TODO set all the things
				}
			})[0];

			device.DestroyDescriptorSetLayout(descLayout);
			device.DestroyPipelineLayout(layout);

			return pipeline;	
		}

		public void Destroy()
		{
			if(pipeline != null)
			{
				device.DestroyPipeline(pipeline);
				pipeline = null;
			}
		}
	}
}
