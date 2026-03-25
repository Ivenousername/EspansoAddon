using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Input;

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
			DeleteSelection();
		}

		private void DeleteSelection()
		{
			if (TheList.SelectedItem is Item selectedItem)
			{
				int i = TheList.SelectedIndex;
				items.Remove(selectedItem);
				SaveEspansoFile();
				TheList.SelectedIndex = i;
			}
		}


		private void Add_Click(object sender, RoutedEventArgs e)
		{
			TryAdding();
		}

		private void TryAdding()
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
		private void DownKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Down && Keyboard.Modifiers == ModifierKeys.Control)
			{
				MoveItemDown();
			}
		}

		private void UpKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((e.Key == System.Windows.Input.Key.Up && Keyboard.Modifiers == ModifierKeys.Control))
			{
				MoveItemUp();
			}
			
		}
		private void Down_Click(object sender, RoutedEventArgs e)
		{
			MoveItemDown();
		}


		private void Up_Click(object sender, RoutedEventArgs e)
		{
			MoveItemUp();
		}

		private void MoveItemDown()
		{
			if (TheList.SelectedIndex < items.Count - 1 && items.Count>=2 && TheList.SelectedItem != null)
			{

				var originalSelection = TheList.SelectedIndex;
				Item temp = items[originalSelection];
				items[originalSelection] = items[originalSelection+1];
				items[originalSelection+1] = temp;
				SaveEspansoFile();
				TheList.SelectedIndex = originalSelection+1;
			}
		}
		private void MoveItemUp()  //This crashed for some fucking reason but it's fine now
		{
			if (TheList.SelectedIndex > 0 && items.Count>=2 && TheList.SelectedItem != null)
			{
				var originalSelection = TheList.SelectedIndex;
				Item temp = items[originalSelection];
				items[originalSelection] = items[originalSelection-1];
				items[originalSelection-1] = temp;
				SaveEspansoFile();
				TheList.SelectedIndex = originalSelection-1;
			}
		}
		private void Shortkeys_Global(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((e.Key == System.Windows.Input.Key.W && Keyboard.Modifiers == ModifierKeys.Control))
			{
				Application.Current.Shutdown();
			}
			else if (e.Key == System.Windows.Input.Key.Delete && TheList.SelectedItem != null )
			{
				DeleteSelection();
			}

		}

		private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			TheList.UnselectAll(); //Apparently doesn't work everywhere?

		}

		private void tbReplace_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key==System.Windows.Input.Key.Enter)
			{
				TryAdding();
				tbTrigger.Focus();
			}
		}

		private void tbTrigger_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key==System.Windows.Input.Key.Enter)
			{
				tbReplace.Focus();
			}
		}
		private void DelKeydown(object sender, KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Delete)
			{
				DeleteSelection();
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
