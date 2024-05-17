using System;
using System.IO;
using System.Linq;
using System.Windows;
using SoundCapture;

namespace Tuner
{
	public partial class SelectDeviceWindow : Window
	{
		private SoundCaptureDevice[] devices;

		public SoundCaptureDevice SelectedDevice
		{
			get { return devices[deviceNamesListBox.SelectedIndex]; }
		}

		public SelectDeviceWindow()
		{
			InitializeComponent();
			LoadDevices();

		}

		//Бинд под Rocksmith cabel
		public void LoadDevices()
		{
			devices = SoundCaptureDevice.GetDevices();

			//проверка на наличие устройства
			bool isRocksmithDeviceAvailable = devices.Any(device => device.Name.Contains("Rocksmith USB Guitar Adapter"));
			if (!isRocksmithDeviceAvailable)
			{
				MessageBox.Show("Подключите Rocksmith USB Guitar Adapter и нажмите 'ОК' для продолжения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
				//DialogResult = false;
				//Close();
				//return;
			}
		
			foreach (var device in devices)
			{
				// Проверку на тип устройства
				if (device.Name.Contains("Rocksmith USB Guitar Adapter"))
				{
					deviceNamesListBox.Items.Add(device.Name);
				}
			}

			if (deviceNamesListBox.Items.Count > 0)
			{
				deviceNamesListBox.SelectedIndex = 0;
			}
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}
