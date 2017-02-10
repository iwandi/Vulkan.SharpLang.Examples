using System;
using Vulkan;
using Vulkan.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Vulkan.SharpLang.Examples
{
	class Program
	{
		static void Main(string[] args)
		{
			AppInfo appInfo = new AppInfo
			{
				Title = "Cube",
				Version = 1,
				EngineTitle = "Cube",
				EngineVersion = 1,
			};

			IAppHost host = new WindowsAppHost();
			host.Init(appInfo);
			IWindow window = host.CreateWindow(new WindowInfo
			{
				Title = appInfo.Title,
				ContentWidth = 1280,
				ContentHeight = 720,
				Fullscreen = FullscreenState.Windowed,
			});

			IGraphics graphics = new VulkanGraphics();
			graphics.Init(appInfo);

			//IGraphicsQueue graphicsQueue = graphics.CreateQueue(GraphicsQueueType.Graphics);

			graphics.InitDevice();

			ISwapChain surface = graphics.CreateSwapChain(window);

			while(host.HandleEvents())
			{
				graphics.NextFrame(surface);
				//graphics.UpdatePresentationSurface(surface, window);
				graphics.PresentFrame(surface);
			}

			graphics.DestroySwapChain(surface);
			graphics.Destroy();

			host.DestroyWindow(window);
			host.Destroy();
		}
	}
}
