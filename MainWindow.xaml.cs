﻿using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace garrysmod_fixr_uppr
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly string version = "0.5";
		private string dir = @"C:\\Program Files (x86)\Steam\steamapps\common\GarrysMod\garrysmod";
		//private string steamdir = @"C:\\Program Files (x86)\Steam\config";
		public MainWindow()
		{
			InitializeComponent();
			header.Text = header.Text.Replace("$[version]", version);
			if (Process.GetProcessesByName("Steam").Length > 0)
			{
				MessageBox.Show("You Must Close Steam to use Some Features of this Program.");
			}
			refresh();
		}

		private Task getAlternateInstall()
		{
			return Task.Run(() => {
				for (byte i = 1; i < 255; i++)
				{
					if (!Directory.Exists(dir))
					{
						string search = "\"BaseInstallFolder_" + i + "\"";
						IEnumerable<String> lines = File.ReadLines(@"C:\\Program Files (x86)\Steam\config\config.vdf").Select(l => new { Line = l, Index = l.IndexOf(search) }).Where(x => x.Index > -1).Select(x => x.Line.Substring(x.Index + search.Length));
						foreach (string line in lines)
						{
							string linedir = line.Trim().Replace("\"", "") + @"\steamapps\common\GarrysMod";
							if (Directory.Exists(linedir))
							{
								dir = linedir + @"\garrysmod";
								break;
							}
						}
					}
				}
			});
		}

		private async void refresh()
		{
			if (Directory.Exists(dir))
			{
				StatusTXT.Foreground = Brushes.ForestGreen;
				StatusTXT.Text = "garrysmod folder found! (" + dir + ") Check what you want and click apply.";
			}
			else
			{
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
				string steamdir = key.GetValue("SteamPath").ToString() + @"/config";
				if (Directory.Exists(steamdir))
				{
					StatusTXT.Foreground = Brushes.Blue;
					StatusTXT.Text = "Steam program folder found!  Searching for garrysmod folder...";
					await getAlternateInstall();
					if (!Directory.Exists(dir))
					{
						StatusTXT.Foreground = Brushes.Red;
						StatusTXT.Text = "Steam program folder found but garrysmod folder NOT found!";
					}
					else
					{
						StatusTXT.Foreground = Brushes.ForestGreen;
						StatusTXT.Text = "garrysmod folder found! (" + dir + ") Check what you want and click apply.";
					}
				}
				else
				{
					StatusTXT.Text = "Steam install folder NOT found!";
					StatusTXT.Foreground = Brushes.Red;
				}
			}
			check0.IsEnabled = Process.GetProcessesByName("Steam").Length == 0 && (Directory.Exists(dir + @"\..\..\..\workshop\content\4000") || File.Exists(dir + @"\..\..\..\workshop\appworkshop_4000.acf"));
			check1.IsEnabled = Directory.Exists(dir + @"\cache");
			check2.IsEnabled = Directory.Exists(dir + @"\downloads");
			check3.IsEnabled = Directory.Exists(dir + @"\cfg") && File.Exists(dir + @"\cfg\client.vdf");
			check4.IsEnabled = File.Exists(dir+@"\cl.db") || File.Exists(dir + @"\mn.db");
			check5.IsEnabled = Directory.Exists(dir + @"\download");
			check6.IsEnabled = (Directory.Exists(dir + @"\..\bin") && (File.Exists(dir + @"\..\bin\analytics.dll") || File.Exists(dir + @"\..\bin\GameAnalytics.dll"))) || (Directory.Exists(dir + @"\..\bin\win64") && (File.Exists(dir + @"\..\bin\win64\analytics.dll") || File.Exists(dir + @"\..\bin\win64\GameAnalytics.dll")));
			Applybtn.IsEnabled = check0.IsEnabled || check1.IsEnabled || check2.IsEnabled || check3.IsEnabled || check4.IsEnabled || check5.IsEnabled || check6.IsEnabled;
			Applybtn.Click += ApplyBtnClick;
		}

		private void statusmsg(string msg, Brush color)
		{
			try
			{
				StatusTXT.Text = msg;
				StatusTXT.Foreground = color;
			}
			catch(Exception)
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
				StatusTXT.Text = "Deleting extracted cache folder...";
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
			if ((bool)check6.IsChecked)
			{
				StatusTXT.Foreground = Brushes.Blue;
				StatusTXT.Text = "Disabling telemetry DLLs...";
				await deletefile(dir + @"\..\bin\analytics.dll");
				await deletefile(dir + @"\..\bin\GameAnalytics.dll");
				if (Directory.Exists(dir + @"\..\bin\win64"))
				{
					await deletefile(dir + @"\..\bin\win64\analytics.dll");
					await deletefile(dir + @"\..\bin\win64\GameAnalytics.dll");
				}
				check6.IsChecked = false;
			}
			Applybtn.IsEnabled = true;
			refresh();
			StatusTXT.Foreground = Brushes.ForestGreen;
			StatusTXT.Text = "Done!";
		}
	}
}
