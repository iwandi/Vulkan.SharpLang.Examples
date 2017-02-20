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
		void Update(IWindow window);

		void NextFrame();
		void Begin(); // block Thread till SwapChain is ready for Rendering
		void End();
		void Present(); // Scedule a Frame ot be presented
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
		IGraphicsQueue QueueGraphics { get; }
		IGraphicsQueue QueueCompute { get; }
		IGraphicsQueue QueueTransfer { get; }

		void Init(AppInfo info);
		void Destroy();
		
		void InitDevice(); // TODO : allow gpu hint so we can feed this via config File

		ISwapChain CreateSwapChain(IWindow window);
		void DestroySwapChain(ISwapChain swapChian);

		IPipelineState CreatePipelineState();
		void DestroyPipelineState(IPipelineState pipelineState);

		IBuffer<T> CreateVertexBuffer<T>(int lenght) where T : struct;
		IBuffer<T> CreateUniformBuffer<T>() where T : struct; // Descriptor Buffer ?
		void DestroyBuffer(IBuffer buffer);

		IShader LoadShader(string fileName, string function, ShaderType type);
		void DestroyShader(IShader shader);
	}

	public interface IResourceManager
	{
		IShader CreateShader();
		IBuffer CreateBuffer();
	}

	public enum GraphicsQueueType
	{
		Graphics,
		Compute,
		Transfer,
		SparseBinding,
	}

	public enum ShaderType
	{
		Vertex,
		Pixel,
	}

	public interface IGraphicsQueue
	{
		void Reset();
		void Submit(ISwapChain swapChain);

		void SetPipelineState(IPipelineState pipeline);
		void Bind(IBuffer buffer);
		void Draw(IBuffer vertexBuffer);
	}

	public interface IShader
	{

	}

	public interface IPipelineState
	{
		IShader VertexShader { get; set; }
		IShader PixelShader { get; set; }
	}

	public interface IBuffer
	{

	}

	public interface IBuffer<T> : IBuffer
	{
		void Write(T data, int targetIndex);
		void Write(T[] data, int sourceIndex, int targetIndex, int count);
	}

	public static class BufferHelper
	{
		public static void Write<T>(this IBuffer<T> buffer, T data)
		{
			buffer.Write(data, 0);
		}

		public static void Write<T>(this IBuffer<T> buffer, T[] data)
		{
			buffer.Write(data, 0, 0, data.Length);
		}
	}

	public interface IVertexBuffer : IBuffer
	{

	}

	public interface IVertexBuffer<T> : IBuffer<T>, IVertexBuffer
	{

	}

	public interface IDescriptorSet<T> : IBuffer<T>
	{

	}

	public interface IRenderPipeline
	{

	}

	public interface IRenderQueue
	{

	}

	public interface ICmdRecord
	{

	}

	public interface IRenderRecorder
	{
		ICmdRecord ToRecord();
	}

	public interface IGaphicsComandBuffer
	{
		void BeginRenderPipeline(IRenderPipeline pipeline);
		void EndRenderPipeline(IRenderPipeline pipeline);

		void SetViewPort(ISwapChain swapChain);
		void SetViewPort(Rect2D pos);

		void ClearImage(Image image);
		void SetShader(IShader shader);
		void WriteDescriptor(IBuffer buffer);
		void BindBuffer(IBuffer buffer); // TODO raget
		void BindImage(Image image);

		void Render(); // TODO set render Stuff
	}
}
