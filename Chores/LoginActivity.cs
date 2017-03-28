using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Preferences;

namespace Chores
{
	[Activity(Label = "Taskya", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class LoginActivity : Activity
	{
		#region CONSTANTS
		const int ACTIVITY_FOR_SIGNUP = 0;
		#endregion

		#region PRIVATE VARIABLES
		ProgressDialog _progDlg;
		EditText _txtUserName;
		EditText _txtPassword;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Login);

			_txtUserName = FindViewById<EditText> (Resource.Id.txtName);
			_txtPassword = FindViewById<EditText> (Resource.Id.txtPassword);

			// Get our button from the layout resource,
			// and attach an event to it
			Button butSignup = FindViewById<Button> (Resource.Id.btnSignup);

			if (butSignup != null) {
				butSignup.Click += delegate {
					Intent signupIntent = new Intent(this, typeof(SignupActivity));
					signupIntent.PutExtra("userName", _txtUserName.Text);
					StartActivityForResult(signupIntent, ACTIVITY_FOR_SIGNUP);
				};
			}

			Button butLogin = FindViewById<Button> (Resource.Id.btnLogin);

			if (butLogin != null) {
				butLogin.Click += delegate {
					if (String.IsNullOrEmpty(_txtUserName.Text) == true)
					{
						Toast.MakeText(this, "Please input user name", ToastLength.Short).Show();
						return;
					}

					if (String.IsNullOrEmpty(_txtPassword.Text) == true)
					{
						Toast.MakeText(this, "Please input password", ToastLength.Short).Show();
						return;
					}

					_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
					_progDlg.CancelEvent += (object sender, EventArgs e) => {
					};

					WebService.doLogin(this, _txtUserName.Text, _txtPassword.Text);
				};
			}

			getUserNameFromConfig ();
			Global.HideKeyboard (this, InputMethodService);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == ACTIVITY_FOR_SIGNUP)
			{
				if (data != null) {
					String userName = data.GetStringExtra ("userName");
					_txtUserName.Text = userName;
				}
			}
		}

		#region PRIVATE METHODS
		private void getUserNameFromConfig()
		{
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
			String userName = prefs.GetString ("chores_username", "");

			if (String.IsNullOrEmpty(userName) == false)
				FindViewById<EditText> (Resource.Id.txtName).Text = userName;
		}

		private void setUserNameToConfig()
		{
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
			ISharedPreferencesEditor editor = prefs.Edit ();
			editor.PutString ("chores_username", FindViewById<EditText> (Resource.Id.txtName).Text);
			editor.Apply ();
		}
		#endregion

		#region WEBSERVICE
		public void loginWithResult(int success, String message, int userId)
		{ 
			try{
				_progDlg.Dismiss();

				if (success == 0)
				{
					if (message != null)
						Toast.MakeText(this, message, ToastLength.Short).Show();
					else
						Toast.MakeText(this, GetString(Resource.String.msg_server_error), ToastLength.Short).Show();
				}
				else
				{
					setUserNameToConfig();

					Global.userId = userId;
					Intent homeIntent = new Intent(this, typeof(HomeActivity));
					StartActivity(homeIntent);
				}
			}
			catch (Exception e) {
			}
		}
		#endregion 
	}
}


