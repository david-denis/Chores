
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
	public class ChoresItemAdapter : BaseAdapter<TaskInfo>
	{
		List<TaskInfo> items;
		Activity context;

		public ChoresItemAdapter(Activity context, List<TaskInfo> items) : base()
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
				view = context.LayoutInflater.Inflate (Resource.Layout.ChoresItem, null);
			}

			view.FindViewById<TextView> (Resource.Id.txtTaskName).Text = item.taskName;
			view.FindViewById<TextView> (Resource.Id.txtPoints).Text = item.points.ToString();

			if (position % 2 == 0) {
				view.FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.White);
			} else {
				view.FindViewById<RelativeLayout> (Resource.Id.relativeItemRoot).SetBackgroundColor (Android.Graphics.Color.LightGray);
			}

			return view;
		}
	}
}