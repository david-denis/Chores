
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
	public class MemberItemAdapter : BaseAdapter<GroupMemberInfo>
	{
		List<GroupMemberInfo> items;
		Activity context;

		public MemberItemAdapter(Activity context, List<GroupMemberInfo> items) : base()
		{
			this.context = context;
			this.items = items;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override GroupMemberInfo this[int position]
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
				view = context.LayoutInflater.Inflate (Resource.Layout.MemberCustomView, null);
			}

			view.FindViewById<TextView> (Resource.Id.txtMemberName).Text = item.memberName;

			if (item.status == 0)
				view.FindViewById<LinearLayout> (Resource.Id.linearMemberRoot).SetBackgroundColor (Android.Graphics.Color.LightGreen);
			//else
			//	view.FindViewById<LinearLayout> (Resource.Id.linearMemberRoot).SetBackgroundColor (Android.Graphics.Color.White);

			return view;
		}
	}
}