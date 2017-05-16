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

namespace POIntillismeApp
{
	public class DeleteDialogFragment : DialogFragment
	{
		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			POIDetailFragment targetFragment = (POIDetailFragment)TargetFragment;

			string poiName = Arguments.GetString("name");

			AlertDialog.Builder alertConfirm = new AlertDialog.Builder(this.Activity);
			alertConfirm.SetTitle("Confirm delete");
			alertConfirm.SetCancelable(false);
			alertConfirm.SetPositiveButton("OK", (sender, e) => {
				targetFragment.DeletePOIAsync();
			});
			alertConfirm.SetNegativeButton("Cancel", delegate {});
			alertConfirm.SetMessage(String.Format("Are you sure you want to delete {0}?", poiName));
			return alertConfirm.Create ();
		}
	}
}
