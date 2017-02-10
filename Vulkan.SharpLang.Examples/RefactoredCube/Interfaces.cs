using System;

namespace Vulkan.SharpLang.Examples
{
	public interface IWindow
	{
		WindowInfo Info { get; set; }
	}

	public class WindowInfo
	{
		public string Title;
		public int ContentWidth;
		public int ContentHeight;
		public FullscreenState Fullscreen;
	}

	public enum FullscreenState
	{
		Windowed,
		Fullscreen,
		WindowedFullscreen,
	}

	public interface IAppHost
	{
		void Init(AppInfo info);
		void Destroy();

		IWindow CreateWindow(WindowInfo info);
		void DestroyWindow(IWindow window);

		bool HandleEvents();			
	}

	public interface ISwapChain
	{

	}

	public class AppInfo
	{
		public string Title;
		public uint Version;
		public string EngineTitle;
		public uint EngineVersion;
		public bool Debug;
	}

	public interface IGraphics
	{
		void Init(AppInfo info);
		void Destroy();

		//IGraphicsQueue CreateQueue(GraphicsQueueType type);

		void InitDevice(); // TODO : allow gpu hint so we can feed this via config File

		ISwapChain CreateSwapChain(IWindow window);
		//void UpdatePresentationSurface(ISwapChain surface, IWindow window);
		void DestroySwapChain(ISwapChain swapChian);

		void NextFrame(ISwapChain swapChian);
		void PresentFrame(ISwapChain swapChian);
	}

	public enum GraphicsQueueType
	{
		Graphics,
		Compute,
		Transfer,
		SparseBinding,
	}

	/*public interface IGraphicsQueue
	{

	}*/
}
