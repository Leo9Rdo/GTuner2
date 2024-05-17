using System;
using System.Collections.Generic;
using NAudio.Wave;
using SoundCapture;
using SoundAnalysis;
using Tuner;

namespace Tuner
{
	public class SoundFrequencyInfoSource : FrequencyInfoSource
	{

		
		SoundCaptureDevice device;
		Adapter adapter;

		public SoundFrequencyInfoSource(SoundCaptureDevice device)
		{
			this.device = device;
		}

		public object LastDetectedFrequency { get; set; }
		public bool? IsListening { get; set; }


		public double DetectFrequency()
		{
			// Здесь должна быть реальная логика определения частоты, но пока вернем просто 0
			return 0;
		}



		public override void Listen()
		{
			adapter = new Adapter(this, device);
			adapter.Start();
		}

		public override void Stop()
		{
			adapter.Stop();
		}

		class Adapter : SoundCaptureBase
		{
			SoundFrequencyInfoSource owner;

			const double MinFreq = 60;
			const double MaxFreq = 1300;

			public Adapter(SoundFrequencyInfoSource owner, SoundCaptureDevice device)
				: base(device)
			{
				this.owner = owner;
			}

			protected override void ProcessData(short[] data)
			{
				double[] x = new double[data.Length];
				for (int i = 0; i < x.Length; i++)
				{
					x[i] = data[i] / 32768.0;
					// Разделить на 32768 чтобы получить частоту 1 Гц (период 1 сек.)
				}

				double freq = FrequencyUtils.FindFundamentalFrequency(x, SampleRate, MinFreq, MaxFreq);
				owner.OnFrequencyDetected(new FrequencyDetectedEventArgs(freq));
			}

			
		}
	}
}
