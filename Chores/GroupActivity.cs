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
	public class GroupActivity : Activity
	{
		#region CONSTANTS
		const int ACTIVITY_FOR_CHANGEGROUP = 0;
		#endregion

		#region PRIVATE VARIABLES
		ProgressDialog _progDlg;
		List<GroupInfo> _groupList =  new List<GroupInfo>();
		List<GroupMemberInfo> _groupMemberList = new List<GroupMemberInfo> ();

		MemberItemAdapter adapter = null;

		int _selGroupId = -1;
		int _selMemberId = -1;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Group);

			Button butGroupNew = FindViewById<Button> (Resource.Id.btnJoinGroup);

			butGroupNew.Click += delegate {
				Intent GroupNewIntent = new Intent(this, typeof(GroupNewActivity));
				StartActivityForResult(GroupNewIntent, ACTIVITY_FOR_CHANGEGROUP);
			};

			Button butGroupClose = FindViewById<Button> (Resource.Id.btnCloseGroup);

			butGroupClose.Click += delegate {
				var builder = new AlertDialog.Builder(this);
				builder.SetTitle("Warning");
				builder.SetMessage ("Do you want to close this group?");
				builder.SetPositiveButton("Yes", (sender, args) => { 
					_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
					_progDlg.CancelEvent += (object sender1, EventArgs e) => {
					};

					WebService.doCloseGroup(this, _groupList[_selGroupId].groupId);
				});
				builder.SetNegativeButton("No", (sender, args) => {  });
				builder.SetCancelable(false);
				builder.Show ();


			};

			Button butAcceptUser = FindViewById<Button> (Resource.Id.btnAccept);

			butAcceptUser.Click += delegate {
				_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				_progDlg.CancelEvent += (object sender, EventArgs e) => {
				};

				WebService.doAcceptUser(this, _groupMemberList[_selMemberId].memberId, _groupList[_selGroupId].groupId);
			};

			Button butKickUser = FindViewById<Button> (Resource.Id.btnKick);

			butKickUser.Click += delegate {
				_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				_progDlg.CancelEvent += (object sender, EventArgs e) => {
				};

				WebService.doKickUser(this, _groupMemberList[_selMemberId].memberId, _groupList[_selGroupId].groupId);
			};

			Button btnChangePoints = FindViewById<Button> (Resource.Id.btnAddSubPoints);
			btnChangePoints.Click += delegate {
				EditText txtPoints = FindViewById<EditText>(Resource.Id.txtNewPoints);
				if (String.IsNullOrEmpty(txtPoints.Text))
				{
					Toast.MakeText(this, "Please input points to change", ToastLength.Short);
					return;
				}

				_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				_progDlg.CancelEvent += (object sender, EventArgs e) => {
				};

				WebService.changeMemberPoints(this, _groupMemberList[_selMemberId].memberId, int.Parse(txtPoints.Text));
				txtPoints.Text = "";
			};

            LinearLayout butHome = FindViewById<LinearLayout>(Resource.Id.linearBtnHome);
            LinearLayout butActivity = FindViewById<LinearLayout>(Resource.Id.linearBtnActivity);
            LinearLayout butTask = FindViewById<LinearLayout>(Resource.Id.linearBtnTask);
            LinearLayout butNotification = FindViewById<LinearLayout>(Resource.Id.linearBtnNotify);

			butHome.Click += delegate {
				Intent homeIntent = new Intent(this, typeof(HomeActivity));
				StartActivity(homeIntent);
				Finish();
			};

			butActivity.Click += delegate {
				Intent activityIntent = new Intent(this, typeof(ActivityActivity));
				StartActivity(activityIntent);
				Finish();
			};

			butTask.Click += delegate {
				Intent taskIntent = new Intent(this, typeof(TaskActivity));
				StartActivity(taskIntent);
				Finish();
			};

			butNotification.Click += delegate {
				Intent notifyIntent = new Intent(this, typeof(NotifyActivity));
				StartActivity(notifyIntent);
				Finish();
			};

			updateUserInfo ();
			changeButtonEnable (0);
			runBackground ();

			Global.HideKeyboard (this, InputMethodService);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == ACTIVITY_FOR_CHANGEGROUP)
			{
				changeButtonEnable (0);
				runBackground ();
			}
		}

		#region PRIVATE METHODS
		private void changeButtonEnable(int status)
		{
			Button btnAccept = FindViewById<Button> (Resource.Id.btnAccept);
			btnAccept.Enabled = (_selMemberId != -1 && status == 0 && _selGroupId != -1);

			Button btnCloseGroup = FindViewById<Button> (Resource.Id.btnCloseGroup);
			btnCloseGroup.Enabled = (_selGroupId != -1);

			Button btnKick = FindViewById<Button> (Resource.Id.btnKick);
			btnKick.Enabled = (_selMemberId != -1 && _selGroupId != -1);

			Button btnChangePoints = FindViewById<Button> (Resource.Id.btnAddSubPoints);
			btnChangePoints.Enabled = (_selMemberId != -1 && _selGroupId != -1 && status == 1);
		}

		private void changeGroupMemberVisible(bool bFlag)
		{
			LinearLayout linearGroup = FindViewById<LinearLayout> (Resource.Id.linearGroup);
			LinearLayout linearGroupMain = FindViewById<LinearLayout> (Resource.Id.linearGroupMembers);
			LinearLayout linearGroupActions = FindViewById<LinearLayout> (Resource.Id.linearGroupActions);

			linearGroup.Visibility = (bFlag) ? ViewStates.Visible : ViewStates.Invisible;
			linearGroupMain.Visibility = (bFlag) ? ViewStates.Visible : ViewStates.Invisible;
			linearGroupActions.Visibility = (bFlag) ? ViewStates.Visible : ViewStates.Invisible;
		}

		private void runBackground()
		{
			_selGroupId = -1;
			_selMemberId = -1;

			_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
			_progDlg.CancelEvent += (object sender, EventArgs e) => {
			};

			WebService.doGetGroups(this, Global.userId, "", WebService.GET_GROUPS_GROUP, 0);
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

			FindViewById<TextView>(Resource.Id.txtPoints).Text = "";
			WebService.doGetGroupMembers(this, groupId, WebService.GET_GROUPMEMBERS_GROUP);
		}
		#endregion

		#region PUBLIC METHODS
		public void changeUserInfo(int itemId)
		{
			if (itemId != -1) {
				GroupMemberInfo _info = _groupMemberList [itemId];

				// Show points/photo
				changeButtonEnable (_info.status);
			} else
				changeButtonEnable (0);
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
						changeGroupMemberVisible(true);
						String[] _groupNameArr = new String[groupArray.Count];
						for (int i = 0; i < _groupList.Count; i++)
							_groupNameArr[i] = _groupList[i].groupName;

						ArrayAdapter<String> adapter_group = new ArrayAdapter<String>(this, Resource.Layout.SpinnerText/*Android.Resource.Layout.SimpleSpinnerItem*/, _groupNameArr);
						adapter_group.SetDropDownViewResource(Resource.Layout.SpinnerText);
						spinnerGroup.Adapter = adapter_group;

						spinnerGroup.ItemSelected += delegate(object sender, AdapterView.ItemSelectedEventArgs e) {
							if (e.Position != -1 && _selGroupId != e.Position)
							{
								int index = e.Position;
								_selGroupId = index;
								changeButtonEnable(0);
								changeGroup(_groupList[index].groupId);
							}
						};
					}
					else
					{
						changeGroupMemberVisible(false);
					}
				}
			}
			catch (Exception e) {
			}
		}

		public void userGroupMembersWithResult(List<GroupMemberInfo> membersArray)
		{
			try{
				_progDlg.Dismiss();

				_groupMemberList = membersArray;

				_selMemberId = -1;
				// Show members on the UI
				adapter = new MemberItemAdapter(this, _groupMemberList);
				FindViewById<ListView>(Resource.Id.listGroupMembers).Adapter = adapter;
				FindViewById<ListView>(Resource.Id.listGroupMembers).ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
					if (_selMemberId != -1)
					{
						FindViewById<ListView>(Resource.Id.listGroupMembers).GetChildAt(_selMemberId).FindViewById<LinearLayout>(Resource.Id.linearMemberRoot).SetBackgroundColor(Android.Graphics.Color.Transparent);
					}

					_selMemberId = e.Position;
					FindViewById<TextView>(Resource.Id.txtPoints).Text = _groupMemberList[_selMemberId].points.ToString();
					Global.GetImageFromUrl(this, FindViewById<ImageView>(Resource.Id.imgUserPhoto), _groupMemberList[_selMemberId].imgPath);
					changeUserInfo(e.Position);

					FindViewById<ListView>(Resource.Id.listGroupMembers).GetChildAt(e.Position).FindViewById<LinearLayout>(Resource.Id.linearMemberRoot).SetBackgroundColor(Android.Graphics.Color.Rgb(0xcc, 0xcc, 0xcc));
				};

				changeUserInfo(-1);
			}
			catch (Exception e) {
			}
		}

		public void groupCloseResult(int success, String message)
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
					changeButtonEnable (0);
					runBackground();
				}
			}
			catch (Exception e) {
			}
		}

		public void acceptUserResult(int success, String message)
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
					changeButtonEnable(0);
					changeGroup(_groupList[_selGroupId].groupId);
				}
			}
			catch (Exception e) {
			}
		}

		public void kickUserResult(int success, String message)
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
					changeButtonEnable(0);
					changeGroup(_groupList[_selGroupId].groupId);
				}
			}
			catch (Exception e) {
			}
		}

		public void changePointsResult(int success, String message)
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
					changeButtonEnable(0);
					changeGroup(_groupList[_selGroupId].groupId);
				}
			}
			catch (Exception e) {
			}
		}
		#endregion 
	}
}