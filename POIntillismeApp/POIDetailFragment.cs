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
using Android.Content.PM;
using Android.Provider;
using Android.Graphics;

namespace POIntillismeApp
{
	public class POIDetailFragment : Fragment, ILocationListener
	{
		PointOfInterest _poi;

		EditText _nameEditText;
		EditText _descrEditText;
		EditText _addrEditText;
		EditText _latEditText;
		EditText _longEditText;

		ImageView _poiImageView;

		ImageButton _locationImageButton;
		ImageButton _mapImageButton;
		ImageButton _photoImageButton;

		private Activity activity;

		const int CAPTURE_PHOTO = 100;

		LocationManager locMgr;

		public override void OnAttach(Activity activity)
		{
			base.OnAttach(activity);
			this.activity = activity;
		}


		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			if (Arguments != null && Arguments.ContainsKey("poi")) {
				string poiJson = Arguments.GetString("poi");
				_poi = JsonConvert.DeserializeObject<PointOfInterest>(poiJson);
			} else {
				_poi = new PointOfInterest();
			}
		}


		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.POIDetailFragment, container, false);

			_nameEditText = view.FindViewById<EditText>(Resource.Id.nameEditText);
			_descrEditText = view.FindViewById<EditText>(Resource.Id.descrEditText);
			_addrEditText = view.FindViewById<EditText>(Resource.Id.addrEditText);
			_latEditText = view.FindViewById<EditText>(Resource.Id.latEditText);
			_longEditText = view.FindViewById<EditText>(Resource.Id.longEditText);
			_poiImageView = view.FindViewById<ImageView>(Resource.Id.poiImageView);

			locMgr = (LocationManager)Activity.GetSystemService(Context.LocationService);

			_locationImageButton = view.FindViewById<ImageButton>(Resource.Id.locationImageButton);
			_locationImageButton.Click += GetLocationClicked;

			_mapImageButton = view.FindViewById<ImageButton>(Resource.Id.mapImageButton);
			_mapImageButton.Click += MapClicked;

			_photoImageButton = view.FindViewById<ImageButton>(Resource.Id.photoImageButton);
			_photoImageButton.Click += NewPhotoClicked;

			SetHasOptionsMenu(true);

			if (Arguments != null && Arguments.ContainsKey("poi")) {
				string poiJson = Arguments.GetString("poi");
				_poi = JsonConvert.DeserializeObject<PointOfInterest>(poiJson);
			} else {
				_poi = new PointOfInterest();
			}

			UpdateUI();

			return view;
		}

		public void OnLocationChanged(Location location)
		{
			// Remove progress dialog fragment
			FragmentTransaction ft = FragmentManager.BeginTransaction();
			ProgressDialogFragment dialogFragment = (ProgressDialogFragment)FragmentManager.FindFragmentByTag("progress_dialog");
			if (dialogFragment != null) {
				ft.Remove (dialogFragment).Commit();
			}

			_latEditText.Text = location.Latitude.ToString();
			_longEditText.Text = location.Longitude.ToString ();
			Geocoder geocdr = new Geocoder(activity);

			IList<Address> addresses = geocdr.GetFromLocation(location.Latitude, location.Longitude, 5);
			if (addresses.Any()) {
				UpdateAddressFields(addresses.First ());
			}
		}

		protected void UpdateAddressFields(Address addr)
		{
			if (String.IsNullOrEmpty(_nameEditText.Text))
				_nameEditText.Text = addr.FeatureName;
			if (String.IsNullOrEmpty(_addrEditText.Text)) 
			{
				for (int i = 0; i < addr.MaxAddressLineIndex; i++) {
					if (!String.IsNullOrEmpty(_addrEditText.Text))
						_addrEditText.Text += System.Environment.NewLine;
					
					_addrEditText.Text += addr.GetAddressLine(i);
				}
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

		protected void GetLocationClicked(object sender, EventArgs e)
		{
			Criteria criteria = new Criteria();
			criteria.Accuracy = Accuracy.NoRequirement;
			criteria.PowerRequirement = Power.NoRequirement;
			locMgr.RequestSingleUpdate(criteria, this, null);

			FragmentTransaction ft = FragmentManager.BeginTransaction();
			var dialogFragment = new ProgressDialogFragment ();
			dialogFragment.Show(ft, "progress_dialog");

		}

		protected void MapClicked(object sender, EventArgs e){
			Android.Net.Uri geoUri;
			if (String.IsNullOrEmpty(_addrEditText.Text)) {
				geoUri = Android.Net.Uri.Parse(String.Format("geo:{0},{1}", _poi.Latitude, _poi.Longitude));
			}
			else {
				geoUri = Android.Net.Uri.Parse(String.Format("geo:0,0?q={0}", _addrEditText.Text));
			}
			Intent mapIntent = new Intent(Intent.ActionView, geoUri);

			PackageManager packageManager = Activity.PackageManager;
			IList<ResolveInfo> activities = packageManager.QueryIntentActivities(mapIntent, 0);
			if (activities.Count == 0) {
				Toast.MakeText(activity, "No map app available.", ToastLength.Short).Show();
			} 
			else
			{
				StartActivity(mapIntent);
			}
		}

		void NewPhotoClicked(object sender, EventArgs e)
		{
			if (_poi.Id <= 0) {
				Toast.MakeText(activity, "You must save the POI before attaching a photo.", ToastLength.Short).Show();
				return;
			}

			Intent cameraIntent = new Intent(MediaStore.ActionImageCapture);
			PackageManager packageManager = Activity.PackageManager;
			IList<ResolveInfo> activities = packageManager.QueryIntentActivities(cameraIntent, 0);
			if (activities.Count == 0) {
				Toast.MakeText(activity, "No camera app available.", ToastLength.Short).Show();
			} else {
				string path = POIService.GetFileName(_poi.Id);
				Java.IO.File imageFile = new Java.IO.File(path);
				Android.Net.Uri imageUri = Android.Net.Uri.FromFile(imageFile);
				cameraIntent.PutExtra(MediaStore.ExtraOutput, imageUri);
				cameraIntent.PutExtra(MediaStore.ExtraSizeLimit, 1 * 1024 * 1024);
				StartActivityForResult(cameraIntent, CAPTURE_PHOTO);
			}
		}

		protected void UpdateUI()
		{
			_nameEditText.Text = _poi.Name;
			_descrEditText.Text = _poi.Description;
			_addrEditText.Text = _poi.Address;
			_latEditText.Text = _poi.Latitude.ToString();
			_longEditText.Text = _poi.Longitude.ToString();
		}

		protected void SavePOI()
		{
			bool errors = false;
			if (String.IsNullOrEmpty(_nameEditText.Text)) {
				_nameEditText.Error = "Name cannot be empty";
				errors = true;
			} else {
				_nameEditText.Error = null;
			}

			double? tempLatitude = null;
			if (!String.IsNullOrEmpty(_latEditText.Text)) {
				try {
					tempLatitude = Double.Parse(_latEditText.Text);
					if ((tempLatitude > 90) | (tempLatitude < -90)) {
						_latEditText.Error = "Latitude must be a decimal value between -90 and 90";
						errors = true;
					} else {
						_latEditText.Error = null;
					}
				}
				catch {
					_latEditText.Error = "Latitude must be valid decimal number";
					errors = true;
				}
			}

			double? tempLongitude = null;
			if (!String.IsNullOrEmpty(_longEditText.Text)) {
				try {
					tempLongitude = Double.Parse(_longEditText.Text);
					if ((tempLongitude > 180) | (tempLongitude < -180)) {
						_longEditText.Error = "Longitude must be a decimal value between -180 and 180";
						errors = true;
					} else {
						_longEditText.Error = null;
					}
				} catch {
					_longEditText.Error = "Longitude must be valid decimal number";
					errors = true;
				}
			}

			if (errors) {
				return;
			}

			_poi.Name = _nameEditText.Text;
			_poi.Description = _descrEditText.Text;
			_poi.Address = _addrEditText.Text;
			_poi.Latitude = tempLatitude;
			_poi.Longitude = tempLongitude;

			CreateOrUpdatePOIAsync(_poi);
		}

		private async void CreateOrUpdatePOIAsync(PointOfInterest poi) {
			POIService service = new POIService();
			if (!service.isConnected(activity)) {
				Toast toast = Toast.MakeText(activity, "Not connected to the internet. Please, check your device network settings.", ToastLength.Short);
				toast.Show();
				return;
			}

			Bitmap bitmap = null;
			if (_poi.Id > 0) {
				bitmap = POIService.GetImage(_poi.Id);	
			}

			string response = null;
			if (bitmap != null) {
				response = await service.CreateOrUpdatePOIAsync(_poi, bitmap);
			} else {
				response = await service.CreateOrUpdatePOIAsync(_poi);
			}

			if (!string.IsNullOrEmpty(response)) {
				Toast toast = Toast.MakeText(activity, String.Format("{0} saved.", _poi.Name), ToastLength.Short);
				toast.Show();

				DBManager.Instance.SavePOI(poi);

				if (!POIListActivity.isDualMode)
					activity.Finish();
			} else {
				Toast toast = Toast.MakeText(activity, "Something went wrong!", ToastLength.Short);
				toast.Show();
			}


			if (bitmap != null) {
				bitmap.Dispose();
				bitmap = null;
			}

		}

		protected void DeletePOI()
		{
			FragmentTransaction ft = FragmentManager.BeginTransaction();

			// Create and show the dialog
			DeleteDialogFragment dialogFragment = new DeleteDialogFragment();
			dialogFragment.SetTargetFragment(this, 0);

			Bundle bundle = new Bundle();
			bundle.PutString("name", _poi.Name);
			dialogFragment.Arguments = bundle;

			// Add fragment
			dialogFragment.Show(ft, "dialog");
		}

		public async void DeletePOIAsync(){
			POIService service = new POIService();
			if (!service.isConnected(activity)) {
				Toast toast = Toast.MakeText(activity, "Not conntected to the internet. Please, check your device network settings.", ToastLength.Short);
				toast.Show();
				return;
			}

			string response = await service.DeletePOIAsync(_poi.Id);
			if (!string.IsNullOrEmpty(response)) {
				Toast toast = Toast.MakeText(activity, String.Format("{0} deleted.", _poi.Name), ToastLength.Short);
				toast.Show();

				DBManager.Instance.DeletePOI(_poi.Id);

				if (!POIListActivity.isDualMode)
					activity.Finish();
			} else {
				Toast toast = Toast.MakeText(activity, "Something went wrong!", ToastLength.Short);
				toast.Show();
			}
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.POIDetailMenu, menu);
			base.OnCreateOptionsMenu(menu, inflater);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Resource.Id.actionSave:
				SavePOI();
				return true;
			case Resource.Id.actionDelete: 
				DeletePOI();
				return true;
			default:
				return base.OnOptionsItemSelected(item);
			}
		}

		public override void OnPrepareOptionsMenu(IMenu menu)
		{
			base.OnPrepareOptionsMenu(menu);
			if (_poi.Id <= 0) {
				IMenuItem item = menu.FindItem(Resource.Id.actionDelete);
				item.SetEnabled(false);
				item.SetVisible(false);
			}
		}


		public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == CAPTURE_PHOTO) {
				if (resultCode == Result.Ok) {
					// Display saved image
					Bitmap bitmap = POIService.GetImage(_poi.Id);
					_poiImageView.SetImageBitmap(bitmap);

					// Dispose image after upload
					if (bitmap != null) {
						bitmap.Dispose();
						bitmap = null;
					}
				} else {
					// Let the user know the photo was cancelled
					Toast.MakeText(Activity, "No picture captured.", ToastLength.Short).Show();
				}
			} else {
				base.OnActivityResult(requestCode, resultCode, data);
			}
		}

	}
}
