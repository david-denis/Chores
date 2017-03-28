using System;
using System.Json;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Java.Net;

using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.Views.InputMethods;

namespace Chores
{
	public class Global
	{
		#region CONSTANTS
		public const String ServerUrl = "http://hardslashdesign.com/service/index.php?";
		public const String imgPhotoPrefix = "http://hardslashdesign.com/service/";
		public const String imgUploadPath = "http://hardslashdesign.com/service/upload.php";

		//public const String ServerUrl = "http://192.168.1.71/hardslashdesign/index.php?";
		//public const String imgPhotoPrefix = "http://192.168.1.71/hardslashdesign/";
		//public const String imgUploadPath = "http://192.168.1.71/hardslashdesign/upload.php";

		public const String WebSvcLoginFormat = "method={0}&username={1}&pwd={2}";
		public const String UserLoginFunc = "login";

		public const String WebSvcRegUserFormat = "method={0}&username={1}&email={2}&pwd={3}&imgpath={4}";
		public const String UserRegisterFunc = "registeruser";

		public const String WebSvcGetGroupFormat = "method={0}&userid={1}&groupname={2}&type={3}";
		public const String UserGroupFunc = "getGroups";

		public const String WebSvcGetGroupMembersFormat = "method={0}&groupid={1}&type={2}";
		public const String UserGroupMembersFunc = "getGroupMembers";

		public const String WebSvcAddGroupFormat = "method={0}&userid={1}&groupname={2}";
		public const String UserGroupAddFunc = "addGroup";

		public const String WebSvcJoinGroupFormat = "method={0}&userid={1}&groupid={2}";
		public const String UserGroupJoinFunc = "joinGroup";

		public const String WebSvcCloseGroupFormat = "method={0}&groupid={1}";
		public const string UserGroupCloseFunc = "closeGroup";

		public const String WebSvcAcceptUserFormat = "method={0}&userid={1}&groupid={2}";
		public const string UserAcceptUserFunc = "acceptuser";

		public const String WebSvcKickUserFormat = "method={0}&userid={1}&groupid={2}";
		public const string UserKickUserFunc = "declineuser";

		public const String WebSvcAddTaskFormat = "method={0}&userid={1}&taskname={2}&description={3}&point={4}&priority={5}&group={6}&user={7}&finishDate={8}&postDate={9}&imagepath={10}";
		public const String UserAddTaskFunc = "addTask";

		public const String WebSvcGetChoresFormat = "method={0}&userid={1}&groupid={2}&type={3}";
		public const String UserGetChoresFunc = "getChores";

		public const String WebSvcAcceptTaskFormat = "method={0}&userid={1}&taskid={2}";
		public const String UserAcceptTaskFunc = "acceptTask";

		public const String WebSvcStartTaskFormat = "method={0}&taskid={1}&userid={2}";
		public const String UserStartTaskFunc = "startTask";

		public const String WebSvcEndTaskFormat = "method={0}&taskid={1}";
		public const String UserEndTaskFunc = "endTask";

		public const String WebSvcGetBidUsersFormat = "method={0}&taskid={1}";
		public const String UserGetBidUsersFunc = "getBidUsers";

		public const String WebSvcChangeTaskStateFormat = "method={0}&taskuserid={1}&taskid={2}&status={3}";
		public const String UserChangeTaskStateFunc = "changeTaskState";

		public const String WebSvcChangeMemberPointsFormat = "method={0}&userid={1}&point={2}";
		public const String UserChangeMemberPointsFunc = "changeMemberPoints";

		public const String WebSvcGetActivitiesFormat = "method={0}&groupid={1}";
		public const String UserGetActivitiesFunc = "getuseractivities";

		public const String WebSvcGetUserHistoryFormat = "method={0}&userid={1}";
		public const String UserGetUserHistoryFunc = "getuserhistory";

		public const String WebSvcGetNotificationsFormat = "method={0}&userid={1}";
		public const String UserGetNotificationFunc = "getnotification";

		public const String WebSvcClearNotificationsFormat = "method={0}&userid={1}";
		public const String UserClearNotificationFunc = "clearnotification";

		public static Global _global = null;
		#endregion

		#region STATIC VARIABLES
		public static int userId = -1;
		public static String userName = "";
		public static int userPoint = 0;
		public static String userPhoto = "";
		#endregion

		#region PRIVATE VARIABLES
		#endregion

		#region CONSTRUCTOR
		public Global ()
		{
		}
		#endregion

		#region STATIC FUNCTIONS
		public static Global getSharedInstance()
		{
			if (_global == null) {
				_global = new Global ();
				// Do initialization
			}

			return _global;
		}

		public static String getTaskStatus(int type)
		{
			String retType = "-";

			switch (type) {
			case 0:
				retType = "New Post";
				break;
			case 1:
				retType = "Working";
				break;
			case 2:
				retType = "Finished";
				break;
			case 3:
				retType = "Completed";
				break;
			}

			return retType;
		}

		public static String getTaskUserAction(int type)
		{
			String retType = "-";

			switch (type) {
			case 0:
				retType = "Accepted";
				break;
			case 1:
				retType = "Started";
				break;
			case 2:
				retType = "Finished";
				break;
			case 3:
				retType = "Completed";
				break;
			}

			return retType;
		}

		// Download Image from server and set it to imageview
		public static void GetImageFromUrl(Activity activity, ImageView imgView, String url)
		{
			try
			{
				if (String.IsNullOrEmpty(url))
				{
					imgView.SetImageResource(Resource.Drawable.user);
					return;
				}

				WebClient client = new WebClient ();
				client.DownloadDataAsync (new Uri (url));
				client.DownloadDataCompleted += delegate(object sender, DownloadDataCompletedEventArgs e) {
					if (e.Cancelled == false)
					{
						if (e.Result != null)
						{
							activity.RunOnUiThread (() => {
								var bitmap = BitmapFactory.DecodeByteArray(e.Result, 0, e.Result.Length);
								imgView.SetImageBitmap(bitmap);
							} );
						}
					}
				};
			}
			catch (Exception e) {
			}
		}
		//

		// Hide Keyboard from the view
		public static void HideKeyboard(Activity activity, String methodService)
		{
			try
			{
				if (activity != null) {
					InputMethodManager manager = (InputMethodManager) activity.GetSystemService(methodService);
					manager.HideSoftInputFromWindow(activity.CurrentFocus.WindowToken, 0);
				}
			}
			catch (Exception e) {
			}
		}
		#endregion
	}
}

