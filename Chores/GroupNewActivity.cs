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
	public class GroupNewActivity : Activity
	{
		#region PRIVATE CONSTANTS
		ProgressDialog _progDlg;
		ListView _listViewGroups;

		List<GroupInfo> _groupList;
		GroupItemAdapter adapter = null;

		int _selectedGroupId = -1;
		#endregion

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.GroupNew);

			Button butGroupNew = FindViewById<Button> (Resource.Id.btnCreateGroup);

			butGroupNew.Click += delegate {
				EditText txtGroupName = FindViewById<EditText>(Resource.Id.txtGroupName);
				if (String.IsNullOrEmpty(txtGroupName.Text))
				{
					Toast.MakeText(this, "Please input group name first", ToastLength.Short).Show();
					return;
				}

				_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				_progDlg.CancelEvent += (object sender, EventArgs e) => {
				};

				WebService.doAddGroup(this, Global.userId, txtGroupName.Text);
			};

			Button butGroupJoin = FindViewById<Button> (Resource.Id.btnSendJoin);

			butGroupJoin.Click += delegate {
				if (_selectedGroupId == -1)
				{
					Toast.MakeText(this, "Please select group first", ToastLength.Short).Show();
					return;
				}

				_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				_progDlg.CancelEvent += (object sender1, EventArgs e1) => {
				};

				WebService.doJoinGroup(this, Global.userId, _groupList[_selectedGroupId].groupId);
			};

			_listViewGroups = FindViewById<ListView> (Resource.Id.listGroups);
			_listViewGroups.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e) {
				if (e.Position != -1)
					changeButtonEvents(true);
				else
					changeButtonEvents(false);

				_selectedGroupId = e.Position;
			};

			ImageView imgSearch = FindViewById<ImageView> (Resource.Id.imgSearch);
			imgSearch.Click += delegate {
				_selectedGroupId = -1;
				EditText txtGroupName = FindViewById<EditText>(Resource.Id.txtSearchName);
			/*	if (String.IsNullOrEmpty(txtGroupName.Text))
				{
					Toast.MakeText(this, "Please input search text", ToastLength.Short).Show();
					return;
				}*/

				_progDlg = ProgressDialog.Show (this, "", GetString(Resource.String.msg_wait), true, true);
				_progDlg.CancelEvent += (object sender, EventArgs e) => {
				};

				WebService.doGetGroups(this, -1, txtGroupName.Text, WebService.GET_GROUPS_GROUPNEW, 0);
			};

            ImageView imgBack = FindViewById<ImageView>(Resource.Id.imgBack);
            imgBack.Click += delegate
            {
                SetResult(Result.Ok);
                Finish();
            };

			changeButtonEvents (false);

			Global.HideKeyboard (this, InputMethodService);
		}

		public override void OnBackPressed ()
		{
			SetResult (Result.Ok);

			base.OnBackPressed ();
		}

		#region PRIVATE METHODS
		private void changeButtonEvents(bool bFlag)
		{
			Button butGroupJoin = FindViewById<Button> (Resource.Id.btnSendJoin);
			butGroupJoin.Enabled = bFlag;
		}
		#endregion

		#region WEBSERVICE
		public void groupAddResult(int success, String message, int groupId)
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
					Toast.MakeText(this, "Sucessfully created", ToastLength.Short).Show();
				}
			}
			catch (Exception e) {
			}
		}

		public void groupJoinResult(int success, String message)
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
					Toast.MakeText(this, "Sucessfully posted", ToastLength.Short).Show();
				}
			}
			catch (Exception e) {
			}
		}

		public void userGroupsWithResult(List<GroupInfo> groupArray)
		{ 
			try{
				_progDlg.Dismiss();

				_groupList = groupArray;

				adapter = new GroupItemAdapter(this, _groupList);
				FindViewById<ListView>(Resource.Id.listGroups).Adapter = adapter;
			}
			catch (Exception e) {
			}
		}
		#endregion
	}
}