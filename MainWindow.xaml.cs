using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.ComponentModel;

namespace EspansoAddon
{
	public partial class MainWindow : Window
	{
		public ObservableCollection<Item> items { get; set; } = new ObservableCollection<Item>();
		private string path = @"C:\Users\anond\AppData\Roaming\espanso\match\packages\myshit\myshit.yml";
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
			TheList.ItemsSource = items;
			LoadEspansoFile();
		}
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		private void SaveEspansoFile()
		{
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

		private void LoadEspansoFile()
		{
			try
			{
				if (!File.Exists(path)) return;

				string yamlContent = File.ReadAllText(path);
				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.IgnoreUnmatchedProperties()
					.Build();

				var config = deserializer.Deserialize<EspansoConfig>(yamlContent);

				if (config?.Matches != null)
				{
					items.Clear();
					foreach (var match in config.Matches)
					{
						if (!string.IsNullOrEmpty(match.Trigger))
						{
							items.Add(match);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
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
			items.Add(new Item(TbTrigger, TbReplace));
			SaveEspansoFile();
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
