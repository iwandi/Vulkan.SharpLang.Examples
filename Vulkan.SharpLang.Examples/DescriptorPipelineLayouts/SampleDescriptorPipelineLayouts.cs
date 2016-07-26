using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vulkan;
using Vulkan.Windows;

namespace Vulkan.SharpLang.Examples
{
    class SampleDescriptorPipelineLayouts
    {
        static void Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            sample.InitGlobalLayerProperties();
            Instance instance = sample.InitInstance("Descriptor / Pipeline Layout Sample");
            sample.InitEnumerateDevice();
            sample.InitQueueFamilyIndex();
            Device device = sample.InitDevice();

            /* VULKAN_KEY_START */
            /* Start with just our uniform buffer that has our transformation matrices
             * (for the vertex shader). The fragment shader we intend to use needs no
             * external resources, so nothing else is necessary
             */

            /* Note that when we start using textures, this is where our sampler will
             * need to be specified
             */
            DescriptorSetLayoutBinding layoutBinding = new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.Vertex,
                ImmutableSamplers = null,
            };

            /* Next take layout bindings and use them to create a descriptor set layout
             */
            DescriptorSetLayoutCreateInfo descriptorLayout = new DescriptorSetLayoutCreateInfo
            {
                BindingCount = 1,
                Bindings = new DescriptorSetLayoutBinding[] { layoutBinding },
            };

            DescriptorSetLayout descLayout = device.CreateDescriptorSetLayout(descriptorLayout);

            /* Now use the descriptor layout to create a pipeline layout */
            PipelineLayoutCreateInfo pPipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            {
                PushConstantRanges = new PushConstantRange[0],
                SetLayouts = new DescriptorSetLayout[] { descLayout },
            };

            PipelineLayout pipelineLayout = device.CreatePipelineLayout(pPipelineLayoutCreateInfo);
            /* VULKAN_KEY_END */

            device.DestroyDescriptorSetLayout(descLayout);
            device.DestroyPipelineLayout(pipelineLayout);
            device.Destroy();
            instance.Destroy();
        }
    }
}
