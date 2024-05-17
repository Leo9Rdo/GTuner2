using System;

namespace SoundAnalysis
{

	// БПФ Кули — Туки
	public static class FftAlgorithm
	{
		//х - входные данные
		public static double[] Calculate(double[] x)
		{
			int length;
			int bitsInLength;
			if (IsPowerOfTwo(x.Length))
			{
				length = x.Length;
				bitsInLength = Log2(length) - 1;
			}
			else
			{
				bitsInLength = Log2(x.Length);
				length = 1 << bitsInLength;
				//объекты будут дополнены нулями
			}

			//реверсирвоание
			ComplexNumber[] data = new ComplexNumber[length];
			for (int i = 0; i < x.Length; i++)
			{
				int j = ReverseBits(i, bitsInLength);
				data[j] = new ComplexNumber(x[i]);
			}

			// Кули-Туки 
			for (int i = 0; i < bitsInLength; i++)
			{
				int m = 1 << i;
				int n = m * 2;
				double alpha = -(2 * Math.PI / n);

				for (int k = 0; k < m; k++)
				{
					// e^(-2*pi/N*k)
					ComplexNumber oddPartMultiplier = new ComplexNumber(0, alpha * k).PoweredE();

					for (int j = k; j < length; j += n)
					{
						ComplexNumber evenPart = data[j];
						ComplexNumber oddPart = oddPartMultiplier * data[j + m];
						data[j] = evenPart + oddPart;
						data[j + m] = evenPart - oddPart;
					}
				}
			}

			//вычисление спектрограмы
			double[] spectrogram = new double[length];
			for (int i = 0; i < spectrogram.Length; i++)
			{
				spectrogram[i] = data[i].AbsPower2();
			}
			return spectrogram;
		}

		//получает количество значащих байт
		//количество битов для хранения числа
		private static int Log2(int n)
		{
			int i = 0;
			while (n > 0)
			{
				++i; n >>= 1;
			}
			return i;
		}


		//Меняет биты в числах
		//n - число
		//bitsCount кол-во значащих битов в числе
		private static int ReverseBits(int n, int bitsCount)
		{
			int reversed = 0;
			for (int i = 0; i < bitsCount; i++)
			{
				int nextBit = n & 1;
				n >>= 1;

				reversed <<= 1;
				reversed |= nextBit;
			}
			return reversed;
		}

		//проверяет является ли число степенью двойки
		//true если n=2^k и k - положительное целое
		private static bool IsPowerOfTwo(int n)
		{
			return n > 1 && (n & (n - 1)) == 0;
		}
	}

}