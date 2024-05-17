using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using SoundCapture;
using SoundAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Controls;
using System.Linq;

namespace Tuner
{
	public partial class MainWindow : Window
	{
		private bool isListening = false;
		private FrequencyInfoSource frequencyInfoSource;
		private TextBox activeNoteNameTextBox; // Текущий активный текстбокс для вывода нот

		

		public double FrequencyValueNoteName
		{
				get { return (double)GetValue(FrequencyProperty); }
				set { SetValue(FrequencyProperty, value); }
		}

			public static readonly DependencyProperty FrequencyProperty =
				DependencyProperty.Register("FrequencyValue", typeof(double), typeof(MainWindow));

			public double ClosestFrequencyValue
			{
				get { return (double)GetValue(ClosestFrequencyProperty); }
				set { SetValue(ClosestFrequencyProperty, value); }
			}

			public static readonly DependencyProperty ClosestFrequencyProperty =
				DependencyProperty.Register("ClosestFrequencyValue", typeof(double), typeof(MainWindow));

			public string NoteNameValue
			{
				get { return (string)GetValue(NoteNameProperty); }
				set { SetValue(NoteNameProperty, value); }
			}

			public static readonly DependencyProperty NoteNameProperty =
				DependencyProperty.Register("NoteNameValue", typeof(string), typeof(MainWindow));
		


		public MainWindow()
		{
			InitializeComponent();
			UpdateListenStopButtons();
			Closed += MainWindow_Closed; 

			NoteNameTextBox1.PreviewMouseDoubleClick += NoteNameTextBox_PreviewMouseDoubleClick;
			NoteNameTextBox2.PreviewMouseDoubleClick += NoteNameTextBox_PreviewMouseDoubleClick;
			NoteNameTextBox3.PreviewMouseDoubleClick += NoteNameTextBox_PreviewMouseDoubleClick;
			NoteNameTextBox4.PreviewMouseDoubleClick += NoteNameTextBox_PreviewMouseDoubleClick;
			NoteNameTextBox5.PreviewMouseDoubleClick += NoteNameTextBox_PreviewMouseDoubleClick;
			NoteNameTextBox6.PreviewMouseDoubleClick += NoteNameTextBox_PreviewMouseDoubleClick;

		}

		private bool IsRocksmithDeviceConnected()
		{
			return SoundCaptureDevice.GetDevices().Any(device => device.Name.Contains("Rocksmith USB Guitar Adapter"));
		}



		private void MainWindow_Closed(object sender, EventArgs e)
		{

			var openWindows = Application.Current.Windows.Cast<Window>().ToList();
			foreach (var window in openWindows)
			{
				if (isListening)
				{
					StopListening();
				}

				if (window != this)
				{
					window.Close();
				}
				
			}

			
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			
			Close();
		}

		private void StopListening()
		{
			isListening = false;
			frequencyInfoSource.Stop();
			frequencyInfoSource.FrequencyDetected -= FrequencyInfoSource_FrequencyDetected;
			frequencyInfoSource = null;
		}

	
		private void StartListening(SoundCaptureDevice device)
		{
			isListening = true;
			frequencyInfoSource = new SoundFrequencyInfoSource(device);
			frequencyInfoSource.FrequencyDetected += FrequencyInfoSource_FrequencyDetected;
			frequencyInfoSource.Listen();
		}

		private void FrequencyInfoSource_FrequencyDetected(object sender, FrequencyDetectedEventArgs e)
		{
			Dispatcher.Invoke(() => UpdateFrequencyDisplays(e.Frequency));
		}

		private void UpdateFrequencyDisplays(double frequency)
		{
			if (frequency > 0 && activeNoteNameTextBox != null)
			{
				FrequencyTextBox.IsEnabled = true;
				FrequencyTextBox.Text = frequency.ToString("f3");

				double closestFrequency;
				string noteName;
				FindClosestNote(frequency, out closestFrequency, out noteName);

				CloseFrequencyTextBox.IsEnabled = true;
				CloseFrequencyTextBox.Text = closestFrequency.ToString("f3");

				activeNoteNameTextBox.IsEnabled = true;
				activeNoteNameTextBox.Text = noteName;
			}
			else
			{
				FrequencyTextBox.IsEnabled = false;
				CloseFrequencyTextBox.IsEnabled = false;
				if (activeNoteNameTextBox != null)
				{
					activeNoteNameTextBox.IsEnabled = false;
				}
			}
		}

		private static string[] NoteNames = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
		private static double ToneStep = Math.Pow(2, 1.0 / 12);

		public void FindClosestNote(double frequency, out double closestFrequency, out string noteName)
		{
			const double AFrequency = 440.0;
			const int ToneIndexOffsetToPositives = 120;

			int toneIndex = (int)Math.Round(Math.Log(frequency / AFrequency, ToneStep));
			noteName = NoteNames[(ToneIndexOffsetToPositives + toneIndex) % NoteNames.Length];
			closestFrequency = Math.Pow(ToneStep, toneIndex) * AFrequency;
		}

		private void ListenButton_Click(object sender, RoutedEventArgs e)
		{
			if (!isListening && IsRocksmithDeviceConnected())
			{
				SelectDeviceWindow form = new SelectDeviceWindow();
				if (form.ShowDialog() == true)
				{
					SoundCaptureDevice device = form.SelectedDevice;
					if (device != null)
					{
						StartListening(device);
						UpdateListenStopButtons();
					}
				}
			}
			else if (!IsRocksmithDeviceConnected())
			{
				MessageBox.Show("Подключите Rocksmith USB Guitar Adapter и нажмите 'ОК' для продолжения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			else
			{
				StopListening();
				UpdateListenStopButtons();
			}
		}



		private void UpdateListenStopButtons()
		{
			ListenButton.IsEnabled = !isListening;
			StopButton.IsEnabled = isListening;
		}

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			StopListening();
			UpdateListenStopButtons();
		}

		

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (isListening)
			{
				StopListening();
			}
		}

		private void NoteNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

		}

		private void CloseFrequencyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

		}

		private void FrequencyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

		}

		

		private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

		}

		private void NoteNameTextBox2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

		}

		private void NoteNameTextBox_PreviewMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			activeNoteNameTextBox = (TextBox)sender;
		}

		private void NoteNameTextBox2_TextChanged_1(object sender, TextChangedEventArgs e)
		{

		}

		private void NoteNameTextBox3_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void NoteNameTextBox4_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void NoteNameTextBox5_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void NoteNameTextBox6_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void BuildButton_Click(object sender, RoutedEventArgs e)
		{
			BuildWindow buildWindow = new BuildWindow();
			buildWindow.Show();
		}

	}
}
