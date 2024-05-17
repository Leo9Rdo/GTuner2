using System;
using System.Collections.Generic;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Microsoft.Win32.SafeHandles;
using System.Linq;

namespace SoundCapture
{
	// Базовый класс для захвата аудиосэмплов.
	public abstract class SoundCaptureBase : IDisposable
	{
		const int BufferSeconds = 3;
		const int NotifyPointsInSecond = 2;
		const int BitsPerSample = 16;
		const int ChannelCount = 1;

		int sampleRate = 44100;
		bool isCapturing = false;
		bool disposed = false;

		public bool IsCapturing
		{
			get { return isCapturing; }
		}

		public int SampleRate
		{
			get { return sampleRate; }
			set
			{
				if (sampleRate <= 0) throw new ArgumentOutOfRangeException();

				EnsureIdle();

				sampleRate = value;
			}
		}

		WaveInEvent waveIn;
		int bufferLength;
		AutoResetEvent positionEvent;
		SafeWaitHandle positionEventHandle;
		ManualResetEvent terminated;
		Thread thread;
		SoundCaptureDevice device;

		public SoundCaptureBase()
			: this(SoundCaptureDevice.GetDefaultDevice())
		{
		}

		public SoundCaptureBase(SoundCaptureDevice device)
		{
			this.device = device;

			positionEvent = new AutoResetEvent(false);
			positionEventHandle = positionEvent.SafeWaitHandle;
			terminated = new ManualResetEvent(true);
		}

		private void EnsureIdle()
		{
			if (IsCapturing)
				throw new SoundCaptureException("Захват в процессе");
		}

		public void Start()
		{
			EnsureIdle();

			isCapturing = true;

			waveIn = new WaveInEvent();

			var devices = SoundCaptureDevice.GetDevices();
			int deviceIndex = -1;

			for (int i = 0; i < devices.Length; i++)
			{
				if (devices[i].Id == device.Id)
				{
					deviceIndex = i;
					break;
				}
			}

			if (deviceIndex == -1)
				throw new InvalidOperationException("Устройство не найдено");

			waveIn.DeviceNumber = deviceIndex;

			if (deviceIndex == -1)
				throw new InvalidOperationException("Устройство не найдено");

			waveIn.DeviceNumber = deviceIndex;

			waveIn.WaveFormat = new WaveFormat(sampleRate, BitsPerSample, ChannelCount);

			bufferLength = waveIn.WaveFormat.AverageBytesPerSecond * BufferSeconds;

			waveIn.BufferMilliseconds = (int)((float)bufferLength / waveIn.WaveFormat.AverageBytesPerSecond * 1000);
			waveIn.DataAvailable += WaveIn_DataAvailable;

			terminated.Reset();
			thread = new Thread(new ThreadStart(ThreadLoop));
			thread.Name = "Sound capture";
			thread.Start();

			waveIn.StartRecording();
		}

		private void ThreadLoop()
		{
			WaitHandle[] handles = new WaitHandle[] { terminated, positionEvent };
			while (WaitHandle.WaitAny(handles) > 0)
			{
				// Handle buffer data or other processing logic here
			}
		}

		private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
		{
			short[] data = new short[e.Buffer.Length / 2];
			Buffer.BlockCopy(e.Buffer, 0, data, 0, e.Buffer.Length);

			ProcessData(data);

			if (!terminated.WaitOne(0))
			{
				positionEvent.Set();
			}
		}

		protected abstract void ProcessData(short[] data);

		public void Stop()
		{
			if (isCapturing)
			{
				isCapturing = false;

				terminated.Set();
				thread.Join();

				waveIn.StopRecording();
				waveIn.Dispose();
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		~SoundCaptureBase()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (disposed) return;

			disposed = true;
			GC.SuppressFinalize(this);
			if (IsCapturing) Stop();
			positionEventHandle.Dispose();
			positionEvent.Close();
			terminated.Close();
		}
	}
}
