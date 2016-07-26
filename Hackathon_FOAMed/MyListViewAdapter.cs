using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using System.Threading.Tasks;
using System.Net.Http;

namespace FOAMsearch
{
    class MyListViewAdapter : BaseAdapter<string>
    {
        private List<MainActivity.infoBlock> mItems;
        private Context mContext;

        public MyListViewAdapter(Context context, List<MainActivity.infoBlock> items)
        {
            mItems = items;
            mContext = context;
        }

        public override int Count
        {
            get { return mItems.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position]
        {
            get { return mItems[position].name; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if (row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.ListView_Row, null, false);
            }

            TextView txtName = row.FindViewById<TextView>(Resource.Id.txtName);
            TextView txtContent = row.FindViewById<TextView>(Resource.Id.txtContent);
            TextView txtLink = row.FindViewById<TextView>(Resource.Id.txtLink);
            ImageView txtPicture = row.FindViewById<ImageView>(Resource.Id.txtPicture);

            txtName.Text = mItems[position].name;
            txtContent.Text = mItems[position].contents;

            if (mItems[position].link != null)
            {
                txtLink.Text = mItems[position].link;
            }
            else
            {
                txtLink.Visibility = ViewStates.Gone;
            }

            if (mItems[position].imgURL != null)
            {
                GetImageBitmapFromUrlAsync(position, txtPicture);
            }
            else
            {
                txtPicture.Visibility = ViewStates.Gone;
            }
            
            return row;
        }

        private async void GetImageBitmapFromUrlAsync(int position, ImageView txtPicture)
        {
            Bitmap imageBitmap = null;

            using (var httpClient = new HttpClient())
            {
                var imageBytes = await httpClient.GetByteArrayAsync(mItems[position].imgURL);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            txtPicture.SetImageBitmap(imageBitmap);
        }
    }
}