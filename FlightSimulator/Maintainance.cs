using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FlightSimulator
{

	[Serializable()]
	class Maintainance
	{
		public DateTime MaintainanceStartedTime { get; set; }
		public DateTime ManntainanceEndedTime { get; set; }

		public string DisplayMaintainanceStartedTime { 
			get 
			{
				return this.MaintainanceStartedTime.ToString(Program.FORMAT_DATE);
			} 
		}

		public string DisplayMaintainanceEndedTime
		{
			get
			{
				return this.ManntainanceEndedTime.ToString(Program.FORMAT_DATE);
			}
		}

		public bool IsActive
		{
			get
			{
				return
					Program.CurrentTick >= MaintainanceStartedTime
					&&
					Program.CurrentTick <= ManntainanceEndedTime;
			}
		}
		public Air[] Slot = new Air[Program.MAX_AIR];

		public Maintainance(DateTime maintainanceStartedTime, DateTime maintainanceEndedTime)
		{
			this.MaintainanceStartedTime = maintainanceStartedTime;
			this.ManntainanceEndedTime = maintainanceEndedTime;
		}

		public List<RankData> Ranks { get; set; }

		public Maintainance AddAir(Air air)
		{

			Maintainance nearestSchedule = Matrix.NearestSchedule(air);
			if (nearestSchedule == null)
			{
				return null;
			}
			else
			{
				//Try first nearest schedule.
				this.AllocateSlot(air, nearestSchedule);
			}

			return nearestSchedule;
		}

		private void AllocateSlot(Air air,Maintainance nearestSchedule)
		{
			int airInSchedule = nearestSchedule.Slot.Where(f => f != null).Count();
			int nextIndex = Matrix.MaintainanceCollection.LastIndexOf(nearestSchedule);
			Trace.WriteLine("airInSchedule " + airInSchedule);
			if (airInSchedule < 5)
			{

				while (true)
				{
					int random = new Random().Next(nearestSchedule.Slot.Length - 1);
					//Trace.WriteLine("Random " + random);
					if (nearestSchedule.Slot[random] == null)
					{
						nearestSchedule.Slot[random] = air;
						break;
					}
				}
				Trace.WriteLine("Maintainance Start Time : " + nearestSchedule.MaintainanceStartedTime);
				Trace.WriteLine("Slot Capacity : " + nearestSchedule.Slot.Where(f => f != null).Count());
				
			}
			else if (nextIndex + 1 != Matrix.MaintainanceCollection.Count)
			{
				AllocateSlot(air, Matrix.MaintainanceCollection[nextIndex + 1]);
				Trace.WriteLine("Maintainance Start Time : " + Matrix.MaintainanceCollection[nextIndex + 1].MaintainanceStartedTime);
				Trace.WriteLine("Slot Capacity : " + Matrix.MaintainanceCollection[nextIndex + 1].Slot.Where(f => f != null).Count());
			}
			
		}


	


	}

}
