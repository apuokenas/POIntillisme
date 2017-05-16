using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Android.Locations;

namespace POIntillismeApp
{
	public class POIListFragment : ListFragment, ILocationListener
	{
		private ProgressBar progressBar;
		private List<PointOfInterest> poiListData;
		private POIListViewAdapter poiListAdapter;
		private Activity activity;

		int scrollPosition;

		LocationManager locMgr;

		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);
			this.activity = activity;
		}



		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			if (null != savedInstanceState) {
				scrollPosition = savedInstanceState.GetInt("scroll_position");	
			}
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			int currentPosition = ListView.FirstVisiblePosition;
			outState.PutInt("scroll_position", currentPosition);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.POIListFragment, container, false);
			progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar);

			SetHasOptionsMenu(true);

			locMgr = (LocationManager)Activity.GetSystemService(Context.LocationService);

			return view;
		}

		public void OnLocationChanged(Location location)
		{
			if (null != poiListAdapter) {
				poiListAdapter.CurrentLocation = location;
				poiListAdapter.NotifyDataSetChanged();
			}
		}

		public void OnProviderDisabled(string provider)
		{
		}

		public void OnProviderEnabled(string provider)
		{
		}

		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
		}

		public override void OnResume()
		{
			base.OnResume();
			DownloadPoisListAsync();

			Criteria criteria = new Criteria();
			criteria.Accuracy = Accuracy.NoRequirement;
			criteria.PowerRequirement = Power.NoRequirement;
			string provider = locMgr.GetBestProvider(criteria, true);
			locMgr.RequestLocationUpdates(provider, 2000, 100, this);
		}

		public override void OnPause()
		{
			base.OnPause();
			locMgr.RemoveUpdates(this);
		}


		public async void DownloadPoisListAsync() {
			POIService service = new POIService();
			if (!service.isConnected(activity)) {
				Toast toast = Toast.MakeText(activity, "Not conntected to the internet. Please, check your device network settings.", ToastLength.Short);
				toast.Show();
				poiListData = DBManager.Instance.GetPOIListFromCache();
			} else {
				progressBar.Visibility = ViewStates.Visible;
				poiListData = await service.GetPOIListAsync();

				// Clear cached data
				DBManager.Instance.ClearPOICache();

				// Save updated POI data
				DBManager.Instance.InsertAll(poiListData);

				progressBar.Visibility = ViewStates.Gone;
			}

			poiListAdapter = new POIListViewAdapter(activity, poiListData);
			this.ListAdapter = poiListAdapter;
			ListView.Post(() => {
				ListView.SetSelection(scrollPosition);
			});
		}


		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.POIListViewMenu, menu);
			base.OnCreateOptionsMenu(menu, inflater);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Resource.Id.actionNew:
				if (POIListActivity.isDualMode) {
					var detailFragment = new POIDetailFragment();
					FragmentTransaction ft = FragmentManager.BeginTransaction();
					ft.Replace(Resource.Id.poiDetailLayout, detailFragment);
					ft.Commit();
				} else {
					Intent intent = new Intent(activity, typeof(POIDetailActivity));
					StartActivity(intent);
				}
				return true;
			case Resource.Id.actionRefresh:
				DownloadPoisListAsync();
				return true;
			default :
				return base.OnOptionsItemSelected(item);
			}
		}

		public override void OnListItemClick(ListView l, View v, int position, long id)
		{
			PointOfInterest poi = poiListData[position];
			if (POIListActivity.isDualMode) {
				var detailFragment = new POIDetailFragment();
				detailFragment.Arguments = new Bundle();
				detailFragment.Arguments.PutString("poi", JsonConvert.SerializeObject(poi));

				FragmentTransaction ft = FragmentManager.BeginTransaction();
				ft.Replace(Resource.Id.poiDetailLayout, detailFragment);
				ft.Commit();
			} else {
				Intent poiDetailIntent = new Intent(activity, typeof(POIDetailActivity));
				poiDetailIntent.PutExtra("poi", JsonConvert.SerializeObject(poi));
				StartActivity(poiDetailIntent);
			}
		}
	}
}
