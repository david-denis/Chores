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
	public class TaskDetailActivity : Activity
	{
		#region PRIVATE VARIABLES
		ProgressDialog _progDlg;

		List<UserInfo> _userBidList = new List<UserInfo>();
		TaskBidUserItemAdapter _adapter = null;

		int _listItemId = -1;

		int _taskId = -1;
		int _flagTask = 0;
		int _status = 0;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.TaskDetail);

			_flagTask = Intent.GetIntExtra ("fromTask", 0);

			Button butActionJob = FindViewById<Button> (Resource.Id.btnAction);
			butActionJob.Click += delegate {
				if (_flagTask == 1)
				{
					if (_status == 0)
					{
						_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
						_progDlg.CancelEvent += (object sender, EventArgs e) => {
						};

						WebService.doStartTask(this, _taskId, _userBidList[_listItemId].userId);
					}
					else if (_status == 1 || _status == 2)
					{
						_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
						_progDlg.CancelEvent += (object sender, EventArgs e) => {
						};

						WebService.doEndTask(this, _taskId);
					}
				}
			};

			Button butClose = FindViewById<Button> (Resource.Id.btnCancel);
			butClose.Click += delegate {
				if (_flagTask == 1 && _status == 2)
				{
					_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
					_progDlg.CancelEvent += (object sender, EventArgs e) => {
					};

					WebService.changeTaskState(this, -1, _taskId, 1, 0);
				}
				else
				{
					SetResult(Result.Canceled);
					Finish();
				}
			};

			_taskId = Intent.GetIntExtra("taskId", -1);

			TextView txtName = FindViewById<TextView> (Resource.Id.txtTaskName);
			txtName.Text = Intent.GetStringExtra ("taskName");

			TextView txtDescription = FindViewById<TextView> (Resource.Id.txtTaskDescription);
			txtDescription.Text = Intent.GetStringExtra ("taskDescription");

			TextView txtGroupName = FindViewById<TextView> (Resource.Id.txtGroupName);
			txtGroupName.Text = Intent.GetStringExtra ("groupName");

			TextView txtUserName = FindViewById<TextView> (Resource.Id.txtUserName);
			txtUserName.Text = Intent.GetStringExtra ("userName");

			TextView txtPoints = FindViewById<TextView> (Resource.Id.txtPoints);
			txtPoints.Text = Intent.GetIntExtra ("points", 0).ToString();

			TextView txtPriority = FindViewById<TextView> (Resource.Id.txtPriority);
			txtPriority.Text = getPriorityString(Intent.GetIntExtra ("priority", 0));

			TextView txtPostDate = FindViewById<TextView> (Resource.Id.txtPostDate);
			txtPostDate.Text = Intent.GetStringExtra("postDate");

			TextView txtFinishDate = FindViewById<TextView> (Resource.Id.txtFinishDate);
			txtFinishDate.Text = Intent.GetStringExtra ("finishDate");

			_status = Intent.GetIntExtra ("status", 0);
			TextView txtStatus = FindViewById<TextView> (Resource.Id.txtTaskStatus);
			txtStatus.Text = "(" + Global.getTaskStatus(_status) + ")";

			bool bButtonEnabled = false;
			if (_flagTask == 1) {
				FindViewById<TextView> (Resource.Id.txtTaskTitle).Text = "Task Detail";

				if (_status == 0)
					runBackground ();
				else if (_status == 1) {
					butActionJob.Text = "Finish";
					bButtonEnabled = true;
				} else if (_status == 2) {
					butActionJob.Text = "Finish";
					butClose.Text = "Reset";
					bButtonEnabled = true;
				}
			}
			else
				FindViewById<TextView> (Resource.Id.txtTaskTitle).Text = "Post User: " + Intent.GetStringExtra("postusername");

			changeButtonState (bButtonEnabled);
		}

		#region PRIVATE METHODS
		private String getPriorityString(int priority)
		{
			if (priority == 0)
				return "Low";
			else if (priority == 1)
				return "Medium";
			else
				return "High";
		}

		private void runBackground()
		{
			_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			_progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetBidUsers(this, _taskId);
		}

		private void changeButtonState(bool bActive)
		{
			FindViewById<Button> (Resource.Id.btnAction).Enabled = bActive;
		}

		#endregion

		#region WEBSERVICE
		public void getBidUsersResult(List<UserInfo> bidUserList)
		{
			_progDlg.Dismiss();

			_listItemId = -1;

			_userBidList = bidUserList;

			ListView listBidUsers = FindViewById<ListView>(Resource.Id.listBidUsers);
			_adapter = new TaskBidUserItemAdapter(this, _userBidList);

			listBidUsers.Adapter = _adapter;

			listBidUsers.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				if (e.Position != -1)
				{
					_listItemId = e.Position;
					changeButtonState(true);
				}
			};
		}

		public void startTaskResult(int success, String message)
		{
			_progDlg.Dismiss();

			if (success == 0) {
				if (message != null)
					Toast.MakeText (this, message, ToastLength.Short).Show ();
				else
					Toast.MakeText (this, GetString (Resource.String.msg_server_error), ToastLength.Short).Show ();
			} else {
				Toast.MakeText (this, "Success", ToastLength.Short).Show ();
				SetResult (Result.Ok);
				Finish ();
			}
		}

		public void endTaskResult(int success, String message)
		{
			_progDlg.Dismiss();

			if (success == 0) {
				if (message != null)
					Toast.MakeText (this, message, ToastLength.Short).Show ();
				else
					Toast.MakeText (this, GetString (Resource.String.msg_server_error), ToastLength.Short).Show ();
			} else {
				Toast.MakeText (this, "Success", ToastLength.Short).Show ();
				SetResult (Result.Ok);
				Finish ();
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
				SetResult (Result.Ok);
				Finish ();
			}
		}
		#endregion
	}
}


