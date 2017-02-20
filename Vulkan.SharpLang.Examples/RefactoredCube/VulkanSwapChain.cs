using System;
using Vulkan;

namespace Vulkan.SharpLang.Examples
{
	public class VulkanSwapChain : ISwapChain
	{
		Device device;
		VulkanQueue queue;

		SurfaceKhr surface;

		Extent2D extend;
		Format format;
		SwapchainKhr swapChain;
		VulkanImage[] buffers;
		RenderPass renderPass;
		Framebuffer[] frameBuffers;
		uint currentBuffer;

		Format depthBufferFormat;
		VulkanImage depthBuffer;

		Semaphore imageAcquiredSemaphore;

		public Extent2D Extend { get { return extend; } }
		public SwapchainKhr SwapChain {  get { return swapChain; } }

		public VulkanImage CurrentBuffer {  get { return buffers[currentBuffer]; } }
		public Image CurrentBufferImage {  get { return CurrentBuffer.Image; } }
		public ImageView CurrentBufferView { get { return CurrentBuffer.View; } }
		public Framebuffer CurrentFrameBuffer { get { return frameBuffers[currentBuffer]; } }

		public Semaphore ImageAcquiredSemaphore { get { return imageAcquiredSemaphore; } }

		internal VulkanSwapChain(SurfaceKhr surface)
		{
			this.surface = surface;
		}

		internal void Init(PhysicalDevice gpu, Device device, VulkanMemoryManager  memory, VulkanQueue queue, uint width, uint height)
		{
			this.device = device;
			this.queue = queue;

			UpdateExtends(gpu, width, height);

			InitSwapChain(gpu, device, width, height);
			InitBuffers(device);
			InitDepthBuffer(gpu, device, memory, queue.Cmd);
			InitRenderPass(device);
			InitFrmeBuffer(device);

			imageAcquiredSemaphore = device.CreateSemaphore(new SemaphoreCreateInfo { });
		}

		protected void UpdateExtends(PhysicalDevice gpu, uint width, uint height)
		{
			SurfaceCapabilitiesKhr surfaceCap = gpu.GetSurfaceCapabilitiesKHR(surface);
			extend = new Extent2D
			{
				Width = width,
				Height = height,
			};
			if (surfaceCap.CurrentExtent.Width != uint.MaxValue)
			{
				extend = surfaceCap.CurrentExtent;
			}
		}

		protected Format GetSwapChainFormat(Format targetFormat, SurfaceFormatKhr[] surfFormats)
		{
			if (surfFormats.Length == 1 && surfFormats[0].Format == Format.Undefined)
			{
				return targetFormat;
			}
			foreach (SurfaceFormatKhr surfFormat in surfFormats)
			{
				if (surfFormat.Format == targetFormat)
				{
					return targetFormat;
				}
			}
			throw new Exception("SwapChain format not supported " + targetFormat);
		}

		protected PresentModeKhr GetPresentMode(PresentModeKhr[] presentModes)
		{
			bool hasMailbox = false;
			bool hasImediate = false;
			foreach (PresentModeKhr presentModeCheck in presentModes)
			{
				switch(presentModeCheck)
				{
					case PresentModeKhr.Mailbox:
						hasMailbox = true;
						break;
					case PresentModeKhr.Immediate:
						hasImediate = true;
						break;
				}
			}
			if(hasMailbox )
			{
				return PresentModeKhr.Mailbox;
			}
			else if(hasImediate)
			{
				return PresentModeKhr.Immediate;
			}
			return PresentModeKhr.Fifo;
		}

		protected void InitSwapChain(PhysicalDevice gpu, Device device, uint width, uint height)
		{
			int queueCount = gpu.GetQueueFamilyProperties().Length;
			bool[] supportsPresent = new bool[queueCount];
			for (uint i = 0; i < supportsPresent.Length; i++)
			{
				supportsPresent[i] = gpu.GetSurfaceSupportKHR(i, surface);
			}
			// TODO : at this point we need to select the correct Queue

			format = GetSwapChainFormat(Format.B8G8R8A8Unorm, gpu.GetSurfaceFormatsKHR(surface));			

			SurfaceCapabilitiesKhr surfaceCap = gpu.GetSurfaceCapabilitiesKHR(surface);
			uint numImages = surfaceCap.MinImageCount + 1;
			if (surfaceCap.MaxImageCount > 0 &&
				numImages > surfaceCap.MaxImageCount)
			{
				numImages = surfaceCap.MaxImageCount;
			}

			// TODO : is this a thing we need to handle on Mobile ?
			SurfaceTransformFlagsKhr preTransform = surfaceCap.CurrentTransform;
			if ((preTransform & SurfaceTransformFlagsKhr.Identity) == SurfaceTransformFlagsKhr.Identity)
			{
				preTransform = SurfaceTransformFlagsKhr.Identity;
			}

			PresentModeKhr presentMode = GetPresentMode(gpu.GetSurfacePresentModesKHR(surface));

			swapChain = device.CreateSwapchainKHR(new SwapchainCreateInfoKhr
			{
				Surface = surface,
				MinImageCount = numImages,
				ImageFormat = format,
				ImageExtent = extend,
				PreTransform = preTransform,
				PresentMode = presentMode,
				CompositeAlpha = CompositeAlphaFlagsKhr.Opaque,
				ImageArrayLayers = 1,
				Clipped = true,
				ImageColorSpace = ColorSpaceKhr.SrgbNonlinear,
				ImageUsage = ImageUsageFlags.ColorAttachment,
				ImageSharingMode = SharingMode.Exclusive,
			});
		}

		protected void InitBuffers(Device device)
		{
			Image[] swapChainImages = device.GetSwapchainImagesKHR(swapChain);
			buffers = new VulkanImage[swapChainImages.Length];
			for (uint i = 0; i < swapChainImages.Length; i++)
			{
				VulkanImage bufferImage = new VulkanImage(swapChainImages[i], format);
				bufferImage.CreateView(device);
				buffers[i] = bufferImage;
			}

			currentBuffer = 0;
		}

		protected ImageTiling GetDepthBufferTiling(PhysicalDevice gpu, Format targetFormat)
		{
			FormatProperties props = gpu.GetFormatProperties(targetFormat);
			if ((props.LinearTilingFeatures & FormatFeatureFlags.DepthStencilAttachment) == FormatFeatureFlags.DepthStencilAttachment)
			{
				return ImageTiling.Linear;
			}
			else if ((props.OptimalTilingFeatures & FormatFeatureFlags.DepthStencilAttachment) == FormatFeatureFlags.DepthStencilAttachment)
			{
				return ImageTiling.Optimal;
			}
			else
			{
				/* Try other depth formats? */
				Console.WriteLine("VK_FORMAT_D16_UNORM Unsupported.");
				throw new Exception("VK_FORMAT_D16_UNORM Unsupported.");
			}
		}

		protected void InitDepthBuffer(PhysicalDevice gpu, Device device, VulkanMemoryManager memory, CommandBuffer cmd)
		{
			depthBufferFormat = Format.D16Unorm;
			ImageTiling tiling = GetDepthBufferTiling(gpu, depthBufferFormat);

			depthBuffer = new VulkanImage(device.CreateImage(new ImageCreateInfo
			{
				Format = depthBufferFormat,
				Tiling = tiling,
				ImageType = ImageType.Image2D,
				Extent = VulkanHelper.ToExtend3D(extend, 1),
				MipLevels = 1,
				ArrayLayers = 1,
				Samples = SampleCountFlags.Count1,
				InitialLayout = ImageLayout.Undefined,
				Usage = ImageUsageFlags.DepthStencilAttachment,
				SharingMode = SharingMode.Exclusive,
			}), depthBufferFormat);

			depthBuffer.Alloc(device, memory);
			depthBuffer.Bind(device);
			depthBuffer.SetImageLayout(cmd, ImageAspectFlags.Depth, ImageLayout.DepthStencilAttachmentOptimal);
			depthBuffer.CreateView(device, ImageAspectFlags.Depth);// | ImageAspectFlags.Stencil);
		}

		protected void InitRenderPass(Device device)
		{
			renderPass = device.CreateRenderPass(new RenderPassCreateInfo
			{
				Attachments = new AttachmentDescription[]
				{
					new AttachmentDescription
					{
						Format = format,
						Samples = SampleCountFlags.Count1,
						LoadOp = AttachmentLoadOp.Clear,
						StoreOp = AttachmentStoreOp.DontCare,
						StencilLoadOp = AttachmentLoadOp.DontCare,
						StencilStoreOp = AttachmentStoreOp.DontCare,
						InitialLayout = ImageLayout.Undefined,
						FinalLayout = ImageLayout.PresentSrcKhr,
					},
					new AttachmentDescription
					{
						Format = depthBufferFormat,
						Samples = SampleCountFlags.Count1,
						LoadOp = AttachmentLoadOp.Clear,
						StoreOp = AttachmentStoreOp.Store,
						StencilLoadOp = AttachmentLoadOp.Load,
						StencilStoreOp = AttachmentStoreOp.Store,
						InitialLayout = ImageLayout.Undefined,
						FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
					},
				},
				Subpasses = new SubpassDescription[]
				{
					new SubpassDescription
					{
						ColorAttachments = new AttachmentReference[]
						{
							new AttachmentReference
							{
								Attachment = 0,
								Layout = ImageLayout.ColorAttachmentOptimal,
							},
						},
						PipelineBindPoint = PipelineBindPoint.Graphics,
					}
				},
			});
		}

		protected void InitFrmeBuffer(Device device)
		{
			frameBuffers = new Framebuffer[buffers.Length];

			for(int i = 0; i < buffers.Length; i++)
			{
				frameBuffers[i] = device.CreateFramebuffer(new FramebufferCreateInfo
				{
					RenderPass = renderPass,
					Attachments = new ImageView[]
					{
						buffers[i].View,
						depthBuffer.View,
					},
					Width = extend.Width,
					Height = extend.Height,
					Layers = 1,
				});
			}
		}

		public void Update(IWindow window)
		{
			WindowInfo info = window.Info;
			//extend = new Extent2D { Width = (uint)info.ContentWidth, Height = (uint)info.ContentHeight };

			// TODO Create a bigger Frame buffer if we have to grow
		}

		public void NextFrame()
		{
			currentBuffer = device.AcquireNextImageKHR(swapChain, uint.MaxValue, imageAcquiredSemaphore);
		}

		public void Begin()
		{
			Begin(device, queue.Cmd, imageAcquiredSemaphore);
		}

		protected void Begin(Device device, CommandBuffer cmd, Semaphore imageAcquiredSemaphore)
		{
			cmd.CmdBeginRenderPass(new RenderPassBeginInfo
			{
				RenderPass = renderPass,
				Framebuffer = CurrentFrameBuffer,
				RenderArea = VulkanHelper.ToRect2D(extend),
				ClearValues = new ClearValue[]
				{
					new ClearValue
					{
						// TODO : allow clear color by data
						//Color = new ClearColorValue (new float[] { 0.0f, 0.5f, 1.0f, 0.2f }),
						Color = new ClearColorValue (new float[] { 0.15f, 0.15f, 0.15f, 0.2f }),
					},
					new ClearValue
					{
						DepthStencil = new ClearDepthStencilValue
						{
							Depth = 1f,
							Stencil = 0,
						},
					},
				},
			}, SubpassContents.Inline);
		}

		public void End()
		{
			End( queue.Cmd);
		}

		protected void End(CommandBuffer cmd)
		{
			cmd.CmdEndRenderPass();
		}

		public void Present()
		{
			Present(device, queue.Queue, queue.SubmitFence);
		}

		protected void Present(Device device, Queue queue, Fence fence)
		{
			bool check = true;
			do
			{
				try
				{
					const uint timeout = 1000000;
					device.WaitForFence(fence, true, timeout);
					check = false;
				}
				catch(ResultException ex)
				{
					if(ex.Result != Result.Timeout)
					{
						Console.WriteLine(ex.Message);
						Console.WriteLine(ex.StackTrace);
						check = false;
					}
				}
			}
			while (check);

			queue.PresentKHR(new PresentInfoKhr
			{
				Swapchains = new SwapchainKhr[] { swapChain },
				ImageIndices = new uint[] { currentBuffer },
			});
		}

		internal void Destroy(Instance instance, Device device, VulkanMemoryManager memory)
		{
			if(frameBuffers != null)
			{
				foreach(Framebuffer frameBuffer in frameBuffers)
				{
					device.DestroyFramebuffer(frameBuffer);
				}
				frameBuffers = null;
			}

			if(renderPass != null)
			{
				device.DestroyRenderPass(renderPass);
				renderPass = null;
			}

			if(imageAcquiredSemaphore != null)
			{
				device.DestroySemaphore(imageAcquiredSemaphore);
				imageAcquiredSemaphore = null;
			}

			if (depthBuffer != null)
			{
				depthBuffer.Destroy(device);
				depthBuffer.Free(memory);
				depthBuffer = null;
			}

			if(buffers != null)
			{
				foreach(VulkanImage image in buffers)
				{
					image.Destroy(device, false);
				}
				buffers = null;
			}

			if(swapChain != null)
			{
				device.DestroySwapchainKHR(swapChain);
				swapChain = null;
			}

			if (surface != null)
			{
				instance.DestroySurfaceKHR(surface);
				surface = null;
			}
		}
	}
}
