using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Database;
using Android.Graphics;
using Android.Provider;

using Java.IO;

using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace Chores
{
	[Activity (Label = "Taskya", Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
	public class SignupActivity : Activity
	{
		#region PRIVATE VARIABLES
		ProgressDialog _progDlg;
		EditText _txtUserName;
		EditText _txtEmail;
		EditText _txtPassword;
		EditText _txtConfirmPwd;

		public const int PickImageId = 1000;
		public const int PICK_FROM_CROP = 1001;
		private ImageView _imageView;
		String path = null;

        private bool bAddPhoto = false;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			App._file = null;
			App._dir = null;
			App.bitmap = null;

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Signup);

			_txtUserName = FindViewById<EditText> (Resource.Id.txtName);
			_txtEmail = FindViewById<EditText> (Resource.Id.txtEmail);
			_txtPassword = FindViewById<EditText> (Resource.Id.txtPassword);
			_txtConfirmPwd = FindViewById<EditText> (Resource.Id.txtConfirmPassword);

			String userName = Intent.GetStringExtra("userName");
			if (String.IsNullOrEmpty (userName) == false) {
				_txtUserName.Text = userName;
			}

			_imageView = FindViewById<ImageView> (Resource.Id.imgPhoto);

			// Get our button from the layout resource,
			// and attach an event to it
			Button butRegister = FindViewById<Button> (Resource.Id.btnRegister);

			if (butRegister != null) {
				butRegister.Click += delegate {
					if (String.IsNullOrEmpty(_txtUserName.Text) == true)
					{
						Toast.MakeText(this, "Please input user name", ToastLength.Short).Show();
						return;
					}

					if (String.IsNullOrEmpty(_txtEmail.Text) == true)
					{
						Toast.MakeText(this, "Please input email", ToastLength.Short).Show();
						return;
					}

					if (String.IsNullOrEmpty(_txtPassword.Text) == true ||
						String.IsNullOrEmpty(_txtConfirmPwd.Text) == true)
					{
						Toast.MakeText(this, "Please input password", ToastLength.Short).Show();
						return;
					}

					if (_txtPassword.Text.Equals(_txtConfirmPwd.Text) == false)
					{
						Toast.MakeText(this, "Passwords are not the same. Please check again", ToastLength.Short).Show();
						return;
					}

					_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
					_progDlg.CancelEvent += (object sender, EventArgs e) => {
					};

					if (App._file == null)
					{
						WebService.doRegisterUser(this, _txtUserName.Text, _txtEmail.Text, _txtPassword.Text, "");
					}
					else
						WebService.doUploadPhoto(this, App._file.Path);
				};
			}

			Button btnCancel = FindViewById<Button> (Resource.Id.btnCancel);

			if (btnCancel != null) {
				btnCancel.Click += delegate {
					Finish();
					//FinishActivity(-1);
				};
			}

			CreateDirectoryForPictures ();

            RelativeLayout rlTakePHoto = FindViewById<RelativeLayout>(Resource.Id.rlImagePhoto);
            rlTakePHoto.Click += delegate {
				Intent intent = new Intent(MediaStore.ActionImageCapture);
				App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
				intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
				StartActivityForResult(intent, PickImageId);
			};

			Global.HideKeyboard (this, InputMethodService);
		}

		#region PRIVATE METHODS
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (resultCode != Result.Ok)
			{
				return;
			}

			switch (requestCode)
			{
			case PickImageId:
				Intent intent = new Intent(this, typeof(CropImage));
				intent.PutExtra("image-path", App._file.Path);
				intent.PutExtra("scale", true);
				StartActivityForResult(intent, PICK_FROM_CROP);
				break;
			case PICK_FROM_CROP:
				// make it available in the gallery
				Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
				Uri contentUri = Uri.FromFile(App._file);
				mediaScanIntent.SetData(contentUri);
				SendBroadcast(mediaScanIntent);

				// display in ImageView. We will resize the bitmap to fit the display
				// Loading the full sized image will consume to much memory 
				// and cause the application to crash.
				int height = Resources.DisplayMetrics.HeightPixels;
				int width = _imageView.Width ;
				App.bitmap = BitmapHelpers.LoadAndResizeBitmap (App._file.Path, width, height);

				if (App.bitmap != null) {
					_imageView.SetImageBitmap (App.bitmap);
					App.bitmap = null;
                    bAddPhoto = true;
                    FindViewById<TextView>(Resource.Id.txtAddImage).Visibility = ViewStates.Gone;
				}
				break;
			}

			// For single image Selection
			/*if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
			{
				Android.Net.Uri uri = data.Data;
				_imageView.SetImageURI(uri);

				path = GetPathToImage (uri);
			}
			*/
		}

		private string GetPathToImage(Android.Net.Uri uri)
		{
			string path = null;
			// The projection contains the columns we want to return in our query.
			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
			using (ICursor cursor = ManagedQuery(uri, projection, null, null, null))
			{
				if (cursor != null)
				{
					int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
					cursor.MoveToFirst();
					path = cursor.GetString(columnIndex);
				}
			}
			return path;
		}

		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		private void CreateDirectoryForPictures()
		{
			App._dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "CameraAppDemo");
			if (!App._dir.Exists())
			{
				App._dir.Mkdirs();
			}
		}


		#endregion

		#region WEBSERVICE
		public void registerWithResult(int success, String message, int userId)
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
					Toast.MakeText(this, GetString(Resource.String.msg_register_success), ToastLength.Short).Show();

					Intent resultIntent = new Intent(this, typeof(LoginActivity));
					resultIntent.PutExtra("userName", _txtUserName.Text);
					SetResult(Result.Ok, resultIntent);

					Finish();
				}
			}
			catch (Exception e) {
			}
		}

		public void uploadPhotoWithResult(String filePath)
		{
			if (String.IsNullOrEmpty (filePath)) {
				_progDlg.Dismiss ();

				Toast.MakeText (this, "Cannot upload photo", ToastLength.Short).Show ();
			}{
				WebService.doRegisterUser(this, _txtUserName.Text, _txtEmail.Text, _txtPassword.Text, filePath);
			}
		}
		#endregion
	}

	public static class App{
		public static File _file;
		public static File _dir;     
		public static Bitmap bitmap;
	}

	public static class BitmapHelpers
	{
		public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
		{
			// First we get the the dimensions of the file on disk
			BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
			BitmapFactory.DecodeFile(fileName, options);

			// Next we calculate the ratio that we need to resize the image by
			// in order to fit the requested dimensions.
			int outHeight = options.OutHeight;
			int outWidth = options.OutWidth;
			int inSampleSize = 1;

			if (outHeight > height || outWidth > width)
			{
				inSampleSize = outWidth > outHeight
					? outHeight / height
					: outWidth / width;
			}

			// Now we will load the image and have BitmapFactory resize it for us.
			options.InSampleSize = inSampleSize;
			options.InJustDecodeBounds = false;
			Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

			return resizedBitmap;
		}
	}
}


