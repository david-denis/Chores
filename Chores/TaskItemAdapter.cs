
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Chores
{
	public class TaskItemAdapter : BaseAdapter<TaskInfo>
	{
		List<TaskInfo> items;
		Activity context;

		public TaskItemAdapter(Activity context, List<TaskInfo> items) : base()
		{
			this.context = context;
			this.items = items;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override TaskInfo this[int position]
		{
			get { return items [position]; }
		}

		public override int Count
		{
			get { return items.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = items [position];

			View view = convertView;
			if (view == null) {
				view = context.LayoutInflater.Inflate (Resource.Layout.TaskNewListItem, null);
			}

			view.FindViewById<TextView> (Resource.Id.txtTaskName).Text = item.taskName;
			view.FindViewById<TextView> (Resource.Id.txtPostDate).Text = item.postdate.ToString("g");
			view.FindViewById<TextView> (Resource.Id.txtDescription).Text = item.description;
			view.FindViewById<TextView> (Resource.Id.txtState).Text = Global.getTaskStatus(item.status);

			if (position % 2 == 0) {
				view.FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.White);
			} else {
				view.FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.LightGray);
			}

			return view;
		}
	}
}