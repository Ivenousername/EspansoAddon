using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.ComponentModel;
using Microsoft.Win32;

namespace EspansoAddon
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public ObservableCollection<Item> items { get; set; } = new ObservableCollection<Item>();
		private string pathPath;
		private string path;

		private string _tbReplace;
		public string TbReplace
		{
			get => _tbReplace;
			set
			{
				_tbReplace = value;
				OnPropertyChanged(nameof(TbReplace));
			}
		}

		private string _tbTrigger;
		public string TbTrigger
		{
			get => _tbTrigger;
			set
			{
				_tbTrigger = value;
				OnPropertyChanged(nameof(TbTrigger));
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = this;

			pathPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "path.txt");

			if (File.Exists(pathPath))
			{
				path = File.ReadAllText(pathPath);
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
				{
					if (LoadEspansoFile())
					{
						fileTextBlock.Text = path;
					}
					else
					{
						fileTextBlock.Text = "Invalid YAML Format";
					}
				}
				else
				{
					fileTextBlock.Text = "Target file missing";
				}
			}
			else
			{
				path = string.Empty;
				fileTextBlock.Text = "No file selected";
			}

			TheList.ItemsSource = items;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void SaveEspansoFile()
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

			try
			{
				var serializer = new SerializerBuilder()
					.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.Build();

				var config = new EspansoConfig { Matches = new List<Item>(items) };
				string yamlOutput = serializer.Serialize(config);

				File.WriteAllText(path, yamlOutput);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to save: {ex.Message}");
			}
		}

		private bool LoadEspansoFile()
		{
			try
			{
				if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;

				string yamlContent = File.ReadAllText(path);
				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.IgnoreUnmatchedProperties()
					.Build();

				var config = deserializer.Deserialize<EspansoConfig>(yamlContent);

				items.Clear();
				if (config?.Matches != null)
				{
					foreach (var match in config.Matches)
					{
						if (!string.IsNullOrEmpty(match.Trigger))
						{
							items.Add(match);
						}
					}
					return true;
				}

				return config != null;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"YAML Error: {ex.Message}");
				return false;
			}
		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			if (TheList.SelectedItem is Item selectedItem)
			{
				items.Remove(selectedItem);
				SaveEspansoFile();
			}
		}

		private void Add_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(TbTrigger) && !string.IsNullOrWhiteSpace(TbReplace))
			{
				items.Add(new Item(TbTrigger, TbReplace));
				SaveEspansoFile();

				TbTrigger = string.Empty;
				TbReplace = string.Empty;
			}
		}

		private void fileSelect_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "espanso");
			fileDialog.Filter = "YAML Files (*.yml)|*.yml";

			if (fileDialog.ShowDialog() == true)
			{
				string selectedPath = fileDialog.FileName;
				string oldPath = path;
				path = selectedPath;

				if (LoadEspansoFile())
				{
					File.WriteAllText(pathPath, path);
					fileTextBlock.Text = path;
				}
				else
				{
					path = oldPath;
					fileTextBlock.Text = "BAD FILE";
				}
			}
		}
	}

	public class EspansoConfig
	{
		public List<Item> Matches { get; set; }
	}

	public class Item
	{
		public string Trigger { get; set; }
		public string Replace { get; set; }

		public Item() { }

		public Item(string trigger, string replace)
		{
			Trigger = trigger;
			Replace = replace;
		}
	}
}
