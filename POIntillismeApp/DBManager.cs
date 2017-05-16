using System;
using SQLite;
using System.Collections.Generic;

namespace POIntillismeApp
{
	public class DBManager
	{
		private const string DB_NAME = "PointOfInterest_DB.db3";

		private static readonly DBManager instance = new DBManager();

		SQLiteConnection dbConn;

		private DBManager()
		{
		}

		public static DBManager Instance
		{
			get 
			{
				return instance;
			}
		}

		public void CreateTable()
		{
			var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			dbConn = new SQLiteConnection(System.IO.Path.Combine (path, DB_NAME));
			dbConn.CreateTable<PointOfInterest>();
		}

		public int SavePOI(PointOfInterest poi) 
		{
			int result = dbConn.InsertOrReplace(poi);
			Console.WriteLine("{0} record updated!", result);
			return result;
		}

		public int InsertAll(List<PointOfInterest> poiListData)
		{
			int result = dbConn.InsertAll(poiListData);
			return result;
		}

		public List<PointOfInterest> GetPOIListFromCache() 
		{
			var poiListData = new List<PointOfInterest>();
			IEnumerable<PointOfInterest> table = dbConn.Table<PointOfInterest>();
			foreach (PointOfInterest poi in table) 
			{
				poiListData.Add(poi);
			}
			return poiListData;
		}

		public PointOfInterest GetPOI(int poiId)
		{
			PointOfInterest poi = dbConn.Table<PointOfInterest>().Where(a => a.Id.Equals(poiId)).FirstOrDefault();
			return poi;
		}

		public int DeletePOI(int poiId) 
		{
			int result = dbConn.Delete<PointOfInterest>(poiId);
			Console.WriteLine("{0} record effected!", result);
			return result;
		}

		public int ClearPOICache()
		{
			int result = dbConn.DeleteAll<PointOfInterest>();
			Console.WriteLine("{0} records effected!", result);
			return result;
		}
	}
}
