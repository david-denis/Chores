using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Chores
{
	[Activity (Label = "Taskya", Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
	public class NotifyActivity : Activity
	{
		#region PRIVATE VARIABLES
		ProgressDialog _progDlg;

		List<NotificationInfo> _notifyList = new List<NotificationInfo>();
		NotificationAdapter adapter = null;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Notification);

            LinearLayout butActivity = FindViewById<LinearLayout>(Resource.Id.linearBtnActivity);
            LinearLayout butTask = FindViewById<LinearLayout>(Resource.Id.linearBtnTask);
            LinearLayout butGroup = FindViewById<LinearLayout>(Resource.Id.linearBtnGroup);
            LinearLayout butHome = FindViewById<LinearLayout>(Resource.Id.linearBtnHome);

			butActivity.Click += delegate {
				Intent ActivityIntent = new Intent(this, typeof(ActivityActivity));
				StartActivity(ActivityIntent);
				Finish();
			};

			butTask.Click += delegate {
				Intent TaskIntent = new Intent(this, typeof(TaskActivity));
				StartActivity(TaskIntent);
				Finish();
			};

			butGroup.Click += delegate {
				Intent GroupIntent = new Intent(this, typeof(GroupActivity));
				StartActivity(GroupIntent);
				Finish();
			};

			butHome.Click += delegate {
				Intent HomeIntent = new Intent(this, typeof(HomeActivity));
				StartActivity(HomeIntent);
				Finish();
			};

			Button btnClear = FindViewById<Button> (Resource.Id.btnClear);
			btnClear.Click += delegate {
				_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				_progDlg.CancelEvent += (object sender, EventArgs e) => {
				};

				WebService.doClearNotification(this, Global.userId);
			};

			updateUserInfo();
			runBackground ();
		}

		#region PRIVATE METHODS
		private void runBackground()
		{
			_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			_progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetNotifications(this, Global.userId);
		}

		private void updateUserInfo()
		{
            /*
			TextView txtUserName = FindViewById<TextView> (Resource.Id.lblName);
			TextView txtUserPoints = FindViewById<TextView> (Resource.Id.lblPoints);
			ImageView imgUserPhoto = FindViewById<ImageView> (Resource.Id.imgUser);
			txtUserName.Text = Global.userName;
			txtUserPoints.Text = "Points: " + Global.userPoint.ToString();
			Global.GetImageFromUrl (this, imgUserPhoto, Global.userPhoto);
             */
		}
		#endregion

		#region WEBSERVICE
		public void getNotificationResult(List<NotificationInfo> notifyList)
		{
			try{
				_progDlg.Dismiss();

				updateUserInfo();

				_notifyList = notifyList;

				ListView listNotification = FindViewById<ListView>(Resource.Id.listNotifications);
				adapter = new NotificationAdapter(this, _notifyList);

				listNotification.Adapter = adapter;
			}
			catch (Exception e) {
			}
		}

		public void clearNotificationResult()
		{
			try{
				_progDlg.Dismiss();

				_notifyList.Clear();
				ListView listNotification = FindViewById<ListView>(Resource.Id.listNotifications);
				adapter = new NotificationAdapter(this, _notifyList);

				listNotification.Adapter = adapter;
			}
			catch (Exception e) {
			}
		}
		#endregion
	}
}


