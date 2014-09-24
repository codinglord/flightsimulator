using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlightSimulator
{
	class CalculationOutput
	{

		public List<CalucationData> TemparatureAdaptive1 { get; set; }
		public List<CalucationData> TemparatureAdaptive2 { get; set; }
		public List<CalucationData> TemparatureAdaptive3 { get; set; }
		public List<CalucationData> TemparatureAdaptive4 { get; set; }

		public List<CalucationData> TemparatureExponential { get; set; }
		public List<CalucationData> TemparatureLogarithmic { get; set; }
		public List<CalucationData> TemparatureLinear { get; set; }
		public List<CalucationData> TemparatureGeometric { get; set; }

		public List<CalucationData> IterationAdaptive1 { get; set; }
		public List<CalucationData> IterationAdaptive2 { get; set; }
		public List<CalucationData> IterationAdaptive3 { get; set; }
		public List<CalucationData> IterationAdaptive4 { get; set; }

		public List<CalucationData> IterationExponential { get; set; }
		public List<CalucationData> IterationLogarithmic { get; set; }
		public List<CalucationData> IterationLinear { get; set; }
		public List<CalucationData> IterationGeometric { get; set; }

	}
}
