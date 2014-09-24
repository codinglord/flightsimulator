using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FlightSimulator
{
	static class Matrix
	{

		public static List<Maintainance> MaintainanceCollection = new List<Maintainance>();

		public static List<int> IndexedColumns { get; set; }


		public static Maintainance NearestSchedule(Air air)
		{
			
			foreach (var i in MaintainanceCollection.Where(f => f.MaintainanceStartedTime >= air.ArrivalTime))
			{
				return i;
			}
			return null;
			
		}

		public static Maintainance ActiveSchedule
		{
			get
			{
				return MaintainanceCollection.Find(f => f.IsActive);
			}
		}

		static Matrix()
		{
			Matrix.Init();
		}

		public static void Init()
		{
			DateTime startedTime = Program.GetTime("06:00");
			for (int i = 0; i < 2 * 7; i++)
			{
				MaintainanceCollection.Add(new Maintainance(
					startedTime.AddHours(i * 12),
					startedTime.AddHours(i * 12 + 12)
				));
			}

			IndexedColumns = new List<int>();
			for (int i = 0; i < Program.MAX_AIR; i++)
			{
				IndexedColumns.Add(i);
			}

		}

		public static void ClearMatrix()
		{
			MaintainanceCollection.Clear();
		}

		public static Maintainance AddAirToMaintain(Air air)
		{
			Maintainance m = NearestSchedule(air);
			Maintainance alcocatedSchdule = null;
			try
			{
				alcocatedSchdule = m.AddAir(air);
			}
			catch (Exception) 
			{/*An exception will be occured when it's run to latest maintaince.*/
				
			}
			return alcocatedSchdule;
		}

		public static List<Maintainance> Fixation(List<Maintainance> schedules)
		{
			//List<ColumnRank> cImproved = null;
			//while (cImproved == null || cImproved.Count > 0)
			//{
			//    cImproved = GetRankedColumns(schedules).Where(f => f.MustImprove).ToList();
			//    if (cImproved.Count != 0)
			//    {
			//        for (int i = 0; i < schedules.Count; i++)
			//        {
			//            List<ColumnRank> iColRank = GetRankedColumns(schedules);
			//            schedules[i] = SwapSlot(schedules[i], cImproved, iColRank);
			//        }
			//    }

			//}

			List<KeyValuePair<int, Air>> cLog = new List<KeyValuePair<int, Air>>();
			int count = 0;
			foreach (Maintainance i in schedules)
			{

				for (int j = 0; j < i.Slot.Length; j++)
				{
					if (IndexedColumns.Contains(j) && i.Slot[j] != null)
					{
						IndexedColumns.Remove(j);			
					}
					else
					{
						if (i.Slot[j] != null)
						{
							cLog.Add(new KeyValuePair<int, Air>(
								count,
								Program.Clone(i.Slot[j])
							));
							i.Slot[j] = null;
						}


						
					}
				}
				count++;
			}

			while (IndexedColumns.Count != 0 || cLog.Count != 0)
			{
				if (IndexedColumns.Count == 0)
				{
					break;
				}
				int ran = IndexedColumns[0];
				foreach (var i in cLog)
				{
					schedules[i.Key].Slot[ran] = i.Value;
					IndexedColumns.Remove(ran);
					break;
				}
				if (cLog.Count > 0)
				{
					cLog.RemoveAt(0);
				}
			}


			List<int> cRandom = new List<int>();
			for (int i = 0; i < Program.MAX_AIR; i++)
			{
				cRandom.Add(i);
			}
			cRandom = new List<int>(RandomizeInt(cRandom.ToArray()));


			foreach (var i in schedules)
			{

				if (cRandom.Count != 0)
				{
					for (int j = 0; j < i.Slot.Length; j++)
					{
						Air jAir = i.Slot[j];
						int jNewPosition = cRandom[0];
						Air jNewAirPosition = i.Slot[jNewPosition];
						if (jAir != null && j != jNewPosition && jNewAirPosition != null)
						{
							i.Slot[jNewPosition] = Program.Clone(jAir);
							i.Slot[j] = null;
							cRandom.Remove(jNewPosition);
							break;
						}
						else if (j == jNewPosition)
						{
							cRandom.Remove(j);
							break;
						}
					}
				}

			}
			



			return schedules;
		}

		static Random _random = new Random();

		public static int[] RandomizeInt(int[] arr)
		{
			List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>();
			// Add all strings from array
			// Add new random int each time
			foreach (int s in arr)
			{
				list.Add(new KeyValuePair<int, int>(_random.Next(), s));
			}
			// Sort the list by the random number
			var sorted = from item in list
						 orderby item.Key
						 select item;
			// Allocate new string array
			int[] result = new int[arr.Length];
			// Copy values to array
			int index = 0;
			foreach (KeyValuePair<int, int> pair in sorted)
			{
				result[index] = pair.Value;
				index++;
			}
			// Return copied array
			return result;
		}

		public static List<Maintainance> FixPoorRank(List<Maintainance> schedules,List<ColumnRank> ranks)
		{
			List<ColumnRank> cImproved = GetRankedColumns(schedules).Where(f => f.IsPoor.Value).ToList();

			for (int i = 0; i < schedules.Count; i++)
			{
				List<ColumnRank> iColRank = GetRankedColumns(schedules);
				bool iChanged = false;

				Maintainance iSwap = schedules[i];
				Maintainance iBefore = Program.Clone(iSwap);

				
				SwapPoorWatingSlot(
					ref iSwap, 
					cImproved.Where(f => f.IsPoor.Value == true).ToList(),
					ref iChanged
				);

				if (iChanged){}
			}

			#if true
			
			int countFixedPoor = cImproved.Count(f => f.IsPoor.Value);

			#endif

			Maintainance[] output = new Maintainance[schedules.Count];
			schedules.CopyTo(output);
			return output.ToList();
		}


		public static List<ColumnRank> GetRankedColumns(List<Maintainance> sourceSchedules)
		{
			List<Maintainance> schedules = sourceSchedules.ToList();
			List<EmptySlot> cFixation = new List<EmptySlot>();
			List<ColumnRank> cColumnRank = new List<ColumnRank>();

			//Ranking matrix
			for (int i = 0; i < schedules.Count; i++)
			{
				for (int j = 0; j < Program.MAX_AIR; j++)
				{
					cFixation.Add(new EmptySlot()
					{
						Row = i,
						Col = j,
						IsEmpty = schedules[i].Slot[j] == null
					});
				}
			}


			for (int j = 0; j < Program.MAX_AIR; j++)
			{
				ColumnRank jColumnRank = new ColumnRank() { ColumnIndex = j };
				for (int i = 0; i < schedules.Count; i++)
				{
					jColumnRank.TotalAir += schedules[i].Slot[j] == null
						? 0
						: 1;
					jColumnRank.TotalWating += schedules[i].Slot[j] == null
						? 0
						: schedules[i].Slot[j].WaitingDurationLog;
				}

				cColumnRank.Add(jColumnRank);
			}
			cColumnRank = (from r in cColumnRank
							   orderby r.TotalWating descending
							   select r).ToList();

			int count = 0;
			for (int i = 0; i < cColumnRank.Count; i++)
			{
				ColumnRank iColumnRank = cColumnRank[i];
				iColumnRank.IsPoor = count++ < Program.MAX_AIR / 2;
			}


			return cColumnRank.ToList();
		}



		private static Maintainance SwapSlot(Maintainance schedule, List<ColumnRank> improveColumns, List<ColumnRank> allRankedColumns)
		{

			foreach (var i in improveColumns)
			{
				List<ColumnRank> destCols = allRankedColumns.FindAll(f => {

					return !improveColumns.Exists(g => g.ColumnIndex == f.ColumnIndex);

				});



				if (destCols.Count > 0)
				{
					int selectedPoorIndex = new Random(Guid.NewGuid().GetHashCode()).Next(0, destCols.Count - 1);
					if (destCols.Count == 2)
					{
						selectedPoorIndex = Convert.ToInt32(Math.Abs(Guid.NewGuid().GetHashCode() % 2));
					}
					else if (destCols.Count == 1)
					{
						
					}
					ColumnRank swapCol = destCols.ElementAt(selectedPoorIndex);


					Air iDestAir = schedule.Slot[swapCol.ColumnIndex];
					if (iDestAir != null)
					{
						//Clone Air object from destination array then remove it.
						Air sourceAir = schedule.Slot[swapCol.ColumnIndex];
						schedule.Slot[swapCol.ColumnIndex] = null;

						//Assign cloning object to improve index.
						schedule.Slot[i.ColumnIndex] = sourceAir;
					}


				}
				else
				{
					Trace.WriteLine("Poor column were not found!");
				}
			}


			return schedule;
		}


		private static void SwapPoorWatingSlot(ref Maintainance schedule, List<ColumnRank> improveColumns,ref bool isChanged)
		{
			
			foreach (var i in improveColumns.Where(f => f.IsPoor.Value == true))//Take poor ranks
			{

				#if true

				int countPoor = improveColumns.Count(f => f.IsPoor.Value == true);
				
				#endif


				List<ColumnRank> destCols = improveColumns.FindAll(f => f.ColumnIndex != i.ColumnIndex && f.IsPoor.Value == true);
				if (destCols.Count > 0)
				{
					int selectedPoorIndex = new Random(Guid.NewGuid().GetHashCode()).Next(0, destCols.Count - 1);
					if (destCols.Count == 2)
					{
						selectedPoorIndex = Convert.ToInt32(Math.Abs(Guid.NewGuid().GetHashCode() % 2));
					}

					ColumnRank swapCol = destCols.ElementAt(selectedPoorIndex);

					if (schedule.Slot[swapCol.ColumnIndex] != null && schedule.Slot[i.ColumnIndex] != null)
					{
						
						//Swap objects
						Program.SwapRef(ref schedule.Slot[swapCol.ColumnIndex], ref schedule.Slot[i.ColumnIndex]);
						isChanged = true;
						i.IsPoor = false;
						break;
					}


				}
				else
				{
					Trace.WriteLine("Poor column were not found!");
				}
			}


			
		}

	}


	public class EmptySlot
	{
		public int? Col { get; set; }
		public int? Row { get; set; }
		public bool IsEmpty { get; set; }



	}

	public class ColumnRank
	{
		public int ColumnIndex { get; set; }
		public int TotalAir { get; set; }
		public bool MustImprove 
		{
			get
			{
				return TotalAir <= 1;
			}
		}
		public bool? IsPoor { get; set; }

		public int TotalWating { get; set; }
		EmptySlot EmptySlot { get; set; }
	}

}
