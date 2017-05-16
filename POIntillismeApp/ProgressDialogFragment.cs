using System;
using Android.App;

namespace POIntillismeApp
{
	public class ProgressDialogFragment : DialogFragment
	{
		public override Dialog OnCreateDialog (Android.OS.Bundle savedInstanceState)
		{
			Cancelable = false;
			ProgressDialog _progressDialog = new ProgressDialog(Activity);
			_progressDialog.SetMessage("Getting location...");
			_progressDialog.Indeterminate = true;
			_progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
			return _progressDialog;
		}
	}
}
