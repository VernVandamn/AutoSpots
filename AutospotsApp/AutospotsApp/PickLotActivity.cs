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

namespace AutospotsApp
{
    [Activity(Label = "PickLotActivity")]
    public class PickLotActivity : Activity
    {
        int pos;
        WebClient mClient;
        string[] lotNames;
        Spinner parkingLotChooser;
        //Lot[] lotArray;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);

            mClient = new WebClient();

            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadDataCompleted;
            // Create your application here
            float dps = Resources.DisplayMetrics.Density;
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            mainlayout.AddView(titleText);
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.InLotButtonText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            //ViewGroup.LayoutParams ps1 = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            TextView buildingPickerLabel = new TextView(this);
            buildingPickerLabel.Text = GetString(Resource.String.SelectAParkingLotText);
            mainlayout.AddView(buildingPickerLabel);
            parkingLotChooser = new Spinner(this);
            //statChooser.SetBackgroundColor(Android.Graphics.Color.Black);
            parkingLotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(parkingLotChooser_ItemSelected);
            //var adapter2 = ArrayAdapter.CreateFromResource(
                    //this, Resource.Array.SelectParkingLotArray, Android.Resource.Layout.SimpleSpinnerItem);

            //adapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            //parkingLotChooser.Adapter = adapter2;
            parkingLotChooser.SetPadding(0, 0, 0, 40 * (int)dps);
            mainlayout.AddView(parkingLotChooser);
            Button okButton = new Button(this);
            okButton.Text = "OK";
            okButton.Click += delegate {
                ClickOk();
            };
            mainlayout.AddView(okButton);
            SetContentView(mainlayout);
        }

        private void MClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try {
                string json = Encoding.UTF8.GetString(e.Result);
                lotNames = JsonConvert.DeserializeObject<string[]>(json);
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                parkingLotChooser.Adapter = adapter;
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        private void parkingLotChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            pos = e.Position;
        }

        private void ClickOk()
        {
            var nbActivity = new Intent(this, typeof(FindNearestActivity));
            nbActivity.PutExtra("lotIndex", pos);
            StartActivity(nbActivity);
        }
    }
}