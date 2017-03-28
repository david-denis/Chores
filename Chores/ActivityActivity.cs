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
    [Activity(Label = "Taskya", Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class ActivityActivity : Activity
	{
		#region PRIVATE VARIABLES
		ProgressDialog _progDlg;
		List<GroupInfo> _groupList =  new List<GroupInfo>();

		List<ActivityInfo> _activityList = new List<ActivityInfo> ();
		List<ActivityInfo> _activityDetailList = new List<ActivityInfo> ();

		ActivityItemAdapter adapter = null;
		ActivityItemAdapter adapterDetail = null;

		int _selGroupId = -1;
		int _selUserId = -1;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Activity);

            LinearLayout linearbutHome = FindViewById<LinearLayout>(Resource.Id.linearBtnHome);
            LinearLayout linearbutTask = FindViewById<LinearLayout>(Resource.Id.linearBtnTask);
            LinearLayout linearbutGroup = FindViewById<LinearLayout>(Resource.Id.linearBtnGroup);
            LinearLayout linearbutNotification = FindViewById<LinearLayout>(Resource.Id.linearBtnNotify);

            linearbutHome.Click += delegate
            {
				Intent HomeIntent = new Intent(this, typeof(HomeActivity));
				StartActivity(HomeIntent);
				Finish();
			};

            linearbutTask.Click += delegate
            {
				Intent TaskIntent = new Intent(this, typeof(TaskActivity));
				StartActivity(TaskIntent);
				Finish();
			};

            linearbutGroup.Click += delegate
            {
				Intent GroupIntent = new Intent(this, typeof(GroupActivity));
				StartActivity(GroupIntent);
				Finish();
			};

            linearbutNotification.Click += delegate
            {
				Intent NotifyIntent = new Intent(this, typeof(NotifyActivity));
				StartActivity(NotifyIntent);
				Finish();
			};

			updateUserInfo ();
			runBackground ();
		}

		#region PRIVATE METHODS
		private void runBackground()
		{
			_selGroupId = -1;
			_selUserId = -1;

			_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			_progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetGroups(this, Global.userId, "", WebService.GET_GROUPS_ACTIVITY, 0);
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

		private void changeGroup(int groupId)
		{
			_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			_progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetActivities(this, groupId);
		}
		#endregion

		#region WEBSERVICE
		public void userGroupsWithResult(int success, String message, List<GroupInfo> groupArray)
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
					updateUserInfo();

					_groupList = groupArray;

					Spinner spinnerGroup = FindViewById<Spinner>(Resource.Id.spinGroup);
					if (_groupList.Count > 0)
					{
						FindViewById<LinearLayout>(Resource.Id.linearGroup).Visibility = ViewStates.Visible;

						String[] _groupNameArr = new String[groupArray.Count];
						for (int i = 0; i < _groupList.Count; i++)
							_groupNameArr[i] = _groupList[i].groupName;

						ArrayAdapter<String> adapter_group = new ArrayAdapter<String>(this, Resource.Layout.SpinnerText/*Android.Resource.Layout.SimpleSpinnerItem*/, _groupNameArr);
						adapter_group.SetDropDownViewResource(Resource.Layout.SpinnerText);
						spinnerGroup.Adapter = adapter_group;

						spinnerGroup.ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
							if (e.Position != -1)// && _selGroupId != e.Position)
							{
								int index = e.Position;
								_selGroupId = index;
								changeGroup(_groupList[index].groupId);
							}
						};
					}
					else
					{
						FindViewById<LinearLayout>(Resource.Id.linearGroup).Visibility = ViewStates.Invisible;
					}
				}
			}
			catch (Exception e) {
			}
		}

		public void getActivitiesResult(List<ActivityInfo> activityList)
		{
			try{
				_progDlg.Dismiss();

				_activityList = activityList;

				_selUserId = -1;

				_activityDetailList.Clear();

				// Show members on the UI
				adapterDetail = new ActivityItemAdapter(this, _activityDetailList);
				FindViewById<ListView>(Resource.Id.listDetailTasks).Adapter = adapterDetail;

				// Show members on the UI
				adapter = new ActivityItemAdapter(this, _activityList);
				FindViewById<ListView>(Resource.Id.listTasks).Adapter = adapter;
				FindViewById<ListView>(Resource.Id.listTasks).ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
					_selUserId = e.Position;

					_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
					_progDlg.CancelEvent += (object sender1, EventArgs e1) => {
					};

					WebService.doGetHistory(this, _activityList[e.Position].userId);
				};
			}
			catch (Exception e) {
			}
		}

		public void getHistoryResult(List<ActivityInfo> activityList)
		{
			try{
				_progDlg.Dismiss();

				_activityDetailList = activityList;

				// Show members on the UI
				adapterDetail = new ActivityItemAdapter(this, _activityDetailList);
				FindViewById<ListView>(Resource.Id.listDetailTasks).Adapter = adapterDetail;
				FindViewById<ListView>(Resource.Id.listDetailTasks).ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				};
			}
			catch (Exception e) {
			}
		}
		#endregion
	}
}


