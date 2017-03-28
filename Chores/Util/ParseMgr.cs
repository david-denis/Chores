using System;

namespace Chores
{
	public class ParseMgr
	{
		public ParseMgr ()
		{
		}
	}

	public class TaskInfo
	{
		public int taskId = -1;
		public int taskUserId = -1;

		public String taskName = "";
		public String description = "";

		public int points = 0;

		public int postuserid = -1;
		public String postName = "";
		public String groupName = "";
		public String userName = "";

		public int groupid = -1;
		public int userid = -1;

		public int priority = 0;

		public DateTime postdate;
		public DateTime finishdate;

		public int status = 0;

		public int userstatus = 0;
		public String imgPath = "";
	}

	public class GroupInfo
	{
		public int groupId = -1;
		public String groupName = "";
		public int leaderId = -1;
	}

	public class GroupMemberInfo
	{
		public int memberId = -1;
		public String memberName = "";
		public String imgPath = "";
		public int points = 0;

		public int status = -1;
	}

	public class UserInfo
	{
		public int userId = -1;
		public String userName = "";
		public String imgPath = "";
		public int points = 0;

		public String createddate = "";
	}

	public class ActivityInfo
	{
		public int userId = -1;
		public String userName = "";
		public int points = 0;

		public int status = 0;
		public String taskName = "";
	}

	public class NotificationInfo
	{
		public int notifyId = -1;
		public String message = "";
		public DateTime createdate;
	}
}

