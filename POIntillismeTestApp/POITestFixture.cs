using System;
using NUnit.Framework;
using POIntillismeApp;
using System.Collections.Generic;

namespace POIntillismeTest
{
	[TestFixture]
	public class POITestFixture
	{
		[SetUp]
		public void Setup()
		{
			DBManager.Instance.CreateTable();
		}

		[Test]
		public void CreatePOI()
		{
			int testId = 1091;
			PointOfInterest newPOI = new PointOfInterest();
			newPOI.Id = testId;
			newPOI.Name = "New POI";
			newPOI.Description = "POI to test creating a new POI";
			newPOI.Address = "100 Main Street\nAnywhere, TX 75069";

			// Saving POI record
			int recordsUpdated = DBManager.Instance.SavePOI(newPOI);

			// Check if the number of records updated are the same as expected
			Assert.AreEqual(1, recordsUpdated);

			// Verify if the newly create POI exists
			PointOfInterest poi = DBManager.Instance.GetPOI(testId);
			Assert.NotNull(poi);
			Assert.AreEqual(poi.Name, "New POI");
		}

		[Test]
		public void DeletePOI()
		{
			int testId = 1019;
			PointOfInterest testPOI = new PointOfInterest();
			testPOI.Id = testId;
			testPOI.Name = "Delete POI";
			testPOI.Description = "POI being saved so we can test delete";
			testPOI.Address = "100 Main Street\nAnywhere, TX 75069";
			DBManager.Instance.SavePOI(testPOI);

			PointOfInterest deletePOI = DBManager.Instance.GetPOI(testId);
			Assert.NotNull(deletePOI);

			DBManager.Instance.DeletePOI(testId);	

			PointOfInterest poi = DBManager.Instance.GetPOI(testId);
			Assert.Null(poi);
		}

		[Test]
		public void ClearCache()
		{
			DBManager.Instance.ClearPOICache();
			List<PointOfInterest> poiList = DBManager.Instance.GetPOIListFromCache();
			Assert.AreEqual(0, poiList.Count);
		}

	}
}
