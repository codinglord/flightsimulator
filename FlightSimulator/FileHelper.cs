using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Data.OleDb;

namespace FlightSimulator
{
	static class FileHelper
	{
		public static void CreateFile(string fileName, string data,string name)
		{

			string template = GetTemplate();
			string content = template.Replace("{!@#$%^&*()_+}", data).Replace("+_)(*&^%$#@!",name);
			string directoryName = fileName.Replace(fileName.Split('/').Last(), string.Empty);

			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (!File.Exists(fileName))
			{
				using (StreamWriter w = File.CreateText(fileName))
				{
					w.Write(content);
				}
			}
		}

		public static void CreateFile(string fileName, string data)
		{


			string directoryName = fileName.Replace(fileName.Split('/').Last(), string.Empty);

			if (!Directory.Exists("c:/simu"))
			{
				Directory.CreateDirectory("c:/simu");
			}

			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}

			if (File.Exists(fileName))
			{
				try
				{
					File.Delete(fileName);
				}
				catch (Exception)
				{
					
				}
			}

			if (!File.Exists(fileName))
			{
				using (StreamWriter w = File.CreateText(fileName))
				{
					w.Write(data);
				}
			}
		}


		private static string GetTemplate()
		{
			return File.ReadAllText(ConfigurationManager.AppSettings["Template"]);
		}

		//private static DataTable _Logger = new DataTable();







		const string CSS = @"


<style type='text/css'>
table.sample {
	border-width: 1px;
	border-spacing: 2px;
	border-style: solid;
	border-color: green;
	border-collapse: separate;
	background-color: rgb(255, 250, 250);
}
table.sample th {
	border-width: 1px;
	padding: 1px;
	border-style: solid;
	border-color: green;
	background-color: white;
	-moz-border-radius: ;
}
table.sample td {
	border-width: 1px;
	padding: 1px;
	border-style: solid;
	border-color: green;
	background-color: white;
	-moz-border-radius: ;
}
body{font-size:xx-small;}
</style>
";


		const string BODY =
@"


	<tr></tr>
		<td>{0}</td><td>{1}</td>
	<tr></tr>



";





		public static void AppendText(string fileName,string msg,CalucationData cal, List<MatrixCompare> cCompare)
		{

			if (!Directory.Exists("c:/simu"))
			{
				Directory.CreateDirectory("c:/simu");
			}

			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("<html><head>{0}</head><body>", CSS);

			sb.AppendLine(string.Format("<h2>{0}</h2>", fileName));

			sb.AppendLine("<div style='float:left'><table class='sample'>");

			sb.AppendFormat(BODY,"T",cal.T);
			sb.AppendFormat(BODY,"TT",cal.TT);
			sb.AppendFormat(BODY,"Iteration",cal.Iteration);
			sb.AppendFormat(BODY,"SigmaT",cal.SigmaT);
			sb.AppendFormat(BODY,"DeltaT",cal.DeltaT);
			sb.AppendFormat(BODY,"TZero",cal.TZero);
			sb.AppendFormat(BODY,"AlphaExpoential",cal.AlphaExpoential);
			sb.AppendFormat(BODY,"K",cal.K);
			sb.AppendFormat(BODY,"Gramma",cal.Gramma);
			sb.AppendFormat(BODY,"CurrentT",cal.CurrentT);
			sb.AppendFormat(BODY,"PreviouseT",cal.PreviouseT);
			sb.AppendFormat(BODY, "TotalWatingTime", cal.TotalWatingTime);



			sb.AppendLine("</table></div>");


			sb.AppendLine("<div style='float:left'><table class='sample'>");

			sb.AppendFormat(BODY, "Adaptive1", cal.Adaptive1);
			sb.AppendFormat(BODY, "Adaptive2", cal.Adaptive2);
			sb.AppendFormat(BODY, "Adaptive3", cal.Adaptive3);
			sb.AppendFormat(BODY, "Adaptive4", cal.Adaptive4);
			sb.AppendFormat(BODY, "Exponential", cal.Exponential);
			sb.AppendFormat(BODY, "Geometric", cal.Geometric);
			sb.AppendFormat(BODY, "Linear", cal.Linear);
			sb.AppendFormat(BODY, "Logarithmic", cal.Logarithmic);

			sb.AppendLine("</table></div>");

			sb.AppendLine("<div style='float:left'><table class='sample'>");

			sb.AppendFormat(BODY, "ProbAdaptive1", cal.ProbAdaptive1);
			sb.AppendFormat(BODY, "ProbAdaptive2", cal.ProbAdaptive2);
			sb.AppendFormat(BODY, "ProbAdaptive3", cal.ProbAdaptive3);
			sb.AppendFormat(BODY, "ProbAdaptive4", cal.ProbAdaptive4);
			sb.AppendFormat(BODY, "ProbExponential", cal.ProbExponential);
			sb.AppendFormat(BODY, "ProbGeometric", cal.ProbGeometric);
			sb.AppendFormat(BODY, "ProbLinear", cal.ProbLinear);
			sb.AppendFormat(BODY, "ProbLogarithmic", cal.ProbLogarithmic);

			sb.AppendLine("</table></div>");


			sb.AppendLine("<br />");
			sb.AppendLine("<br />");
			
			sb.AppendLine("<br />");
			sb.AppendLine("<div style='clear:both;'>&nbsp;</div>");
			sb.AppendLine("<hr />");
			sb.AppendLine("<div style='float:left'><table  class='sample'>");
			int count = 0;
			foreach (var i in cal.Schedules)
			{
				sb.AppendLine("<tr>");

				for (int j = 0; j < i.Slot.Length; j++)
				{
					MatrixCompare jCompare = cCompare.Find(f => f.Position == string.Format("({0},{1})", count, j));

					string iColor = "green";
					if (jCompare != null)
					{
						iColor = jCompare.Changed ? "red" : "green";
					}
						

					Air jAir = i.Slot[j];
					if (jAir == null)
					{
						sb.AppendFormat("<td>({0},{1})</td>",count,j);
					}
					else
					{

						

						sb.AppendFormat(
							"<td style='background-color:green;'>" +
							"({4},{5})<br />" +
							"WT : {2}<br />" +
							"AC : {6}<br />" +
							//"D : {1}<br />" +
							//"No : {3}<br />" +
							"</td>",
							jAir.DisplayArrivalTime,
							jAir.DisplayDepartureTime,
							jAir.WaitingDurationLog,
							jAir.No,
							count,
							j,
							jAir.AccumulateTimeLog
						);
					}
				}
				count++;
				sb.AppendLine("</tr>");
			}
			sb.AppendLine("</table></div>");
			sb.AppendLine("</html>");
			//Keep data to text file
			Console.WriteLine("Trying to save file.");
			string iFolder = string.Format("c:/simu/log{0:ddMMyyyy}/", DateTime.Now);
			string iExcelName = string.Format("c:/simu/log{0:ddMMyyyy}/{1}", DateTime.Now, "calculation.xls");
			string iDestination = string.Format("c:/simu/log{0:ddMMyyyy}/{1}.html", DateTime.Now, fileName);

			CreateFile(iDestination, sb.ToString());


			try
			{
				AppendExcel(iExcelName, msg, cal, cCompare);
			}
			catch (Exception)
			{

			}

			//Process.Start(ConfigurationManager.AppSettings["IE"], "-private " + iDestination);
		}


		public static void AppendExcel(string fileName, string msg, CalucationData cal, List<MatrixCompare> cCompare)
		{
			string excelPath = fileName;

			if (!File.Exists(excelPath))
			{
				File.Copy("c:/simu/data/calculation.xls", fileName);
			}

			
			//Iteration	T	TT	SigmaT	DeltaT	TZero	AlphaExpoential	K	Gramma	CurrentT	PreviouseT	TotalWatingTime	Adaptive1	Adaptive2	Adaptive3	Adaptive4	Exponential	Geometric	Linear	Logarithmic	ProbAdaptive1	ProbAdaptive2	ProbAdaptive3	ProbAdaptive4	ProbExponential	ProbGeometric	ProbLinear	ProbLogarithmic
			string columns = "Iteration,T,TT,SigmaT,DeltaT,TZero,AlphaExpoential,K,Gramma,CurrentT,PreviouseT,TotalWatingTime,Adaptive1,Adaptive2,Adaptive3,Adaptive4,Exponential,Geometric,Linear,Logarithmic,ProbAdaptive1,ProbAdaptive2,ProbAdaptive3,ProbAdaptive4,ProbExponential,ProbGeometric,ProbLinear,ProbLogarithmic";
			string values = "@Iteration,@T,@TT,@SigmaT,@DeltaT,@TZero,@AlphaExpoential,@K,@Gramma,@CurrentT,@PreviouseT,@TotalWatingTime,@Adaptive1,@Adaptive2,@Adaptive3,@Adaptive4,@Exponential,@Geometric,@Linear,@Logarithmic,@ProbAdaptive1,@ProbAdaptive2,@ProbAdaptive3,@ProbAdaptive4,@ProbExponential,@ProbGeometric,@ProbLinear,@ProbLogarithmic";
			
			string updateExPression = "Iteration = @Iteration, T = @T,TT = @TT,SigmaT = @SigmaT,DeltaT = @DeltaT,TZero = @TZero,AlphaExpoential = @AlphaExpoential,K = @K,Gramma = @Gramma,CurrentT = @CurrentT,PreviouseT = @PreviouseT,TotalWatingTime = @TotalWatingTime,Adaptive1 = @Adaptive1,Adaptive2 = @Adaptive2,Adaptive3 = @Adaptive3,Adaptive4 = @Adaptive4,Exponential = @Exponential,Geometric = @Geometric,Linear = @Linear,Logarithmic = @Logarithmic,ProbAdaptive1 = @ProbAdaptive1,ProbAdaptive2 = @ProbAdaptive2,ProbAdaptive3 = @ProbAdaptive3,ProbAdaptive4 = @ProbAdaptive4,ProbExponential = @ProbExponential,ProbGeometric = @ProbGeometric,ProbLinear = @ProbLinear,ProbLogarithmic = @ProbLogarithmic";

			string cmdInsert = string.Format("insert into [Results$] ({0}) values ({1})",columns,values);
			string cmdUpdate = string.Format("update [Results$] set {0} where Iteration = @Iteration",updateExPression);

			string connectionString = string.Format(
				@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Excel 8.0;HDR=YES;""",
				excelPath);
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

					OleDbCommand cmd = new OleDbCommand(string.Format("select count(*) from [Results$] where Iteration = '{0}'", cal.Iteration), connection);
					int counter = Convert.ToInt32(cmd.ExecuteScalar());
					if (counter > 0)
					{
						cmd.CommandText = cmdUpdate;
					}
					else
					{
						cmd.CommandText = cmdInsert;
					}
					#region Parameters

					cmd.Parameters.AddWithValue("Iteration", GetParamValue(cal.Iteration));
						cmd.Parameters.AddWithValue("T",GetParamValue(cal.T));
						cmd.Parameters.AddWithValue("TT",GetParamValue(cal.TT));
						cmd.Parameters.AddWithValue("SigmaT",GetParamValue(cal.SigmaT));
						cmd.Parameters.AddWithValue("DeltaT",GetParamValue(cal.DeltaT));
						cmd.Parameters.AddWithValue("TZero",GetParamValue(cal.TZero));
						cmd.Parameters.AddWithValue("AlphaExpoential",GetParamValue(cal.AlphaExpoential));
						cmd.Parameters.AddWithValue("K",GetParamValue(cal.K));
						cmd.Parameters.AddWithValue("Gramma",GetParamValue(cal.Gramma));
						cmd.Parameters.AddWithValue("CurrentT", GetParamValue(cal.CurrentT));
						cmd.Parameters.AddWithValue("PreviouseT",GetParamValue(cal.PreviouseT));
						cmd.Parameters.AddWithValue("TotalWatingTime",GetParamValue(cal.TotalWatingTime));
						cmd.Parameters.AddWithValue("Adaptive1",GetParamValue(cal.Adaptive1));
						cmd.Parameters.AddWithValue("Adaptive2",GetParamValue(cal.Adaptive2));
						cmd.Parameters.AddWithValue("Adaptive3",GetParamValue(cal.Adaptive3));
						cmd.Parameters.AddWithValue("Adaptive4",GetParamValue(cal.Adaptive4));
						cmd.Parameters.AddWithValue("Exponential",GetParamValue(cal.Exponential));
						cmd.Parameters.AddWithValue("Geometric",GetParamValue(cal.Geometric));
						cmd.Parameters.AddWithValue("Linear",GetParamValue(cal.Linear));
						cmd.Parameters.AddWithValue("Logarithmic",GetParamValue(cal.Logarithmic));
						cmd.Parameters.AddWithValue("ProbAdaptive1",GetParamValue(cal.ProbAdaptive1));
						cmd.Parameters.AddWithValue("ProbAdaptive2",GetParamValue(cal.ProbAdaptive2));
						cmd.Parameters.AddWithValue("ProbAdaptive3",GetParamValue(cal.ProbAdaptive3));
						cmd.Parameters.AddWithValue("ProbAdaptive4",GetParamValue(cal.ProbAdaptive4));
						cmd.Parameters.AddWithValue("ProbExponential",GetParamValue(cal.ProbExponential));
						cmd.Parameters.AddWithValue("ProbGeometric",GetParamValue(cal.ProbGeometric));
						cmd.Parameters.AddWithValue("ProbLinear",GetParamValue(cal.ProbLinear));
						cmd.Parameters.AddWithValue("ProbLogarithmic",GetParamValue(cal.ProbLogarithmic));

					#endregion Parameters
				
					int result = cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}







			//Process.Start(ConfigurationManager.AppSettings["IE"], "-private " + iDestination);
		}

		private static object GetParamValue(object obj)
		{
			if (obj is double?)
			{
				if (!(obj as Double?).HasValue)
				{
					return System.DBNull.Value;
				}
			}
			else if (obj == null)
			{
				return System.DBNull.Value;
			}

			return obj;
		}


	}
}
