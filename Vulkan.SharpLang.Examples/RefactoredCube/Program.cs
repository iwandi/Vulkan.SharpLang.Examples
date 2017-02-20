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
		static Vert[] cube = new Vert[]
		{
			new Vert { },
		};

		static void Main(string[] args)
		{
			AppInfo appInfo = new AppInfo
			{
				Title = "Cube",
				Version = 1,
				EngineTitle = "Cube",
				EngineVersion = 1,
				Debug = true,
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
			graphics.InitDevice();
			IGraphicsQueue graphicsQueue = graphics.QueueGraphics;
			ISwapChain swapChain = graphics.CreateSwapChain(window);

			IBuffer<Vert> vb = graphics.CreateVertexBuffer<Vert>(cube.Length);
			vb.Write(cube);

			IBuffer<MVP> mvp = graphics.CreateUniformBuffer<MVP>(); 
			mvp.Write(new MVP
			{

			});

			IShader vsShader = graphics.LoadShader("basicShader.vert.spv", "main", ShaderType.Vertex);
			IShader psShader = graphics.LoadShader("basicShader.frag.spv", "main", ShaderType.Pixel);

			IPipelineState pipe = graphics.CreatePipelineState();
			pipe.VertexShader = vsShader;
			pipe.PixelShader = psShader;
			// TODO : set uniform buffer

			while (host.HandleEvents())
			{
				swapChain.NextFrame();
				graphicsQueue.Reset();
				swapChain.Begin();

				graphicsQueue.SetPipelineState(pipe);
				graphicsQueue.Bind(mvp);
				graphicsQueue.Draw(vb);

				swapChain.End();
				graphicsQueue.Submit(swapChain);
				swapChain.Present();
			}

			graphics.DestroyShader(vsShader);
			graphics.DestroyShader(psShader);
			graphics.DestroyBuffer(vb);
			graphics.DestroyBuffer(mvp);

			graphics.DestroySwapChain(swapChain);
			graphics.Destroy();

			host.DestroyWindow(window);
			host.Destroy();

			Console.ReadLine();
		}

		public struct Vert
		{
			public Vector4 Pos;
		}

		public struct MVP
		{
			public Matrix4x4 ModelViewProjection;
		}
	}
}
