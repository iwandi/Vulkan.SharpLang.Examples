using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Vulkan.SharpLang.Examples
{
	public class WindowsAppHost : IAppHost
	{
		List<WindowsWindow> windows = new List<WindowsWindow>();

		public void Init(AppInfo info)
		{

		}

		public void Destroy()
		{

		}


		public IWindow CreateWindow(WindowInfo info)
		{
			WindowsWindow wWindow = new WindowsWindow(info);
			windows.Add(wWindow);
			return wWindow;
		}

		public void DestroyWindow(IWindow window)
		{
			WindowsWindow wWindow = window as WindowsWindow;
			if(wWindow != null)
			{
				wWindow.Destroy();

				windows.Remove(wWindow);
			}
		}


		public bool HandleEvents()
		{
			Application.DoEvents();

			int validWindows = 0;
			foreach(WindowsWindow window in windows)
			{
				if(window.Valid)
				{
					validWindows++;
				}
			}
			return validWindows > 0;
		}
	}
}
