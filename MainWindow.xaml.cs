using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace garrysmod_fixr_uppr
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string dir = @"C:\Program Files (x86)\Steam\steamapps\common\GarrysMod\garrysmod";
		public MainWindow()
		{
			InitializeComponent();
			if (Directory.Exists(dir))
			{
				StatusTXT.Foreground = Brushes.ForestGreen;
				StatusTXT.Text = "garrysmod folder found! Check what you want and click apply.";
				if (Process.GetProcessesByName("Steam").Length > 0)
				{
					MessageBox.Show("You Must Close Steam to use Some Features of this Program.");
				}
			}
			else
			{
				StatusTXT.Text = "garrysmod folder NOT found!";
				StatusTXT.Foreground = Brushes.Red;
			}
			Applybtn.Click += ApplyBtnClick;
			refresh();
		}

		private void refresh()
		{
			check0.IsEnabled = Process.GetProcessesByName("Steam").Length == 0 && (Directory.Exists(dir + @"\..\..\..\workshop\content\4000") || File.Exists(dir + @"\..\..\..\workshop\appworkshop_4000.acf"));
			check1.IsEnabled = Directory.Exists(dir + @"\cache");
			check2.IsEnabled = Directory.Exists(dir + @"\downloads");
			check3.IsEnabled = Directory.Exists(dir + @"\cfg") && File.Exists(dir + @"\cfg\client.vdf");
			check4.IsEnabled = File.Exists(dir+@"\cl.db") || File.Exists(dir + @"\mn.db");
			check5.IsEnabled = Directory.Exists(dir + @"\download");
			check6.IsEnabled = false;
		}

		private void statusmsg(string msg, Brush color)
		{
			try
			{
				StatusTXT.Text = msg;
				StatusTXT.Foreground = color;
			}
			catch(Exception e)
			{

			}
		}

		private Task deletefile(string path)
		{
			return Task.Run(() => {
				try
				{
					File.Delete(path);
				}
				catch (Exception e)
				{
					statusmsg(e.Message, Brushes.Red);
				}
			});
		}

		private Task deletefolder(string path)
		{
			return Task.Run(() => {
				try
				{
					Directory.Delete(path, true);
				}
				catch (Exception e)
				{
					statusmsg(e.Message, Brushes.Red);
				}
			});
		}

		private async void ApplyBtnClick(object sender, RoutedEventArgs e)
		{
			Applybtn.IsEnabled = false;
			if ((bool)check0.IsChecked)
			{
				if (Process.GetProcessesByName("Steam").Length > 0)
				{
					MessageBox.Show("You Must Close Steam to use this feature! skipping...");
				}
				else
				{
					StatusTXT.Foreground = Brushes.Blue;
					StatusTXT.Text = "Cleaning and Fixing Downloaded Workshop Cache...";
					await deletefolder(dir + @"\..\..\..\workshop\content\4000");
					await deletefile(dir + @"\..\..\..\workshop\appworkshop_4000.acf");
				}
				check0.IsChecked = false;
			}
			if ((bool)check1.IsChecked)
			{
				StatusTXT.Foreground = Brushes.Blue;
				StatusTXT.Text = "Deleting LUA cache folder...";
				await deletefolder(dir + @"\cache");
				check1.IsChecked = false;
			}
			if ((bool)check2.IsChecked)
			{
				StatusTXT.Foreground = Brushes.Blue;
				StatusTXT.Text = "Deleting downloaded server content folder...";
				await deletefolder(dir + @"\downloads");
				await deletefile(dir + @"\map.pack");
				check2.IsChecked = false;
			}
			if ((bool)check3.IsChecked)
			{
				StatusTXT.Foreground = Brushes.Blue;
				StatusTXT.Text = "Deleting VDF file...";
				await deletefile(dir + @"\cfg\client.vdf");
				check3.IsChecked = false;
			}
			if ((bool)check4.IsChecked)
			{
				StatusTXT.Foreground = Brushes.Blue;
				StatusTXT.Text = "Deleting DB file(s)...";
				await deletefile(dir + @"\cl.db");
				await deletefile(dir + @"\mn.db");
				check4.IsChecked = false;
			}
			if ((bool)check5.IsChecked)
			{
				StatusTXT.Foreground = Brushes.Blue;
				StatusTXT.Text = "Cleaning cached other players' sprays...";
				await deletefolder(dir + @"\download");
				check5.IsChecked = false;
			}
			Applybtn.IsEnabled = true;
			refresh();
			StatusTXT.Foreground = Brushes.ForestGreen;
			StatusTXT.Text = "Done!";
		}
	}
}
