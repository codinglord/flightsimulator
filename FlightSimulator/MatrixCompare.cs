using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlightSimulator
{
	class MatrixCompare
	{
		public int? Before;
		public int? After;
		public string Position { get; set; }
		public bool Changed
		{
			get
			{
				if (!Before.HasValue)
				{
					return false;
				}
				if (!After.HasValue)
				{
					return false;
				}
				return Before.Value != After.Value;
			}
		}
	}

}
