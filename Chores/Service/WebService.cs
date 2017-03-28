using System;
using System.Collections.Generic;
using System.Linq;
using System.Json;
using System.Net;
using System.IO;

using Android.App;
using Java.Net;

namespace Chores
{
	public class WebService
	{
		#region PRIVATE CONSTANT
		#endregion

		#region CONSTRUCTOR
		public WebService ()
		{
		}
		#endregion

		#region PUBLIC CONSTRUCTOR
		public static void doLogin(Activity activity, String userName, String password)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcLoginFormat, Global.UserLoginFunc, URLEncoder.Encode(userName), URLEncoder.Encode(password));
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "Cannot connect to server";
				int userId = -1;

				try
				{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           

						try{
							var s = response.GetResponseStream ();
							var j = (JsonObject)JsonObject.Load (s);

							success = j["success"];
							msg = j["error"];
							userId = j["userId"];
						}
						catch (Exception e)
						{
						}
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((LoginActivity)activity).loginWithResult(success, msg, userId);
				} );

			} , httpReq);
		}

		public static void doUploadPhoto(Activity activity, String filePath)
		{
			string imgpath = "";

			try
			{
				System.Net.WebClient Client = new System.Net.WebClient();
				Client.Headers.Add("Content-Type", "binary/octet-stream");
				Client.UploadFileAsync(new Uri(Global.imgUploadPath), "POST", filePath);
				Client.UploadFileCompleted += delegate(object sender, UploadFileCompletedEventArgs e) {
					imgpath = System.Text.Encoding.UTF8.GetString(e.Result, 0, e.Result.Length);
					activity.RunOnUiThread (() => {
						((SignupActivity)activity).uploadPhotoWithResult(imgpath);
					} );
				};

			}
			catch (Exception e) {
			}
		}

		public static void doRegisterUser(Activity activity, String userName, String userEmail, String userPwd, String imagPath)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcRegUserFormat, Global.UserRegisterFunc, URLEncoder.Encode(userName), URLEncoder.Encode(userEmail), URLEncoder.Encode(userPwd), URLEncoder.Encode(imagPath));
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";
				int userId = -1;

				try
				{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
						userId = j["userId"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((SignupActivity)activity).registerWithResult(success, msg, userId);
				} );
			} , httpReq);
		}

		public const int GET_GROUPS_GROUP = 0;
		public const int GET_GROUPS_GROUPNEW = 1;
		public const int GET_GROUPS_TASKNEW = 2;
		public const int GET_GROUPS_CHORES = 3;
		public const int GET_GROUPS_ACTIVITY = 4;

		public static void doGetGroups(Activity activity, int userId, String groupName, int type, int flag)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcGetGroupFormat, Global.UserGroupFunc, userId, URLEncoder.Encode(groupName), flag);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";
				List<GroupInfo> _resultGroup = new List<GroupInfo>();

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						JsonArray groupsArr = (JsonArray)j ["groups"];

						if (type == GET_GROUPS_GROUP || type == GET_GROUPS_ACTIVITY)
						{
							JsonObject userInfo = (JsonObject)j["userinfo"];

							Global.userName = userInfo["userName"];
							Global.userPoint = userInfo["points"];
							if (String.IsNullOrEmpty(userInfo["imgpath"]) == false)
								Global.userPhoto = Global.imgPhotoPrefix + userInfo["imgpath"];
							success = userInfo["success"];
							msg = userInfo["error"];
						}

						for (int i = 0; i < groupsArr.Count; i++)
						{
							JsonObject groupOne = (JsonObject)groupsArr[i];
							GroupInfo _groupInfo = new GroupInfo();
							_groupInfo.groupId = groupOne["uid"];
							_groupInfo.groupName = groupOne["groupname"];
							_groupInfo.leaderId = groupOne["leader"];

							if (type != GET_GROUPS_GROUPNEW || _groupInfo.leaderId != Global.userId)
								_resultGroup.Add(_groupInfo);
						}
					}
				}
				catch (Exception e)
				{
				}

				if (type == GET_GROUPS_GROUPNEW)
				{
					activity.RunOnUiThread (() => {
						((GroupNewActivity)activity).userGroupsWithResult(_resultGroup);
					} );
				}
				else if (type == GET_GROUPS_GROUP)
				{
					activity.RunOnUiThread (() => {
						((GroupActivity)activity).userGroupsWithResult(success, msg, _resultGroup);
					} );
				}
				else if (type == GET_GROUPS_TASKNEW)
				{
					activity.RunOnUiThread (() => {
						((TaskNewActivity)activity).userGroupsWithResult(_resultGroup);
					} );
				}
				else if (type == GET_GROUPS_CHORES)
				{
					activity.RunOnUiThread (() => {
						((ChoresActivity)activity).userGroupsWithResult(_resultGroup);
					} );
				}
				else if (type == GET_GROUPS_ACTIVITY)
				{
					activity.RunOnUiThread (() => {
						((ActivityActivity)activity).userGroupsWithResult(success, msg, _resultGroup);
					} );
				}
			} , httpReq);
		}

		public const int GET_GROUPMEMBERS_GROUP = 0;
		public const int GET_GROUPMEMBERS_TASKNEW = 1;

		public static void doGetGroupMembers(Activity activity, int groupId, int type)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcGetGroupMembersFormat, Global.UserGroupMembersFunc, groupId, type);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";
				List<GroupMemberInfo> _resultMembers = new List<GroupMemberInfo>();

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var membersArr = (JsonArray)JsonObject.Load (s);

						for (int i = 0; i < membersArr.Count; i++)
						{
							JsonObject memberOne = (JsonObject)membersArr[i];
							GroupMemberInfo _memberInfo = new GroupMemberInfo();
							_memberInfo.memberId = memberOne["uid"];
							_memberInfo.memberName = memberOne["username"];
							if (String.IsNullOrEmpty(memberOne["imgpath"]) == false)
								_memberInfo.imgPath = Global.imgPhotoPrefix + memberOne["imgpath"];
							_memberInfo.points = memberOne["points"];
							_memberInfo.status = memberOne["status"];

							if (type == 1 && _memberInfo.memberId == Global.userId)
							{
							}
							else
								_resultMembers.Add(_memberInfo);
						}
					}
				}
				catch (Exception e)
				{
				}

				if (type == GET_GROUPMEMBERS_GROUP)
				{
					activity.RunOnUiThread (() => {
						((GroupActivity)activity).userGroupMembersWithResult(_resultMembers);
					} );
				}
				else if (type == GET_GROUPMEMBERS_TASKNEW)
				{
					activity.RunOnUiThread (() => {
						((TaskNewActivity)activity).userGroupMembersWithResult(_resultMembers);
					} );
				}
			} , httpReq);
		}

		public static void doAddGroup(Activity activity, int userId, String groupName)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcAddGroupFormat, Global.UserGroupAddFunc, userId, URLEncoder.Encode(groupName));
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				try
				{
					int success = 0;
					String msg = "";
					int groupId = -1;

					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						try{
							var s = response.GetResponseStream ();
							var j = (JsonObject)JsonObject.Load (s);

							success = j["success"];
							msg = j["error"];
							groupId = j["groupId"];
						}
						catch (Exception e)
						{
						}
					}

					activity.RunOnUiThread (() => {
						((GroupNewActivity)activity).groupAddResult(success, msg, groupId);
					} );
				}
				catch (Exception e)
				{
				}
			} , httpReq);
		}

		public static void doJoinGroup(Activity activity, int userId, int groupId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcJoinGroupFormat, Global.UserGroupJoinFunc, userId, groupId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((GroupNewActivity)activity).groupJoinResult(success, msg);
				} );

			} , httpReq);
		}

		public static void doCloseGroup(Activity activity, int groupId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcCloseGroupFormat, Global.UserGroupCloseFunc, groupId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((GroupActivity)activity).groupCloseResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doAcceptUser(Activity activity, int userId, int groupId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcAcceptUserFormat, Global.UserAcceptUserFunc, userId, groupId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((GroupActivity)activity).acceptUserResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doKickUser(Activity activity, int userId, int groupId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcKickUserFormat, Global.UserKickUserFunc, userId, groupId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((GroupActivity)activity).kickUserResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doAddTask(Activity activity, int userId, String taskname, String description, int point, int priority, int group, int user, String finishDate, String postDate, String imgPath)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcAddTaskFormat, Global.UserAddTaskFunc, userId, URLEncoder.Encode(taskname), URLEncoder.Encode(description), point, priority, group, user, URLEncoder.Encode(finishDate), URLEncoder.Encode(postDate), imgPath);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((TaskNewActivity)activity).addTaskResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doGetTasks(Activity activity, int userId, int groupId, int type)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcGetChoresFormat, Global.UserGetChoresFunc, userId, groupId, type);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				List<TaskInfo> _taskList = new List<TaskInfo>();
				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						JsonObject userinfo = (JsonObject)j["userinfo"];
						JsonArray choresArr = (JsonArray)j["chores"];

						Global.userName = userinfo["userName"];
						Global.userPoint = userinfo["points"];
						if (String.IsNullOrEmpty(userinfo["imgpath"]) == false)
							Global.userPhoto = Global.imgPhotoPrefix + userinfo["imgpath"];

						success = userinfo["success"];
						msg = userinfo["error"];

						for (int i = 0; i < choresArr.Count; i++)
						{
							JsonObject _obj = (JsonObject)choresArr[i];

							TaskInfo _newInfo = new TaskInfo();
							_newInfo.taskId = _obj["uid"];
							_newInfo.taskUserId = _obj["taskuseruid"];
							_newInfo.taskName = _obj["taskname"];
							_newInfo.description = _obj["description"];

							_newInfo.postuserid = _obj["postuserid"];

							_newInfo.postName = _obj["postusername"];
							_newInfo.groupName = _obj["groupname"];
							_newInfo.userName = _obj["username"];

							_newInfo.groupid = _obj["groupid"];
							_newInfo.userid = _obj["userid"];

							_newInfo.priority = _obj["priority"];
							_newInfo.points = _obj["points"];

							_newInfo.status = _obj["status"];
							_newInfo.userstatus = _obj["userstatus"];

							_newInfo.postdate = DateTime.Parse(_obj["postdate"]);
							_newInfo.finishdate = DateTime.Parse(_obj["finishdate"]);

							_taskList.Add(_newInfo);
						}
					}
				}
				catch (Exception e)
				{
				}

				if (type == 0)
				{
					activity.RunOnUiThread (() => {
						((ChoresActivity)activity).getPostedTasksResult(success, msg, _taskList);
					} );
				}
				else if (type == 1)
				{
					activity.RunOnUiThread (() => {
						((TaskActivity)activity).getPostedTasksResult(success, msg, _taskList);
					} );
				}
				else if (type == 2)
				{
					activity.RunOnUiThread (() => {
						((HomeActivity)activity).getPostedTasksResult(success, msg, _taskList);
					} );
				}
			} , httpReq);
		}

		public static void doAcceptTask(Activity activity, int userId, int taskId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcAcceptTaskFormat, Global.UserAcceptTaskFunc, userId, taskId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				List<TaskInfo> _taskList = new List<TaskInfo>();
				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((ChoresActivity)activity).acceptTaskResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doStartTask(Activity activity, int taskId, int userId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcStartTaskFormat, Global.UserStartTaskFunc, taskId, userId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((TaskDetailActivity)activity).startTaskResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doEndTask(Activity activity, int taskId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcEndTaskFormat, Global.UserEndTaskFunc, taskId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((TaskDetailActivity)activity).endTaskResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doGetBidUsers(Activity activity, int taskId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcGetBidUsersFormat, Global.UserGetBidUsersFunc, taskId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";
				List<UserInfo> _bidUsers = new List<UserInfo>();

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var bidUserArr = (JsonArray)JsonObject.Load (s);

						for (int i = 0; i < bidUserArr.Count; i++)
						{
							JsonObject _object = (JsonObject)bidUserArr[i];
							UserInfo _info = new UserInfo();
							_info.userId = _object["uid"];
							_info.userName = _object["username"];
							_info.imgPath = Global.imgPhotoPrefix + _object["imgpath"];
							_info.points = _object["points"];
							_info.createddate = _object["createdate"];

							_bidUsers.Add(_info);
						}
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((TaskDetailActivity)activity).getBidUsersResult(_bidUsers);
				} );
			} , httpReq);
		}

		public static void changeTaskState(Activity activity, int taskUserId, int taskId, int state, int type)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcChangeTaskStateFormat, Global.UserChangeTaskStateFunc, taskUserId, taskId, state);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				if (type == 0)
				{
					activity.RunOnUiThread (() => {
						((TaskDetailActivity)activity).changeTaskResult(success, msg);
					} );
				}
				else
				{
					activity.RunOnUiThread (() => {
						((HomeActivity)activity).changeTaskResult(success, msg);
					} );
				}
			} , httpReq);
		}

		public static void changeMemberPoints(Activity activity, int userId, int points)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcChangeMemberPointsFormat, Global.UserChangeMemberPointsFunc, userId, points);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						success = j["success"];
						msg = j["error"];
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((GroupActivity)activity).changePointsResult(success, msg);
				} );
			} , httpReq);
		}

		public static void doGetActivities(Activity activity, int groupId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcGetActivitiesFormat, Global.UserGetActivitiesFunc, groupId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				List<ActivityInfo> _activities = new List<ActivityInfo>();
				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var activityArr = (JsonArray)JsonObject.Load (s);

						for (int i = 0; i < activityArr.Count; i++)
						{
							JsonObject _object = (JsonObject)activityArr[i];
							ActivityInfo _info = new ActivityInfo();
							_info.userId = _object["userid"];
							_info.userName = _object["username"];
							_info.status = _object["status"];
							_info.points = _object["points"];
							_info.taskName = _object["taskname"];

							_activities.Add(_info);
						}
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((ActivityActivity)activity).getActivitiesResult(_activities);
				} );
			} , httpReq);
		}

		public static void doGetHistory(Activity activity, int userId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcGetUserHistoryFormat, Global.UserGetUserHistoryFunc, userId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				List<ActivityInfo> _activities = new List<ActivityInfo>();

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var activityArr = (JsonArray)JsonObject.Load (s);

						for (int i = 0; i < activityArr.Count; i++)
						{
							JsonObject _object = (JsonObject)activityArr[i];
							ActivityInfo _info = new ActivityInfo();
							_info.userId = _object["userid"];
							_info.userName = _object["username"];
							_info.status = _object["status"];
							_info.points = _object["points"];
							_info.taskName = _object["taskname"];

							_activities.Add(_info);
						}
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((ActivityActivity)activity).getHistoryResult(_activities);
				} );
			} , httpReq);
		}

		public static void doGetNotifications(Activity activity, int userId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcGetNotificationsFormat, Global.UserGetNotificationFunc, userId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				int success = 0;
				String msg = "";

				List<NotificationInfo> _notifies = new List<NotificationInfo>();

				try{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						var s = response.GetResponseStream ();
						var j = (JsonObject)JsonObject.Load (s);

						JsonObject userinfo = (JsonObject)j["userinfo"];
						JsonArray notifyArr = (JsonArray)j["notification"];

						Global.userName = userinfo["username"];
						Global.userPoint = userinfo["points"];
						if (String.IsNullOrEmpty(userinfo["imgpath"]) == false)
							Global.userPhoto = Global.imgPhotoPrefix + userinfo["imgpath"];

						for (int i = 0; i < notifyArr.Count; i++)
						{
							JsonObject _object = (JsonObject)notifyArr[i];
							NotificationInfo _info = new NotificationInfo();
							_info.notifyId = _object["uid"];
							_info.message = _object["message"];
							_info.createdate = DateTime.Parse(_object["createdate"]);

							_notifies.Add(_info);
						}
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((NotifyActivity)activity).getNotificationResult(_notifies);
				} );
			} , httpReq);
		}

		public static void doClearNotification(Activity activity, int userId)
		{
			string url = Global.ServerUrl + String.Format(Global.WebSvcClearNotificationsFormat, Global.UserClearNotificationFunc, userId);
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));

			httpReq.BeginGetResponse ((ar) => {
				try
				{
					var request = (HttpWebRequest)ar.AsyncState;
					using (var response = (HttpWebResponse)request.EndGetResponse (ar))     {                           
						int success = 0;
						String msg = "";
					}
				}
				catch (Exception e)
				{
				}

				activity.RunOnUiThread (() => {
					((NotifyActivity)activity).clearNotificationResult();
				} );
			} , httpReq);
		}
		#endregion
	}
}


