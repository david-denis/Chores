using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Uri = Android.Net.Uri;
using Android.Graphics;

namespace Chores
{
	[Activity (Label = "Taskya", Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
	public class HomeActivity : Activity
	{
		#region CONSTANTS
		const int ACTIVITY_FOR_CHORESSCREEN = 0;
		#endregion

		#region PRIVATE VARIABLES
		ProgressDialog _progDlg;
		List<TaskInfo> _taskList =  new List<TaskInfo>();

		HomeItemAdapter adapter = null;

		int _selTaskId = -1;
		#endregion

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Home);

			try
			{
                LinearLayout butActivity = FindViewById<LinearLayout>(Resource.Id.linearBtnActivity);
                LinearLayout butTask = FindViewById<LinearLayout>(Resource.Id.linearBtnTask);
                LinearLayout butGroup = FindViewById<LinearLayout>(Resource.Id.linearBtnGroup);
                LinearLayout butNotification = FindViewById<LinearLayout>(Resource.Id.linearBtnNotify);

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

				butNotification.Click += delegate {
					Intent NotifyIntent = new Intent(this, typeof(NotifyActivity));
					StartActivity(NotifyIntent);
					Finish();
				};

                ImageView btnSearchChores = FindViewById<ImageView>(Resource.Id.imgSearch);
				btnSearchChores.Click += delegate {
					Intent ChoresIntent = new Intent(this, typeof(ChoresActivity));
					StartActivityForResult(ChoresIntent, ACTIVITY_FOR_CHORESSCREEN);
				};

				Button btnComplete = FindViewById<Button>(Resource.Id.btnComplete);
				btnComplete.Click += delegate {
					_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
					_progDlg.CancelEvent += (object sender, EventArgs e) => {
					};

					WebService.changeTaskState(this, -1, _taskList[_selTaskId].taskId, 2, 1);
				};

				updateUserInfo();
				runBackground();
				changeButtonState(false);
			}
			catch (Exception e) {
			}

		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == ACTIVITY_FOR_CHORESSCREEN && resultCode == Result.Ok)
			{
				runBackground ();
			}
		}

		#region PRIVATE METHODS
		private void runBackground()
		{
			changeButtonState(false);
			_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			_progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetTasks(this, Global.userId, -1, 2);
		}

		private void updateUserInfo()
		{
			TextView txtUserName = FindViewById<TextView> (Resource.Id.lblName);
			TextView txtUserPoints = FindViewById<TextView> (Resource.Id.lblPoints);
			ImageView imgUserPhoto = FindViewById<ImageView> (Resource.Id.imgUser);
			txtUserName.Text = Global.userName;
			txtUserPoints.Text = "Points: " + Global.userPoint.ToString();
			Global.GetImageFromUrl (this, imgUserPhoto, Global.userPhoto);
		}

		private void changeButtonState(bool bActive)
		{
			FindViewById<Button> (Resource.Id.btnComplete).Enabled = bActive;
		}


		#endregion

		#region WEBSERVICE
		public void getPostedTasksResult(int success, String message, List<TaskInfo> taskList)
		{
			try{
				_progDlg.Dismiss();

				_selTaskId = -1;

				if (success == 0)
				{
					if (message != null)
						Toast.MakeText(this, message, ToastLength.Short).Show();
					else
						Toast.MakeText(this, GetString(Resource.String.msg_server_error), ToastLength.Short).Show();
				}
				else
				{
					updateUserInfo();

					_taskList = taskList;

					ListView listTask = FindViewById<ListView>(Resource.Id.listTasks);
					adapter = new HomeItemAdapter(this, _taskList);

					listTask.Adapter = adapter;

					listTask.ItemLongClick += delegate(object sender, AdapterView.ItemLongClickEventArgs e) {
						if (e.Position != -1 && _selTaskId != e.Position)
						{
							Intent TaskDetailIntent = new Intent(this, typeof(TaskDetailActivity));

							if (_selTaskId != -1)
							{
								if (_selTaskId % 2 == 0) {
									listTask.GetChildAt(_selTaskId).FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.White);
								} else {
									listTask.GetChildAt(_selTaskId).FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.LightGray);
								}
							}

							_selTaskId = e.Position;

							TaskInfo _info = _taskList[_selTaskId];
							listTask.GetChildAt(e.Position).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.Rgb(0xee, 0x93, 0x2b));

							String finishDate = "";
							if (_info.priority == 0)
								finishDate = "";
							else if (_info.priority == 1)
								finishDate = _info.finishdate.ToString("yyyy-MM-dd");
							else
								finishDate = _info.finishdate.ToString("yyyy-MM-dd HH:mm");

							TaskDetailIntent.PutExtra("taskId", _info.taskId);
							TaskDetailIntent.PutExtra("taskName", _info.taskName);
							TaskDetailIntent.PutExtra("taskDescription", _info.description);
							TaskDetailIntent.PutExtra("groupName", _info.groupName);
							TaskDetailIntent.PutExtra("userName", _info.userName);
							TaskDetailIntent.PutExtra("postDate", _info.postdate.ToString("yyyy-MM-dd"));
							TaskDetailIntent.PutExtra("finishDate", finishDate);
							TaskDetailIntent.PutExtra("points", _info.points);
							TaskDetailIntent.PutExtra("priority", _info.priority);
							TaskDetailIntent.PutExtra("status", _info.status);
							TaskDetailIntent.PutExtra("postusername", _info.postName);

							StartActivity(TaskDetailIntent);

							if (_info.userstatus == 1)
								changeButtonState(true);
							else
								changeButtonState(false);
						}
					};

					listTask.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
						if (e.Position != -1 && _selTaskId != e.Position)
						{
							if (_selTaskId != -1)
							{
								if (_selTaskId % 2 == 0) {
									listTask.GetChildAt(_selTaskId).FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.White);
								} else {
									listTask.GetChildAt(_selTaskId).FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.LightGray);
								}
							}

							_selTaskId = e.Position;
							listTask.GetChildAt(e.Position).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.Rgb(0xee, 0x93, 0x2b));

							TaskInfo _info = _taskList[_selTaskId];
							if (_info.userstatus == 1)
								changeButtonState(true);
							else
								changeButtonState(false);
						}
					};
				}
			}
			catch (Exception e) {
			}
		}

		public void changeTaskResult(int success, String message)
		{
			_progDlg.Dismiss();

			if (success == 0) {
				if (message != null)
					Toast.MakeText (this, message, ToastLength.Short).Show ();
				else
					Toast.MakeText (this, GetString (Resource.String.msg_server_error), ToastLength.Short).Show ();
			} else {
				Toast.MakeText (this, "Success", ToastLength.Short).Show ();

				runBackground ();
			}
		}
		#endregion
	}
}


