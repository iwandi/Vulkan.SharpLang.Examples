using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanImage
	{
		Format format;
		ImageLayout imageLayout;

		Image image;
		ImageView view;

		VulkanAlloc alloc;

		public Image Image {  get { return image; } }
		public ImageView View {  get { return view; } }

		internal VulkanImage(Image image, Format format)
		{
			this.image = image;
			this.imageLayout = ImageLayout.Undefined;
			this.format = format;
		}

		internal VulkanImage(Image image, ImageLayout imageLayout, Format format)
		{
			this.image = image;
			this.imageLayout = imageLayout;
			this.format = format;
		}

		public void CreateView(Device device, ImageAspectFlags aspectFlags = ImageAspectFlags.Color)
		{
			view = device.CreateImageView(new ImageViewCreateInfo
			{
				Image = image,
				Format = format,
				ViewType = ImageViewType.View2D,
				Components = new ComponentMapping
				{
					R = ComponentSwizzle.R,
					G = ComponentSwizzle.G,
					B = ComponentSwizzle.B,
					A = ComponentSwizzle.A,
				},
				SubresourceRange = new ImageSubresourceRange
				{
					AspectMask = aspectFlags,
					BaseMipLevel = 0,
					LevelCount = 1,
					BaseArrayLayer = 0,
					LayerCount = 1,
				},
			});
		}

		public void Alloc(Device device, VulkanMemoryManager manager)
		{
			if (alloc == null)
			{
				alloc = manager.Alloc(device.GetImageMemoryRequirements(image), (MemoryPropertyFlags)0);
			}
		}

		public void Free(VulkanMemoryManager manager)
		{
			if (alloc != null)
			{
				manager.Free(alloc);
				alloc = null;
			}
		}

		public void Bind(Device device)
		{
			device.BindImageMemory(image, alloc.Memory, alloc.Offset);
		}

		public void SetImageLayout(CommandBuffer cmd, ImageAspectFlags aspectMask, ImageLayout newImageLayout)
		{
			if(imageLayout == newImageLayout)
			{
				return;
			}

			ImageLayout oldImageLayout = imageLayout;

			ImageMemoryBarrier imageMemoryBarrier = new ImageMemoryBarrier
			{
				SrcAccessMask = 0,
				DstAccessMask = 0,
				OldLayout = oldImageLayout,
				NewLayout = newImageLayout,
				SrcQueueFamilyIndex = 0,
				DstQueueFamilyIndex = 0,
				Image = image,
				SubresourceRange = new ImageSubresourceRange
				{
					AspectMask = aspectMask,
					BaseMipLevel = 0,
					LevelCount = 1,
					BaseArrayLayer = 0,
					LayerCount = 1,
				},
			};

			if (oldImageLayout == ImageLayout.ColorAttachmentOptimal)
			{
				imageMemoryBarrier.SrcAccessMask = AccessFlags.ColorAttachmentWrite;
			}

			if (newImageLayout == ImageLayout.TransferDstOptimal)
			{
				imageMemoryBarrier.DstAccessMask = AccessFlags.TransferWrite;
			}

			if (newImageLayout == ImageLayout.TransferSrcOptimal)
			{
				imageMemoryBarrier.DstAccessMask = AccessFlags.TransferRead;
			}

			if (oldImageLayout == ImageLayout.TransferDstOptimal)
			{
				imageMemoryBarrier.SrcAccessMask = AccessFlags.TransferWrite;
			}

			if (oldImageLayout == ImageLayout.Preinitialized)
			{
				imageMemoryBarrier.SrcAccessMask = AccessFlags.HostWrite;
			}

			if (newImageLayout == ImageLayout.ShaderReadOnlyOptimal)
			{
				imageMemoryBarrier.DstAccessMask = AccessFlags.ShaderRead;
			}

			if (newImageLayout == ImageLayout.ColorAttachmentOptimal)
			{
				imageMemoryBarrier.DstAccessMask = AccessFlags.ColorAttachmentWrite;
			}

			if (newImageLayout == ImageLayout.DepthStencilAttachmentOptimal)
			{
				imageMemoryBarrier.DstAccessMask = AccessFlags.DepthStencilAttachmentWrite;
			}

			PipelineStageFlags srcStages = PipelineStageFlags.TopOfPipe;
			PipelineStageFlags destStages = PipelineStageFlags.TopOfPipe;

			cmd.CmdPipelineBarrier(srcStages, destStages, (DependencyFlags)0, null, null, new ImageMemoryBarrier[] { imageMemoryBarrier });

			imageLayout = newImageLayout;
		}

		public void Destroy(Device device, bool destroyImage = true)
		{
			if (view != null)
			{
				device.DestroyImageView(view);
				view = null;
			}

			if (image != null && destroyImage)
			{
				device.DestroyImage(image);
				image = null;
			}
		}
	}
}
