using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FlightSimulator
{

    class Program
    {

        private static DateTime _StartTime;
        private static DateTime _EndTime;
        private static readonly int _Tick = 1;
        private static int _TotalTick;
        private static readonly CultureInfo _Culture = CultureInfo.GetCultureInfo("en-US");
        private static StringBuilder _Log = new StringBuilder();

        private static string TestNo;
        public static int Seq = 1;
		public static int CalculationSeq = 1;

        public static DateTime CurrentTick { get; set; }
		public const int MAX_AIR = 18;
		public const int MAX_SCHEDULE = 14;
		public const int FIXED_RULE = 5;

        public static double UserGramma { get; private set; }
		public static double UserTZero { get; private set; }
		public static double UserAlphaExponential { get; private set; }
		public static double UserAlphaLinear { get; private set; }
		public static double UserAlphaGeomatric { get; private set; }
        public static int UserIteration { get; private set; }
		public static double UserStopTemparation { get; private set; }
		public const string FORMAT_DATE = "(dd) hh:mm tt";

		public static DateTime CurrentTime; 

        static Program()
        {
            Program.LoadConfiguration();
			
		}

		static void Main(string[] args)
		{

			string[] testingType = new string[] { 
				"iad1", "iad2", "iad3", "iad4", "iex", "ige", "ili", "ilo", 
				"tad1", "tad2", "tad3", "tad4", "tex", "tge", "tli", "tlo"
			};


			Console.WriteLine("Enter below probability names for testing.");
			Console.WriteLine(
@"
iad1, iad2, iad3, iad4, iex, ige, ili, ilo, 
tad1, tad2, tad3, tad4, tex, tge, tli, tlo
");
			string uInput = Console.ReadLine();


			args = uInput.Split(new string[] { " " }, StringSplitOptions.None);

			double stopTemarature = Program.UserStopTemparation;
			CalculationOutput cUnitTest = new CalculationOutput();

			Console.WriteLine("Sample data have been intialized.");

			InitTesting(args, cUnitTest);

			Console.WriteLine("Started -> Iteration testing.");

			//Iteration testing (8 tests);
			IterationTesting(ParseName("iad1"), Program.GetFlightData(), cUnitTest.IterationAdaptive1);
			IterationTesting(ParseName("iad2"), Program.GetFlightData(), cUnitTest.IterationAdaptive2);
			IterationTesting(ParseName("iad3"), Program.GetFlightData(), cUnitTest.IterationAdaptive3);
			IterationTesting(ParseName("iad4"), Program.GetFlightData(), cUnitTest.IterationAdaptive4);
			IterationTesting(ParseName("iex"), Program.GetFlightData(), cUnitTest.IterationExponential);
			IterationTesting(ParseName("ige"), Program.GetFlightData(), cUnitTest.IterationGeometric);
			IterationTesting(ParseName("ili"), Program.GetFlightData(), cUnitTest.IterationLinear);
			IterationTesting(ParseName("ilo"), Program.GetFlightData(), cUnitTest.IterationLogarithmic);
			Console.WriteLine("Finished -> Iteration testing.");

			//Temparature testing (8 tests);
			Console.WriteLine("Started -> Temparature testing.");
			TemparatureTesting(ParseName("tad1"), Program.GetFlightData(), cUnitTest.TemparatureAdaptive1);
			TemparatureTesting(ParseName("tad2"), Program.GetFlightData(), cUnitTest.TemparatureAdaptive2);
			TemparatureTesting(ParseName("tad3"), Program.GetFlightData(), cUnitTest.TemparatureAdaptive3);
			TemparatureTesting(ParseName("tad4"), Program.GetFlightData(), cUnitTest.TemparatureAdaptive4);
			TemparatureTesting(ParseName("tex"), Program.GetFlightData(), cUnitTest.TemparatureExponential);
			TemparatureTesting(ParseName("tge"), Program.GetFlightData(), cUnitTest.TemparatureGeometric);
			TemparatureTesting(ParseName("tli"), Program.GetFlightData(), cUnitTest.TemparatureLinear);
			TemparatureTesting(ParseName("tlo"), Program.GetFlightData(), cUnitTest.TemparatureLogarithmic);
			Console.WriteLine("Finished -> Temparature testing.");



			//Add data to collection






			Console.WriteLine("Press any key to close program.");
			Console.ReadKey();

			//Keep data to DB.
			//AirTableAdapter adp = new AirTableAdapter();
			//adp.Update(_LogAir);


		}

		public static void Reset()
		{
			_StartTime = Program.CurrentTime = GetTime("00:00");
			_EndTime = _StartTime.AddDays(7).AddHours(-12);
			_TotalTick = Convert.ToInt32((_EndTime - _StartTime).TotalMinutes);
			Matrix.ClearMatrix();
			Matrix.Init();
		}

        public static void LoadConfiguration()
        {

			Program.UserGramma = Convert.ToDouble(ConfigurationManager.AppSettings["UserGramma"]);
			Program.UserTZero = Convert.ToDouble(ConfigurationManager.AppSettings["UserTZero"]);
			Program.UserAlphaExponential = Convert.ToDouble(ConfigurationManager.AppSettings["UserAlphaExponential"]);
			Program.UserAlphaLinear = Convert.ToDouble(ConfigurationManager.AppSettings["UserAlphaLinear"]);
			Program.UserAlphaGeomatric = Convert.ToDouble(ConfigurationManager.AppSettings["UserAlphaGeomatric"]);
			Program.UserIteration = Convert.ToInt32(ConfigurationManager.AppSettings["UserIteration"]);
			Program.UserStopTemparation = Convert.ToDouble(ConfigurationManager.AppSettings["UserStopTemparation"]);
            
        }

		public static List<Air> GetFlightData()
		{
			#region Variables
			List<Air> airs = new List<Air>();
			List<DataTable> tables = new List<DataTable>();
			string connectionString = string.Format(
				@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Excel 8.0;HDR=YES;""",
				ConfigurationManager.AppSettings["EXCEL_PATH"]
				);


			#endregion Variables
			List<string> commands = new List<string>();
			


			/*Update data*/
			using (OleDbConnection connection = new OleDbConnection(connectionString))
			{
				if (connection.State != System.Data.ConnectionState.Closed)
				{
					connection.Close();
				}
				connection.Open();
				int count = 0;
				try
				{
					DataTable tbl = new DataTable();
					OleDbCommand cmd = new OleDbCommand("select * from [Sheet1$]", connection);

					tbl.Load(cmd.ExecuteReader());

					List<string> cAirName = new List<string>();

					foreach (DataRow j in tbl.Rows)
					{
						string jName = j["route"].ToString().Trim().ToLower();
						string jDay = j["day"].ToString();

						if (string.IsNullOrEmpty(jName))
						{
							break;
						}

						DateTime jDeaprture = Program.GetTime(jDay, Convert.ToInt32(j["dep_time"]).ToString("00:00"));
						DateTime jArrive = jDeaprture.AddMinutes(Convert.ToInt32(j["TotalF_Time"]));
							
						int jFlyingDuration = Convert.ToInt32((jArrive - jDeaprture).TotalMinutes);

						if (!cAirName.Exists(f => f == jName))
						{
							cAirName.Add(jName);
						}
						int jAirNo = cAirName.IndexOf(jName) + 1;
						airs.Add(
							new Air
							(jAirNo, jDeaprture,jArrive)
						);
						count++;
					}
					
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

				return airs;
			}
		}

        public static DateTime GetTime(int hh,int mm)
        { 
            //en-US format 12/12/2012 1:45:30 PM;
            return Convert.ToDateTime(
                string.Format("12/12/2012 {0}:{1}:0 AM", hh, mm)
                , _Culture);
        }

        public static DateTime GetTime(string time)
        {
            //en-US format 12/12/2012 1:45:30 PM;
            return Convert.ToDateTime(
				string.Format("01/01/2012 {0} AM", time)
                , _Culture);
        }

		public static DateTime GetTime(string day, string time)
		{
			//en-US format 12/12/2012 1:45:30 PM;

			int hh = Convert.ToInt32(time.Split(':').First());
			if (hh > 12)
			{
				return Convert.ToDateTime(
					string.Format("01/{0}/2012 {1} PM", day, hh % 12)
					, _Culture);
			}
			else
			{
				return Convert.ToDateTime(
					string.Format("01/{0}/2012 {1} AM", day, hh)
					, _Culture);
			}


		}

		public static void SwapRef<T>(ref T param1, ref T param2)
		{
			T temp = param1;
			param1 = param2;
			param2 = temp;
		}

		public static T Clone<T>(T source)
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", "source");
			}

			// Don't serialize a null object, simply return the default for that object
			if (Object.ReferenceEquals(source, null))
			{
				return default(T);
			}

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}

		public static CalucationData Simulate(string name, int iteration, List<CalucationData> cCalculation, params Air[] airs)
        {

			CalucationData prevCalculation = cCalculation.LastOrDefault();
			List<MatrixCompare> checkList = new List<MatrixCompare>();
			//Init matrix.
			List<Maintainance> cMaintain = null;
			CalucationData cal = null;
			if (iteration == 0)
			{
				Program.Reset();
				Program.TestNo = DateTime.Now.ToString("ddMMyyyy_hhmmss");
				
				List<Air> cAir = new List<Air>(airs);
				int countTick = 0;
				while ((_EndTime - Program.CurrentTime).TotalMinutes > -1)
				{


					cAir.ForEach(f =>
					{
						if (f.DepartureTime >= Program.CurrentTime
							&& f.Status != AirStatus.M
						)
						{
							f.UpdateStatus();
						}
					});
					Program.CurrentTime = Program.CurrentTime.AddMinutes(_Tick);

					countTick++;
				}
				cMaintain = Matrix.Fixation(Matrix.MaintainanceCollection);
				cal = ComputationUnit.GetRawCalculationData(iteration, cMaintain);
				cal.StartCalculation(0);
				cCalculation.Add(cal);



				FileHelper.AppendText(name.Replace(".", iteration + "."), "First Matrix",cal, checkList);
			}
			else
			{
				cMaintain = Program.Clone(prevCalculation.Schedules);
				
				int count = 0;
				foreach (var i in cMaintain)
				{

					for (int j = 0; j < Program.MAX_AIR; j++)
					{
						if (i.Slot[j] != null)
						{
							checkList.Add(new MatrixCompare() { 
								Before = i.Slot[j].WaitingDurationLog ,
								Position = string.Format("({0},{1})", count, j)
							});
						}
					}
					count++;
				}


				List<ColumnRank> cPoorRank = Matrix.GetRankedColumns((cMaintain));
				//cMaintain = Matrix.FixPoorRank(cMaintain, cPoorRank);
				
				count = 0;
				foreach (var i in cMaintain)
				{

					for (int j = 0; j < Program.MAX_AIR; j++)
					{
						if (i.Slot[j] != null)
						{
							MatrixCompare iCompare = checkList.Find(f => f.Position == string.Format("({0},{1})", count, j));
							if (iCompare != null)
							{
								iCompare.After = i.Slot[j].WaitingDurationLog;
							}
							

			
						}
					}

					count++;
				}
				Console.WriteLine("========================================");
				Console.WriteLine("Iteration No.{0}", iteration);
				foreach (var i in checkList.Where(f => f.Changed))
				{
					Console.WriteLine(string.Format(
						"{3}\r\nBefore = {0}, After = {1}, Changed = {2}",
						i.Before,
						i.After,
						i.Changed,
						i.Position
					));
				}
				cal = ComputationUnit.GetRawCalculationData(iteration,cMaintain);
				string msg = "";
				if (prevCalculation.TotalWatingTime < cal.TotalWatingTime)
				{
					cal = prevCalculation;
				}
				else
				{
					msg = "New matrix was better!";
					Console.WriteLine(msg);
				}

				cal.StartCalculation(prevCalculation.SigmaT.Value);
				cCalculation.Add(cal);

				FileHelper.AppendText(name.Replace(".", iteration + "."), msg, cal, checkList);
			}

			return cal;
        }





		/// <summary>
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		


		public static string GetDataPackage(CalculationOutput data,string output)
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			serializer.MaxJsonLength = int.MaxValue - 1;
			return serializer.Serialize(new { Output = output,Data = data});
		}



        

		





		private static void TemparatureTesting(string name, List<Air> airs, List<CalucationData> cUnitTest)
		{
			if (cUnitTest != null)
			{
				double tempartureChecker = Program.UserTZero;
				int i = 0;
				//double prevSigmaT = -1;
				//bool changed = true;
				while (tempartureChecker >= Program.UserStopTemparation)
				{
					CalucationData iCal = Simulate(name + "_" + tempartureChecker,i, cUnitTest, Program.GetFlightData().ToArray());
					tempartureChecker = Program.GetTemparatureChecker(name, iCal).Value;
					//if (prevSigmaT != iCal.SigmaT.Value)
					//{
					//    prevSigmaT = iCal.SigmaT.Value;
					//    changed = true;
					//}
					//else
					//{
					//    changed = false;
					//}
					i++;
				}
			}

		}

		/// <summary>
		/// Loop all unit test.
		/// </summary>
		/// <param name="airs"></param>
		/// <param name="cUnitTest"></param>
		private static void IterationTesting(string name,List<Air> airs,List<CalucationData> cUnitTest)
		{
			if (cUnitTest != null)
			{
				for (int i = 0; i < Program.UserIteration; i++)//
				{
					Simulate(name +"_"+ i,i, cUnitTest, Program.GetFlightData().ToArray());
				}
			}
		}

		private static string ParseName(string i)
		{
			string iName = string.Empty;
			switch (i)
			{
				case "iad1":
					iName = "IterationAdaptive1";
					break;
				case "iad2":
					iName = "IterationAdaptive2";
					break;
				case "iad3":
					iName = "IterationAdaptive3";
					break;
				case "iad4":
					iName = "IterationAdaptive4";
					break;
				case "iex":
					iName = "IterationExponential";
					break;
				case "ige":
					iName = "IterationGeometric";
					break;
				case "ili":
					iName = "IterationLinear";
					break;
				case "ilo":
					iName = "IterationLogarithmic";
					break;
				case "tad1":
					iName = "TemparatureAdaptive1";
					break;
				case "tad2":
					iName = "TemparatureAdaptive2";
					break;
				case "tad3":
					iName = "TemparatureAdaptive3";
					break;
				case "tad4":
					iName = "TemparatureAdaptive4";
					break;
				case "tex":
					iName = "TemparatureExponential";
					break;
				case "tge":
					iName = "TemparatureGeometric";
					break;
				case "tli":
					iName = "TemparatureLinear";
					break;
				case "tlo":
					iName = "TemparatureLogarithmic";
					break;
			}
			return iName;
		}

		private static void InitTesting(string[] args, CalculationOutput cUnitTest)
		{
			if (args.Length > 0)
			{
				#region Init data via input

				if (args.Contains("iad1"))
				{
					cUnitTest.IterationAdaptive1 = new List<CalucationData>();
				}
				if (args.Contains("iad2"))
				{
					cUnitTest.IterationAdaptive2 = new List<CalucationData>();
				}
				if (args.Contains("iad3"))
				{
					cUnitTest.IterationAdaptive3 = new List<CalucationData>();
				}
				if (args.Contains("iad4"))
				{
					cUnitTest.IterationAdaptive4 = new List<CalucationData>();
				}
				if (args.Contains("iex"))
				{
					cUnitTest.IterationExponential = new List<CalucationData>();
				}
				if (args.Contains("ige"))
				{
					cUnitTest.IterationGeometric = new List<CalucationData>();
				}
				if (args.Contains("ili"))
				{
					cUnitTest.IterationLinear = new List<CalucationData>();
				}
				if (args.Contains("ilo"))
				{
					cUnitTest.IterationLogarithmic = new List<CalucationData>();
				}

				if (args.Contains("tad1"))
				{
					cUnitTest.TemparatureAdaptive1 = new List<CalucationData>();
				}
				if (args.Contains("tad2"))
				{
					cUnitTest.TemparatureAdaptive2 = new List<CalucationData>();
				}
				if (args.Contains("tad3"))
				{
					cUnitTest.TemparatureAdaptive3 = new List<CalucationData>();
				}
				if (args.Contains("tad4"))
				{
					cUnitTest.TemparatureAdaptive4 = new List<CalucationData>();
				}
				if (args.Contains("tex"))
				{
					cUnitTest.TemparatureExponential = new List<CalucationData>();
				}
				if (args.Contains("tge"))
				{
					cUnitTest.TemparatureGeometric = new List<CalucationData>();
				}
				if (args.Contains("tli"))
				{
					cUnitTest.TemparatureLinear = new List<CalucationData>();
				}
				if (args.Contains("tlo"))
				{
					cUnitTest.TemparatureLogarithmic = new List<CalucationData>();
				}

				#endregion Init data via input
			}
			else
			{
				cUnitTest.IterationAdaptive1 = new List<CalucationData>();
				cUnitTest.IterationAdaptive2 = new List<CalucationData>();
				cUnitTest.IterationAdaptive3 = new List<CalucationData>();
				cUnitTest.IterationAdaptive4 = new List<CalucationData>();
				cUnitTest.IterationExponential = new List<CalucationData>();
				cUnitTest.IterationGeometric = new List<CalucationData>();
				cUnitTest.IterationLinear = new List<CalucationData>();
				cUnitTest.IterationLogarithmic = new List<CalucationData>();

				cUnitTest.TemparatureAdaptive1 = new List<CalucationData>();
				cUnitTest.TemparatureAdaptive2 = new List<CalucationData>();
				cUnitTest.TemparatureAdaptive3 = new List<CalucationData>();
				cUnitTest.TemparatureAdaptive4 = new List<CalucationData>();
				cUnitTest.TemparatureExponential = new List<CalucationData>();
				cUnitTest.TemparatureGeometric = new List<CalucationData>();
				cUnitTest.TemparatureLinear = new List<CalucationData>();
				cUnitTest.TemparatureLogarithmic = new List<CalucationData>();
			}
		}

		private static double? GetTemparatureChecker(string i,CalucationData cal)
		{
			switch (i)
			{
				case "TemparatureAdaptive1":
					return cal.Adaptive1;
				case "TemparatureAdaptive2":
					return cal.Adaptive2;
				case "TemparatureAdaptive3":
					return cal.Adaptive3;
				case "TemparatureAdaptive4":
					return cal.Adaptive4;
				case "TemparatureExponential":
					return cal.Exponential;
				case "TemparatureGeometric":
					return cal.Geometric;
				case "TemparatureLinear":
					return cal.Linear;
				case "TemparatureLogarithmic":
					return cal.Logarithmic;
				default: throw new Exception(string.Format("Unexpected parameter name {0} for generating temparature test.",i));
			}
		}

    }

}
