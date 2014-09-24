

namespace FlightSimulator
{
	enum AirStatus
	{
		U = 0
	   ,
		F = 2
			,
		M = 4
			, A = 8

	}

	enum TestingType
	{
		TemparatureAdaptive1,
		TemparatureAdaptive2,
		TemparatureAdaptive3,
		TemparatureAdaptive4,

		TemparatureExponential,
		TemparatureLogarithmic,
		TemparatureLinear,
		TemparatureGeometric,

		IterationAdaptive1,
		IterationAdaptive2,
		IterationAdaptive3,
		IterationAdaptive4,

		IterationExponential,
		IterationLogarithmic,
		IterationLinear,
		IterationGeometric
	}

}
