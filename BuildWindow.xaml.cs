using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using SoundCapture;
using SoundAnalysis;
using System.Diagnostics.Eventing.Reader;


namespace Tuner
{
	/// <summary>
	/// Логика взаимодействия для BuildWindow.xaml
	/// </summary>
	public partial class BuildWindow : Window
	{
		public BuildWindow()
		{
			InitializeComponent();

			ComboBoxBuild.SelectionChanged += ComboBox_SelectionChanged;
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem selectedItem = (ComboBoxItem)ComboBoxBuild.SelectedItem;
			if (selectedItem != null)
			{
				// Получение содержимого и тега выбранного элемента
				string content = selectedItem.Content.ToString();
				string tag = selectedItem.Tag.ToString();

				// Перенос строки внутри тега
				tag = tag.Replace("\\n", Environment.NewLine);

				// Обновление текста в TextBox
				BuildText.Text = tag;
			}
		}
	}
}
