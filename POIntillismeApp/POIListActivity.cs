using Android.App;
using Android.OS;
using Android.Views;
using Android.Content;

namespace POIntillismeApp
{
	[Activity (Label = "POI List", MainLauncher = true)]
	public class POIListActivity : Activity
	{
		public static bool isDualMode = false; 

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.POIList);

			var detailsLayout = FindViewById (Resource.Id.poiDetailLayout);
			if (detailsLayout != null && detailsLayout.Visibility == ViewStates.Visible) {
				isDualMode = true;
			} else {
				isDualMode = false;
			}

			DBManager.Instance.CreateTable();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
		}
	}
}
