using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net;
using Newtonsoft.Json;
using static Android.Widget.ViewSwitcher;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace AutospotsApp
{
    [Activity(Label = "ViewLotActivity")]
    public class ViewLotActivity : Activity, IViewFactory
    {
        int lotIndex;
        WebClient mClient;
        WebClient mClient1;
        string[] lotNames;
        Spinner lotSelector;
        ImageSwitcher lotImageSwitcher;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            mClient = new WebClient();
            mClient1 = new WebClient();
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadLotListCompleted;

            float dps = this.Resources.DisplayMetrics.Density;
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            mainlayout.AddView(titleText);
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.ViewLotsButtonText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            //ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText);
            lotSelector = new Spinner(this);
            lotSelector.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(lotSelector_ItemSelected);
            mainlayout.AddView(lotSelector);
            lotImageSwitcher = new ImageSwitcher(this);
            lotImageSwitcher.SetFactory(this);
            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);

            mainlayout.AddView(lotImageSwitcher,lp);
            // Create your application here

            SetContentView(mainlayout);
        }

        private void MClient_DownloadLotListCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                string json = Encoding.UTF8.GetString(e.Result);
                lotNames = JsonConvert.DeserializeObject<string[]>(json);
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotSelector.Adapter = adapter;
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        private void lotSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            lotIndex = e.Position;
            mClient1.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/image/"+lotIndex+"/"));
            mClient1.DownloadDataCompleted += MClient_DownloadImageResponseCompleted;
        }

        private void MClient_DownloadImageResponseCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                string json = Encoding.UTF8.GetString(e.Result);
                LotImageResponse li = JsonConvert.DeserializeObject<LotImageResponse>(json);
                Bitmap bm = BitmapFactory.DecodeByteArray(li.Image, 0, li.Image.Length);
                Drawable im = new BitmapDrawable(bm);
                lotImageSwitcher.SetImageDrawable(im);
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        View IViewFactory.MakeView()
        {
            ImageView image = new ImageView(this);
            image.SetScaleType(ImageView.ScaleType.FitCenter);
            //image.LayoutParameters = new ImageSwitcher.LayoutParams(LayoutParams()) => not found !
            return image;
        }
    }
}