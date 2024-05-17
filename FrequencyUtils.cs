using System;
using System.Linq;
using NAudio.Wave;
using SoundAnalysis;

namespace Tuner
{
	// Находим фундаментальную частоту.
	static class FrequencyUtils
	{
		// Находим пики, анализируем
		// x - звуковые сэмплы
		// sampleRate - частота семплирования
		// minFreq - минимальная полезная частота
		// maxFreq - максимальная полезная частота
		// returns - Найденная частота, 0 - в противном случае

		internal static double FindFundamentalFrequency(double[] x, int sampleRate, double minFreq, double maxFreq)
		{
			double[] spectr = FftAlgorithm.Calculate(x);

			int usefullMinSpectr = Math.Max(0, (int)(minFreq * spectr.Length / sampleRate));
			int usefullMaxSpectr = Math.Min(spectr.Length, (int)(maxFreq * spectr.Length / sampleRate) + 1);

			// находим пики частот по БПФ 
			const int PeaksCount = 5;
			int[] peakIndices;
			peakIndices = FindPeaks(spectr, usefullMinSpectr, usefullMaxSpectr - usefullMinSpectr, PeaksCount);

			if (Array.IndexOf(peakIndices, usefullMinSpectr) >= 0)
			{
				// если звука не будет - вернет 0
				return 0;
			}

			// выбирает фрагмент для регистрации пиковых значений: смещение данных
			const int verifyFragmentOffset = 0;
			// и половина длины данных
			int verifyFragmentLength = (int)(sampleRate / minFreq);

			// перебираем все пики для поиска одного с наименьшим отличием от других
			double minPeakValue = double.PositiveInfinity;
			int minPeakIndex = 0;
			int minOptimalInterval = 0;
			for (int i = 0; i < peakIndices.Length; i++)
			{
				int index = peakIndices[i];
				int binIntervalStart = spectr.Length / (index + 1), binIntervalEnd = spectr.Length / index;
				int interval;
				double peakValue;
				// сканирование частот/интервалов
				ScanSignalIntervals(x, verifyFragmentOffset, verifyFragmentLength, binIntervalStart, binIntervalEnd, out interval, out peakValue);

				if (peakValue < minPeakValue)
				{
					minPeakValue = peakValue;
					minPeakIndex = index;
					minOptimalInterval = interval;
				}
			}

			return (double)sampleRate / minOptimalInterval;
		}

		private static void ScanSignalIntervals(double[] x, int index, int length, int intervalMin, int intervalMax, out int optimalInterval, out double optimalValue)
		{
			optimalValue = double.PositiveInfinity;
			optimalInterval = 0;

			// интервал между самым маленьким и большим значением может быть большим
			// ограничиваем его фиксированным значением
			const int MaxAmountOfSteps = 30;
			int steps = intervalMax - intervalMin;
			if (steps > MaxAmountOfSteps)
				steps = MaxAmountOfSteps;
			else if (steps <= 0)
				steps = 1;

			// ищем в интервале волну с наименьшим различием длины
			for (int i = 0; i < steps; i++)
			{
				int interval = intervalMin + (intervalMax - intervalMin) * i / steps;

				double sum = 0;
				for (int j = 0; j < length; j++)
				{
					double diff = x[index + j] - x[index + j + interval];
					sum += diff * diff;
				}
				if (optimalValue > sum)
				{
					optimalValue = sum;
					optimalInterval = interval;
				}
			}
		}

		private static int[] FindPeaks(double[] values, int index, int length, int peaksCount)
		{
			double[] peakValues = new double[peaksCount];
			int[] peakIndices = new int[peaksCount];

			for (int i = 0; i < peaksCount; i++)
			{
				peakValues[i] = values[peakIndices[i] = i + index];
			}

			// находим минимальное значение
			double minStoredPeak = peakValues[0];
			int minIndex = 0;
			for (int i = 1; i < peaksCount; i++)
			{
				if (minStoredPeak > peakValues[i]) minStoredPeak = peakValues[minIndex = i];
			}

			for (int i = peaksCount; i < length; i++)
			{
				if (minStoredPeak < values[i + index])
				{
					// заменяем минимальное значение бОльшим
					peakValues[minIndex] = values[peakIndices[minIndex] = i + index];

					// и ищем минимальное значение снова
					minStoredPeak = peakValues[minIndex = 0];
					for (int j = 1; j < peaksCount; j++)
					{
						if (minStoredPeak > peakValues[j]) minStoredPeak = peakValues[minIndex = j];
					}
				}
			}

			return peakIndices;
		}
	}
}
