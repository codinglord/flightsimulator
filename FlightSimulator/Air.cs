using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FlightSimulator
{
	[Serializable()]
	class Air
	{


		public int No { get; set; }
		public DateTime DepartureTime { get; set; }

		public AirStatus Status
		{
			get;
			private set;
		}

		public DateTime ArrivalTime
		{
			get;
			private set;
		}
		public string DisplayArrivalTime {
			get {
				return this.ArrivalTime.ToString(Program.FORMAT_DATE);
			}
		}

		public string DisplayDepartureTime
		{
			get
			{
				return this.DepartureTime.ToString(Program.FORMAT_DATE);
			}
		}

		public int WaitingDurationLog { get; private set; }
		public int WaitingDuration {
			get;
			private set;
		}

		public DateTime? Schedule { get; set; }

		public int AccumulateTimeLog { get; private set; }
		public int AccumulateTime;

		public Air(int no, DateTime departureTime,DateTime arrivalTime)
		{
			this.No = no;
			this.DepartureTime = departureTime;
			this.ArrivalTime = arrivalTime;
			this.AccumulateTime = 0;
			this.WaitingDuration = 0;
		}

		public void UpdateStatus()
		{

			if (this.DepartureTime >= Program.CurrentTime && this.Status != AirStatus.A)
			{

				int reachedTime = Convert.ToInt32((this.ArrivalTime - this.DepartureTime).TotalMinutes);
				if (reachedTime == this.AccumulateTime)
				{
					this.AccumulateTimeLog = this.AccumulateTime;
					this.AccumulateTime = 0;
					this.Status = AirStatus.A;

					//Try to add air to maintaince
					Maintainance m = Matrix.AddAirToMaintain(this);
					if (m != null)
					{
						this.WaitingDuration = Convert.ToInt32((m.MaintainanceStartedTime - this.ArrivalTime).TotalMinutes);
						this.Schedule = m.MaintainanceStartedTime;
					}
					
				}
				else
				{
					this.AccumulateTime++;
				}
			}
			if (this.Status == AirStatus.A && this.Schedule != null)
			{
				this.Status = AirStatus.M;
				this.WaitingDurationLog = this.WaitingDuration;
				this.WaitingDuration = 0;
			}



		}

	}
}
