using System;
using System.Windows.Forms;
using System.Drawing;

namespace Vulkan.SharpLang.Examples
{
	public class WindowsWindow : IWindow
	{
		Form window;

		public bool Valid
		{
			get
			{
				if(window != null)
				{
					return window.Created;
				}
				return false;
			}
		}

		public WindowInfo Info
		{
			get
			{
				return new WindowInfo
				{
					Title = window.Text,
					ContentWidth = window.ClientSize.Width,
					ContentHeight = window.ClientSize.Height,
				};
			}
			set
			{
				window.Text = value.Title;
				window.ClientSize = new Size
				{
					Width = value.ContentWidth,
					Height = value.ContentHeight,
				};
			}
		}

		public WindowsWindow(WindowInfo info)
		{
			window = new Form();
			this.Info = info;
			window.Show();
		}

		public void Destroy()
		{
			window.Close();
			window = null;
		}
	}
}
