using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlightSimulator
{
	static class ComputationUnit
	{

		public static double GetK(double sigma)
		{
			return Math.Abs((Program.UserTZero * Math.Log(0.8)) / (sigma));
		}

		public static double GetSigma(double totalWatingTime)
		{
			return totalWatingTime / 18;
		}

		public static double Exponential(double t0, double alpha, double k)
		{
			return t0 * Math.Pow(Math.E, -1 * alpha * k * (1 / 18));
		}

		public static double Logarithmic(double t0, double t)
		{
			return t0 / Math.Log(t);
		}

		public static double Linear(double t0, double alpha, double t)
		{
			return t0 - alpha * t;
		}

		public static double Geometric(double alpha, double t, double t0)
		{
			return Math.Pow(alpha, t) * t0;
		}

		public static double Adaptive1(double tt, double sigmaT)
		{
			return tt * (1 - (tt * (Math.Log(2) / (3 * sigmaT))));
		}

		public static double Adaptive2(double tt)
		{
			return tt * (1 - tt * (Math.Log(2) / 14));
		}

		public static double Adaptive3(double tt, double sigmaT, double grama)
		{
			return tt * (1 - tt * ((grama * tt) / (sigmaT)));
		}

		public static double Adaptive4(double tt, double deltaT, double sigmaT)
		{
			return tt * (1 - tt * (deltaT / Math.Pow(sigmaT, 2)));
		}

		public static CalucationData GetRawCalculationData(int iteration, List<Maintainance> maintainances)
		{
			int countCal = Program.CalculationSeq++;
			CalucationData calData = null;
			double t = 0;
			double tt = 0;
		


			for (int i = 0; i < maintainances.Count; i++)
			{
				Maintainance iM = maintainances[i];
				for (int j = 0; j < Program.MAX_AIR; j++)
				{
					if (iM.Slot[j] != null)
					{
						++t;
					}
				}
			}


			for (int i = 0; i < Program.MAX_AIR; i++)
			{
				double iSlot = 0;
				for (int j = 0; j < maintainances.Count; j++)
				{
					if (maintainances[j].Slot[i] != null)
					{
						++iSlot;
					}
				}
				tt += Math.Pow(iSlot,2);
			}


			calData = new CalucationData(
				countCal,
				t,
				tt,
				iteration,
				maintainances
			);

			return calData;
		}

	}
}
