using Android.App;
using Android.OS;
using System;
using Android.Content;

namespace POIntillismeApp
{
	[Activity (Label = "POIDetailActivity")]		
	public class POIDetailActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.POIDetail);

			SetProgressBarIndeterminateVisibility(true);

			if (null == bundle) {
				var detailFragment = new POIDetailFragment();
				detailFragment.Arguments = new Bundle();

				if (Intent.HasExtra ("poi")) {
					string poiJson = Intent.GetStringExtra("poi");
					detailFragment.Arguments.PutString("poi", poiJson);
				}

				FragmentTransaction ft = FragmentManager.BeginTransaction();
				ft.Add(Resource.Id.poiDetailLayout, detailFragment);
				ft.Commit();
			}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
		}
	}
}
