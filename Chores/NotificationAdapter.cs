
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
	public class NotificationAdapter : BaseAdapter<NotificationInfo>
	{
		List<NotificationInfo> items;
		Activity context;

		public NotificationAdapter(Activity context, List<NotificationInfo> items) : base()
		{
			this.context = context;
			this.items = items;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override NotificationInfo this[int position]
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
				view = context.LayoutInflater.Inflate (Resource.Layout.NotificationItem, null);
			}

			view.FindViewById<TextView> (Resource.Id.txtMessage).Text = item.message;
			view.FindViewById<TextView> (Resource.Id.txtDate).Text = item.createdate.ToString("g");

			if (position % 2 == 0) {
				view.FindViewById<LinearLayout> (Resource.Id.linearItem).SetBackgroundColor (Android.Graphics.Color.White);
			} else {
				view.FindViewById<LinearLayout> (Resource.Id.linearItem).SetBackgroundColor (Android.Graphics.Color.LightGray);
			}

			return view;
		}
	}
}