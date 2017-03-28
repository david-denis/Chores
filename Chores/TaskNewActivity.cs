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
	[Activity (Label = "Taskya", Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar", WindowSoftInputMode = Android.Views.SoftInput.AdjustPan, ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
	public class TaskNewActivity : Activity
	{
		#region CONSTANTS
		const int DATE_DIALOG_ID = 0;
		const int TIME_DIALOG_ID = 1;
		#endregion

		#region PRIVATE METHODS
		ProgressDialog progDlg;

		List<GroupInfo> _groupList = new List<GroupInfo> ();
		List<GroupMemberInfo> _userList = new List<GroupMemberInfo> ();

		String dateStr = "";
		String timeStr = "";

		int groupId = -1;
		int userId = -1;

		int priority = 0;

		private int _hour;
		private int _minute;
		private DateTime _date;

		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.TaskNew);

			_date = DateTime.Today;
			_hour = DateTime.Now.Hour;
			_minute = DateTime.Now.Minute;

            ImageView butPost = FindViewById<ImageView>(Resource.Id.imgPost);
			butPost.Click += delegate {
				// do job post
				if (groupId == 0)
				{
					Toast.MakeText(this, "Please select group first", ToastLength.Short).Show();
					return;
				}

				EditText txtJobTitle = FindViewById<EditText>(Resource.Id.txtJobName);
				if (String.IsNullOrEmpty(txtJobTitle.Text))
				{
					Toast.MakeText(this, "Please input task name", ToastLength.Short).Show();
					return;
				}

				if (priority == 1 && String.IsNullOrEmpty(dateStr))
				{
					Toast.MakeText(this, "Please select date first", ToastLength.Short).Show();
					return;
				}

				if (priority == 2 && (String.IsNullOrEmpty(dateStr) || String.IsNullOrEmpty(timeStr)))
				{
					Toast.MakeText(this, "Please select datetime first", ToastLength.Short).Show();
					return;
				}

				int points = int.Parse(FindViewById<EditText>(Resource.Id.txtJobPoint).Text);
				if (points == 0)
				{
					Toast.MakeText(this, "Please input points", ToastLength.Short).Show();
					return;
				}

				String jobName = txtJobTitle.Text;
				String jobDesc = FindViewById<EditText>(Resource.Id.txtJobDescription).Text;
				String finishDate = "";
				String postDate = "";

				int pgroupId = (groupId == 0) ? -1 : _groupList[groupId - 1].groupId;
				int puserId = (userId == 0) ? -1 : _userList[userId - 1].memberId;

				if (puserId == Global.userId)
				{
					Toast.MakeText(this, "You can't select your id", ToastLength.Short).Show();
					return;
				}

				postDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				if (priority == 0)
					finishDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
				else if (priority == 1)
					finishDate = _date.ToString("yyyy-MM-dd");
				else
				{
					finishDate = _date.ToString("yyyy-MM-dd") + " " + String.Format("{0}:{1}", _hour, _minute);
				}

				progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				progDlg.CancelEvent += (object sender, EventArgs e) => {
				};

				WebService.doAddTask(this, Global.userId, jobName, jobDesc, points, priority, pgroupId, puserId, finishDate, postDate, "");
			};

            ImageView imgCancel = FindViewById<ImageView>(Resource.Id.imgCancel);
            imgCancel.Click += delegate
            {
				SetResult(Result.Canceled);
				Finish();
			};

			Button butPriorityLow = FindViewById<Button> (Resource.Id.btnPriorityLow);
			butPriorityLow.Click += delegate {
				changePriority(0);
			};

			Button butPriorityMedium = FindViewById<Button> (Resource.Id.btnPriorityMedium);
			butPriorityMedium.Click += delegate {
				changePriority(1);
			};

			Button butPriorityHigh = FindViewById<Button> (Resource.Id.btnPriorityHigh);
			butPriorityHigh.Click += delegate {
				changePriority(2);
			};

			Button butSelectTime = FindViewById<Button> (Resource.Id.btnSelectTime);
			butSelectTime.Click += delegate {
				ShowDialog(1);
			};

			Button butSelectDate = FindViewById<Button> (Resource.Id.btnSelectDate);
			butSelectDate.Click += delegate {
				ShowDialog(0);
			};

			changePriority (0);

			runBackground ();
		}

		protected override Dialog OnCreateDialog (int id)
		{
			if (id == DATE_DIALOG_ID) { // date
				return new DatePickerDialog (this, DatePickerCallback, _date.Year,
					_date.Month - 1, _date.Day);
			} else if (id == TIME_DIALOG_ID) {
				return new TimePickerDialog (this, TimePickerCallback, _hour, _minute, false);
			}

			return null;
		}

		private void TimePickerCallback (object sender, TimePickerDialog.TimeSetEventArgs e)
		{
			_hour = e.HourOfDay;
			_minute = e.Minute;
			timeStr = getAPMString (_hour, _minute);
			(FindViewById<TextView>(Resource.Id.txtJobTime)).Text = timeStr;
		}

		private void DatePickerCallback (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			_date = e.Date;
			dateStr = _date.ToString ("d");
			(FindViewById<TextView>(Resource.Id.txtJobDate)).Text = dateStr;
		}

		#region PRIVATE METHODS
		private void runBackground()
		{
			groupId = -1;
			userId = -1;

			progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetGroups(this, Global.userId, "", WebService.GET_GROUPS_TASKNEW, 1);
		}

		private void changePriority(int priority)
		{
			this.priority = priority;

			Button butPriorityLow = FindViewById<Button> (Resource.Id.btnPriorityLow);
			Button butPriorityMedium = FindViewById<Button> (Resource.Id.btnPriorityMedium);
			Button butPriorityHigh = FindViewById<Button> (Resource.Id.btnPriorityHigh);

			LinearLayout linearTime = FindViewById<LinearLayout> (Resource.Id.linearTaskTime);
			LinearLayout linearDate = FindViewById<LinearLayout> (Resource.Id.linearTaskDate);

			butPriorityLow.SetBackgroundColor (Android.Graphics.Color.ParseColor("#CCCCCC"));
            butPriorityMedium.SetBackgroundColor(Android.Graphics.Color.ParseColor("#8F8F8F"));
            butPriorityHigh.SetBackgroundColor(Android.Graphics.Color.ParseColor("#CCCCCC"));

			switch (priority) {
			case 0:
				butPriorityLow.SetBackgroundColor (Android.Graphics.Color.Red);
				linearTime.Visibility = ViewStates.Invisible;
				linearDate.Visibility = ViewStates.Invisible;
				break;
			case 1:
                butPriorityMedium.SetBackgroundColor(Android.Graphics.Color.Red);
				linearTime.Visibility = ViewStates.Invisible;
				linearDate.Visibility = ViewStates.Visible;
				break;
			case 2:
                butPriorityHigh.SetBackgroundColor(Android.Graphics.Color.Red);
				linearTime.Visibility = ViewStates.Visible;
				linearDate.Visibility = ViewStates.Visible;
				break;
			}
		}

		private String getAPMString(int hourofDay, int minute)
		{
			String footerStr = "AM";
			if (hourofDay < 12)
				footerStr = "AM";
			else {
				footerStr = "PM";
				hourofDay -= 12;
			}

			if (hourofDay == 0)
				hourofDay = 12;

			return string.Format ("{0}:{1} {2}", hourofDay, minute.ToString ().PadLeft (2, '0'), footerStr);
		}

		private void addEmptyUsertoList()
		{
			GroupMemberInfo _noUserInfo = new GroupMemberInfo();
			_noUserInfo.memberName = "No User";

			userId = 0;
			Spinner spinnerUser = FindViewById<Spinner>(Resource.Id.spinnerUser);
			String[] _userNameArr = new String[1];
			_userNameArr[0] = "No User";

			ArrayAdapter<String> adapter_group = new ArrayAdapter<String>(this, Resource.Layout.SpinnerText, _userNameArr);
			adapter_group.SetDropDownViewResource(Resource.Layout.SpinnerText);
			spinnerUser.Adapter = adapter_group;
		}

		private void changeGroup(int groupId)
		{
			progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetGroupMembers(this, groupId, WebService.GET_GROUPMEMBERS_TASKNEW);
		}
		#endregion

		#region WEBSERVICE
		public void userGroupsWithResult(List<GroupInfo> groupArray)
		{ 
			try{
				progDlg.Dismiss();

				_groupList = groupArray;

				groupId = 0;
				Spinner spinnerGroup = FindViewById<Spinner>(Resource.Id.spinnerGroup);
				String[] _groupNameArr = new String[groupArray.Count + 1];
				_groupNameArr[0] = "No Group";
				for (int i = 1; i <= _groupList.Count; i++)
					_groupNameArr[i] = _groupList[i - 1].groupName;

				ArrayAdapter<String> adapter_group = new ArrayAdapter<String>(this, Resource.Layout.SpinnerText, _groupNameArr);
				adapter_group.SetDropDownViewResource(Resource.Layout.SpinnerText);
				spinnerGroup.Adapter = adapter_group;

				spinnerGroup.ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
					if (e.Position != -1 && groupId != e.Position)
					{
						int index = e.Position;
						groupId = index;

						if (groupId != 0)
							changeGroup(_groupList[index - 1].groupId);
					}
				};

				addEmptyUsertoList();
			}
			catch (Exception e) {
			}
		}

		public void userGroupMembersWithResult(List<GroupMemberInfo> membersArray)
		{
			try{
				progDlg.Dismiss();

				_userList = membersArray;

				userId = 0;
				Spinner spinnerUser = FindViewById<Spinner>(Resource.Id.spinnerUser);
				String[] _userNameArr = new String[membersArray.Count + 1];
				_userNameArr[0] = "No User";
				for (int i = 1; i <= _userList.Count; i++)
					_userNameArr[i] = _userList[i - 1].memberName;

				ArrayAdapter<String> adapter_group = new ArrayAdapter<String>(this, Resource.Layout.SpinnerText, _userNameArr);
				adapter_group.SetDropDownViewResource(Resource.Layout.SpinnerText);
				spinnerUser.Adapter = adapter_group;

				spinnerUser.ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
					if (e.Position != -1 && userId != e.Position)
					{
						int index = e.Position;
						userId = index;
					}
				};
			}
			catch (Exception e) {
			}
		}

		public void addTaskResult(int success, String message)
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
					Toast.MakeText(this, "Sucessfully posted", ToastLength.Short).Show();
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