using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
    class SampleUniformBuffer
    {
        static void Main(string[] args)
        {
            SampleInstance sample = new SampleInstance();
            sample.InitGlobalLayerProperties();
            Instance instance = sample.InitInstance("Uniform Buffer Sample");
            sample.InitEnumerateDevice();
            sample.InitQueueFamilyIndex();
            Device device = sample.InitDevice();
            sample.InitWindowSize(50, 50);
            
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(((float)Math.PI / 180f) *  45f, 1f, 0.1f, 100f); // TODO deg top rad
            Matrix4x4 view = Matrix4x4.CreateLookAt(
                new Vector3(0f, 3f, 10f), // Camera is at (0,3,10), in World Space
                new Vector3(0f, 0f, 0f),  // and looks at the origin
                new Vector3(0f, -1f, 0f)  // Head is up (set to 0,-1,0 to look upside-down)
                );
            Matrix4x4 model = Matrix4x4.Identity;
            // Vulkan clip space has inverted Y and half Z.
            Matrix4x4 clip = new Matrix4x4 (
                1f,  0f,   0f, 0f,
                0f, -1f,   0f, 0f,
                0f,  0f, 0.5f, 0f,
                0f,  0f, 0.5f, 1f );

            Matrix4x4 mvp = clip * projection * view * model;

            /* VULKAN_KEY_START */
            BufferCreateInfo bufInfo = new BufferCreateInfo
            {
                Usage = BufferUsageFlags.UniformBuffer,
                Size = Marshal.SizeOf<Matrix4x4>(),
                QueueFamilyIndices = new uint[0],
                SharingMode = SharingMode.Exclusive,
                Flags = 0,
            };

            Buffer uniformDataBuf = device.CreateBuffer(bufInfo);

            MemoryRequirements memReqs = device.GetBufferMemoryRequirements(uniformDataBuf);

            MemoryAllocateInfo allocInfo = new MemoryAllocateInfo
            {
                MemoryTypeIndex = 0,
                AllocationSize = memReqs.Size,
            };

            uint memoryTypeIndex;
            bool pass = sample.MemoryTypeFromProperties(memReqs.MemoryTypeBits, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out memoryTypeIndex);
            allocInfo.MemoryTypeIndex = memoryTypeIndex;

            Debug.Assert(pass);

            DeviceMemory uniformDataMem = device.AllocateMemory(allocInfo);

            IntPtr pData = device.MapMemory(uniformDataMem, 0, memReqs.Size);

            Marshal.StructureToPtr<Matrix4x4>(mvp, pData, false);

            device.UnmapMemory(uniformDataMem);

            device.BindBufferMemory(uniformDataBuf, uniformDataMem, 0);

            // TODO : Final Mapping ?

            /* VULKAN_KEY_END */

            device.DestroyBuffer(uniformDataBuf);
            device.FreeMemory(uniformDataMem);
            device.Destroy();
            instance.Destroy();
        }
    }
}
