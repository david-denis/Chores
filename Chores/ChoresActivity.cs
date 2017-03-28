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
	public class ChoresActivity : Activity
	{
		#region PRIVATE METHODS
		ProgressDialog progDlg;

		List<GroupInfo> _groupList = new List<GroupInfo> ();
		List<TaskInfo> _taskList = new List<TaskInfo>();

		ChoresItemAdapter _adapter = null;
		int groupId = -1;
		int taskId = -1;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Chores);

			Button btnAcceptJob = FindViewById<Button> (Resource.Id.btnAccept);
			btnAcceptJob.Click += delegate {
				if (taskId != -1)
				{
					progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
					progDlg.CancelEvent += (object sender, EventArgs e) => {
					};

					WebService.doAcceptTask(this, Global.userId, _taskList[taskId].taskId);
				}
			};

            ImageView imgBack = FindViewById<ImageView>(Resource.Id.imgBack);
            imgBack.Click += delegate
            {
				SetResult(Result.Canceled);
				Finish();
			};

			changeUIState (false);
			changeButtonActive (false);
			runBackground ();
		}

		#region PRIVATE METHODS
		private void runBackground()
		{
			groupId = -1;

			progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetGroups(this, Global.userId, "", WebService.GET_GROUPS_CHORES, 1);

		}

		private void changeUIState(bool bGroup)
		{
			LinearLayout _linearGroup = FindViewById<LinearLayout> (Resource.Id.linearGroup);
			ListView _groupList = FindViewById<ListView> (Resource.Id.listTasks);

			_linearGroup.Visibility = (bGroup) ? ViewStates.Visible : ViewStates.Invisible;
			_groupList.Visibility = (bGroup) ? ViewStates.Visible : ViewStates.Invisible;
		}

		private void changeButtonActive(bool bActive)
		{
			Button _btnAccept = FindViewById<Button> (Resource.Id.btnAccept);
			_btnAccept.Enabled = bActive;
		}

		private void changeGroup(int groupId)
		{
			progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			taskId = -1;
			WebService.doGetTasks(this, Global.userId, groupId, 0);
		}
		#endregion

		#region WEBSERVICE
		public void userGroupsWithResult(List<GroupInfo> groupList)
		{
			try{
				progDlg.Dismiss();

				_groupList = groupList;

				groupId = 0;
				Spinner spinnerGroup = FindViewById<Spinner>(Resource.Id.spinGroup);
				String[] _groupNameArr = new String[groupList.Count];
				for (int i = 0; i < _groupList.Count; i++)
					_groupNameArr[i] = _groupList[i].groupName;

				ArrayAdapter<String> adapter_group = new ArrayAdapter<String>(this, /*Resource.Layout.SpinnerText*/Android.Resource.Layout.SimpleSpinnerItem, _groupNameArr);
				adapter_group.SetDropDownViewResource(Resource.Layout.SpinnerText);
				spinnerGroup.Adapter = adapter_group;

				spinnerGroup.ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
					if (e.Position != -1 && groupId != e.Position)
					{
						int index = e.Position;
						groupId = index;

						if (groupId != 0)
							changeGroup(_groupList[index].groupId);
					}
				};

				if (_groupList.Count > 0)
				{
					changeUIState (true);
					changeGroup(_groupList[0].groupId);
				}
			}
			catch (Exception e) {
			}
		}

		public void getPostedTasksResult(int success, String message, List<TaskInfo> taskList)
		{
			try{
				progDlg.Dismiss();

				if (success == 0)
				{
					if (message != null)
						Toast.MakeText(this, message, ToastLength.Short).Show();
					else
						Toast.MakeText(this, GetString(Resource.String.msg_server_error), ToastLength.Short).Show();
				}
				else
				{
					_taskList = taskList;

					ListView listTask = FindViewById<ListView>(Resource.Id.listTasks);
					_adapter = new ChoresItemAdapter(this, _taskList);

					listTask.Adapter = _adapter;

					listTask.ItemLongClick += delegate(object sender, AdapterView.ItemLongClickEventArgs e) {
						if (e.Position != -1)
						{
							Intent TaskDetailIntent = new Intent(this, typeof(TaskDetailActivity));

							TaskInfo _info = _taskList[e.Position];

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

							changeButtonActive(true);

							if (taskId != -1)
							{
								if (taskId % 2 == 0)
									listTask.GetChildAt(taskId).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.White);
								else
									listTask.GetChildAt(taskId).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.LightGray);
							}

							taskId = e.Position;
							listTask.GetChildAt(e.Position).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.Rgb(0xee, 0x93, 0x2b));
						}
					};

					listTask.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
						if (e.Position != -1)
						{
							if (taskId != -1)
							{
								if (taskId % 2 == 0)
									listTask.GetChildAt(taskId).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.White);
								else
									listTask.GetChildAt(taskId).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.LightGray);
							}

							taskId = e.Position;
							listTask.GetChildAt(e.Position).FindViewById<RelativeLayout>(Resource.Id.relativeItemRoot).SetBackgroundColor(Android.Graphics.Color.Rgb(0xee, 0x93, 0x2b));

							changeButtonActive(true);
						}
					};
				}
			}
			catch (Exception e) {
			}
		}

		public void acceptTaskResult(int success, String message)
		{
			try{
				progDlg.Dismiss();

				if (success == 0)
				{
					if (message != null)
						Toast.MakeText(this, message, ToastLength.Short).Show();
					else
						Toast.MakeText(this, GetString(Resource.String.msg_server_error), ToastLength.Short).Show();
				}
				else
				{
					Toast.MakeText(this, "Success", ToastLength.Short).Show();
					SetResult(Result.Ok);
					Finish();
				}
			}
			catch (Exception e) {
			}
		}
		#endregion
	}
}


