using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using Android.Content;
using Android.Util;

namespace AutospotsApp
{
    [Activity(Label = "AutospotsApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class StartActivity : Activity
    {
        int userToken;
        Button parkButton;
        Spinner lotChooser;
        int lotIndex;
        WebClient mClient;
        WebClient mClient1;
        //Lot[] lotArray;
        string[] lotNames;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);

            StopService(new Intent(this, typeof(NavUpdateService))); //JUST IN CASE

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            mClient = new WebClient();
            mClient1 = new WebClient();
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadLotDataCompleted;


            lotIndex = 0;
            lotChooser = FindViewById<Spinner>(Resource.Id.LotPickSpinner);
            lotChooser.SetBackgroundColor(Android.Graphics.Color.DarkGray);
            lotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(lotChooser_ItemSelected);

            //var activity = this.Context as Activity;
            parkButton = FindViewById<Button>(Resource.Id.ParkMeButton);
            parkButton.Click += StartNavigation;

            ImageButton menubutton = FindViewById<ImageButton>(Resource.Id.MainMenuButton);
            menubutton.Click += delegate {
                StartActivity(typeof(MainMenuActivity));
            };
        }

        private void MClient_DownloadLotDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                string json1 = Encoding.UTF8.GetString(e.Result);
                lotNames = JsonConvert.DeserializeObject<string[]>(json1);
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotChooser.Adapter = adapter;
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
            catch (JsonReaderException)
            {
                Toast.MakeText(this, "There was an error parsing the data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        private void lotChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            lotIndex = e.Position;
        }

        public void StartNavigation(object sender, EventArgs e)
        {
            parkButton.Enabled = false;
            //this.StartActivity(typeof(FindNearestActivity));
            var startNavActiv = new Intent(this, typeof(FindNearestActivity));
            startNavActiv.PutExtra("lotIndex", lotIndex);
            startNavActiv.PutExtra("userToken",userToken);
            StartActivity(startNavActiv);
        }

        protected override void OnResume()
        {
            base.OnResume();
            parkButton.Enabled = true;
        }
    }
}

