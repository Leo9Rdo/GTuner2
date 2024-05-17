using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;

namespace SoundCapture
{
	// Записывающее устройство.
	public class SoundCaptureDevice : IDisposable
	{
		MMDevice device;


		public bool IsDefault // Устройство по умолчанию
		{
			get { return device == null; }
		}

		// Идентификатор устройства.
		public string Id
		{
			get { return device?.ID; }
		}

		// Название устройства.
		public string Name
		{
			get { return device?.FriendlyName; }
		}

		internal SoundCaptureDevice(MMDevice device)
		{
			this.device = device;
		}

		public static SoundCaptureDevice[] GetDevices()
		{
			MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
			List<SoundCaptureDevice> devices = new List<SoundCaptureDevice>();
			foreach (var dev in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
			{
				devices.Add(new SoundCaptureDevice(dev));
			}
			return devices.ToArray();

		}

		public static SoundCaptureDevice GetDefaultDevice()
		{
			MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
			var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
			if (defaultDevice == null)
				throw new SoundCaptureException("Устройство захвата по умолчанию не найдено");
			return new SoundCaptureDevice(defaultDevice);
		}

		public void Dispose()
		{
			// Освобождение ресурсов, если необходимо.
		}


		public interface ISoundCaptureDeviceService
		{
			SoundCaptureDevice[] GetDevices();
		}

		public class SoundCaptureDeviceService : ISoundCaptureDeviceService
		{
			public SoundCaptureDevice[] GetDevices()
			{
				return SoundCaptureDevice.GetDevices();
			}
		}


	}
}
