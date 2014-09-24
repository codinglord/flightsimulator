using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlightSimulator
{
	class CalucationData
	{

		public int SeqNo;

		public double T { get; set; }
		public double TT { get; set; }
		public int? Iteration { get; set; }
		public double? SigmaT
		{
			get
			{



				return Math.Sqrt(Math.Pow(this.T,2) + this.TT);
			}
		}
		public double? DeltaT { get; set; }

		public double? TZero { get; set; }
		public double? AlphaExpoential { get; set; }
		public double? K { get; set; }
		public double? Gramma { get; set; }

		public double? CurrentT { get; set; }
		public double? PreviouseT { get; set; }


		public int TotalWatingTime
		{
			get
			{
				int totalWatingTime = 0;
				foreach (var i in this.Schedules)
				{

					for (int j = 0; j < i.Slot.Length; j++)
					{
						if (i.Slot[j] != null)
						{
							totalWatingTime += i.Slot[j].WaitingDurationLog;
						}
					}

				}
				return totalWatingTime;
			}
		}


		public List<Maintainance> Schedules { get; set; }

		#region Exponentials

		public double? Exponential { get; set; }
		public double? Logarithmic { get; set; }
		public double? Linear { get; set; }
		public double? Geometric { get; set; }

		public double ProbExponential
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Exponential.Value);
			}
		}

		public double ProbLogarithmic
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Logarithmic.Value);
			}
		}

		public double ProbLinear
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Linear.Value);
			}
		}

		public double ProbGeometric
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Geometric.Value);
			}
		}


		#endregion Exponentials

		#region Temparatures

		public double? Adaptive1 { get; set; }
		public double? Adaptive2 { get; set; }
		public double? Adaptive3 { get; set; }
		public double? Adaptive4 { get; set; }

		public double ProbAdaptive1
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Adaptive1.Value);
			}
		}

		public double ProbAdaptive2
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Adaptive2.Value);
			}
		}

		public double ProbAdaptive3
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Adaptive3.Value);
			}
		}

		public double ProbAdaptive4
		{
			get
			{
				return Math.Pow(Math.E, (-1 * K.Value * DeltaT.Value) / this.Adaptive4.Value);
			}
		}

		#endregion Temparatures

		public CalucationData(int seqNo, double tSumAll, double tEachSumPowEach, int iteration, List<Maintainance> schedules)
		{
			this.SeqNo = seqNo;
			this.T = tSumAll;
			this.TT = tEachSumPowEach;
			this.Iteration = iteration;
			this.K = ComputationUnit.GetK(SigmaT.Value);
			this.TZero = Program.UserTZero;
			this.AlphaExpoential = Program.UserAlphaExponential;
			this.Gramma = Program.UserGramma;
			
			this.Schedules = schedules;
		}

		public void StartCalculation(double previouTSumAll)
		{
			this.PreviouseT = previouTSumAll;
			this.DeltaT = this.PreviouseT.Value - T;

			//For exponentials
			this.Exponential = ComputationUnit.Exponential(TZero.Value, AlphaExpoential.Value, K.Value);
			this.Logarithmic = ComputationUnit.Logarithmic(TZero.Value, Iteration.Value);
			this.Linear = ComputationUnit.Linear(TZero.Value, AlphaExpoential.Value, Iteration.Value);
			this.Geometric = ComputationUnit.Geometric(AlphaExpoential.Value, Iteration.Value, TZero.Value);

			//For temparatures
			this.Adaptive1 = ComputationUnit.Adaptive1(this.PreviouseT.Value, SigmaT.Value);
			this.Adaptive2 = ComputationUnit.Adaptive2(this.PreviouseT.Value);
			this.Adaptive3 = ComputationUnit.Adaptive3(this.PreviouseT.Value, SigmaT.Value, this.Gramma.Value);
			this.Adaptive4 = ComputationUnit.Adaptive4(this.PreviouseT.Value, 0, this.SigmaT.Value);

		}


	}
}
