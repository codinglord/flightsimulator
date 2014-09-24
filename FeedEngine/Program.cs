using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using FeedEngine.com.morningstar.equityapi1;
using System.Diagnostics;

namespace FeedEngine
{
	static class Program
	{

		const string P_INDUSTRY_GROUP = "industry";
		const string P_BUSINESS_DESCRIPTION = "business_desc";

		public static DataTable GetData()
		{

			DataTable tbl = new DataTable();

			#region Variables
			
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
				try
				{

					OleDbCommand cmd = new OleDbCommand("select * from [Tickers$]", connection);

					tbl.Load(cmd.ExecuteReader());


				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

			
			}

			return tbl;

		}

		public static void AddCompanyInfos(CompanyInfos cCompanyInfos)
		{
			const string cmdInsert = @"
			insert into [Industries$] 
					(identifier,IndustryGroupId ,WebAddress , IndustryName , IndustryGroupName ,LocalName ,CompanyName) 
			values	(@identifier,@IndustryGroupId ,@WebAddress , @IndustryName , @IndustryGroupName ,@LocalName ,@CompanyName); ";
			
			DataTable tbl = new DataTable();

			#region Variables

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
				try
				{

				
					var i = cCompanyInfos;

					if (Array.TrueForAll(new object[] { i ,i.GeneralInfo,i.GeneralInfo.Symbol,i.CompanyInfoEntity.IndustryName,i.CompanyInfoEntity.IndustryGroupName}, f => f != null) )
					{
						OleDbCommand cmd = new OleDbCommand(cmdInsert, connection);
						cmd.Parameters.AddWithValue("@exchangeId", i.GeneralInfo.ExchangeId);
						cmd.Parameters.AddWithValue("@identifier", i.GeneralInfo.Symbol);
						cmd.Parameters.AddWithValue("@IndustryName",i.CompanyInfoEntity.IndustryName);
						cmd.Parameters.AddWithValue("@IndustryGroupName", i.CompanyInfoEntity.IndustryGroupName);
						cmd.Parameters.AddWithValue("@LocalName", i.CompanyInfoEntity.LocalName);
						cmd.Parameters.AddWithValue("@CompanyName", i.GeneralInfo.CompanyName);
						cmd.Parameters.AddWithValue("@WebAddress", i.CompanyInfoEntity.WebAddress);
						cmd.Parameters.AddWithValue("@IndustryGroupId",i.CompanyInfoEntity.IndustryGroupId);
						int result = (int)cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}


			}



		}

		public static void AddBusinessDescriptions(BusinessDescriptions cCompanyInfos)
		{

			const string cmdInsert = "insert into [BusinessDescription$] (identifier,DescENG,DescTHA) values (@identifier,@DescENG,@DescTHA);";

			#region Variables

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
				try
				{


					var i = cCompanyInfos;

					string iTHA = string.Empty;
					string iENG = string.Empty;
					foreach (var j in i.BusinessDescription.MediumDescription)
					{
						switch (j.LanguageCode)
						{
							case "THA" :
								iTHA = j.Value;
								break;
							case "ENG" :
								iENG = j.Value;
								break;
						}
					}


					OleDbCommand cmd = new OleDbCommand(cmdInsert, connection);
					cmd.Parameters.AddWithValue("@exchangeId", i.GeneralInfo.ExchangeId);
					cmd.Parameters.AddWithValue("@identifier",i.GeneralInfo.Symbol);
					cmd.Parameters.AddWithValue("@DescENG", iENG);
					cmd.Parameters.AddWithValue("@DescTHA", iTHA);
					int result = (int)cmd.ExecuteNonQuery();

				


				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}


			}



		}

		static void Main(string[] args)
		{


			

			DataTable tbl = GetData();
			Console.WriteLine("Engine started.");

			com.morningstar.equityapi1.InvestorRelationsService srv = new com.morningstar.equityapi1.InvestorRelationsService();
			foreach (DataRow i in tbl.Rows)
			{

				if ((i["identifier"] is System.DBNull))
				{
					continue;
				}

				object iExchangeId = "";


				
				


				if (args.Contains(P_INDUSTRY_GROUP))
				{
					try
					{
						CompanyInfos cInfo = srv.GetCompanyGeneralInformation(
							i["ExchangeId"].ToString(),
							CompanyInfoIdentifierType.Symbol,
							i["identifier"].ToString());
						
						AddCompanyInfos(cInfo);
					}
					catch (Exception ex){}
				}


				if (args.Contains(P_BUSINESS_DESCRIPTION))
				{
					try
					{
						BusinessDescriptions iBuissInfo = srv.GetBusinessDescription(
							i["ExchangeId"].ToString(),
							BusinessDescriptionIdentifierType.Symbol,
							i["identifier"].ToString()
						);
						AddBusinessDescriptions(iBuissInfo);
					}
					catch (Exception ex){}
				}

				Console.WriteLine("Loaded {0}", i["identifier"]);
			}

			
			Process.Start(ConfigurationManager.AppSettings["EXCEL_PATH"]);
			Console.ReadKey();
		}

		
	
	}
}
