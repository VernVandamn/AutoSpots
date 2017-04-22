using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using Android.Content;
using Android.Util;
using Android.Graphics;

namespace AutospotsApp
{
    [Activity(Label = "AutospotsApp", MainLauncher = true, Icon = "@drawable/autospotsicon")]
    public class StartActivity : Activity
    {
        int userToken;
        Button parkButton;
        Spinner lotChooser;
        int lotIndex;
        WebClient mClient;
        WebClient mClient1;
        Object[][] lotList;
        Typeface tf;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            //If the NavUpdate service somehow stayed running, kill it
            StopService(new Intent(this, typeof(NavUpdateService))); //JUST IN CASE

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            //Intialize web clients for downloading info from the server
            mClient = new WebClient();
            mClient1 = new WebClient();
            //Download lot list
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadLotDataCompleted;
            //Load custom font
            tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
            //Change the main title text view font to custom font and make it bigger
            TextView mttv = FindViewById<TextView>(Resource.Id.MainTitleTextView);
            mttv.SetTypeface(tf,TypefaceStyle.Normal);
            mttv.SetTextSize(ComplexUnitType.Dip,60);
            lotIndex = 0;
            //Change color of drop down menu
            lotChooser = FindViewById<Spinner>(Resource.Id.LotPickSpinner);
            lotChooser.SetBackgroundColor(Color.DarkGray);
            //Create event handler for menu item selection
            lotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(lotChooser_ItemSelected);
            //Assign method to run on button click
            parkButton = FindViewById<Button>(Resource.Id.ParkMeButton);
            parkButton.Click += StartNavigation;
            //Assign delegate method to run on button click
            ImageButton menubutton = FindViewById<ImageButton>(Resource.Id.MainMenuButton);
            menubutton.Click += delegate {
                StartActivity(typeof(MainMenuActivity));
            };
        }

        private void MClient_DownloadLotDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                //Decode and deserialize lot list
                string json1 = Encoding.UTF8.GetString(e.Result);
                lotList = JsonConvert.DeserializeObject<Object[][]>(json1);
                string[] lotNames = new string[lotList.Length];
                for (int i = 0; i < lotList.Length; i++)
                {
                    lotNames[i] = (string)lotList[i][0];
                }
                //Put lot list in drop down menu
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotChooser.Adapter = adapter;
            }
            //Catch JSON errors
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
            //Calculate lot index of selected lot and assign it to a global variable
            int[] lotIndices = new int[lotList.Length];
            for (int i = 0; i < lotList.Length; i++)
            {
                lotIndices[i] = Convert.ToInt32(lotList[i][1]);
            }
            lotIndex = lotIndices[e.Position];
        }

        public void StartNavigation(object sender, EventArgs e)
        {
            //Disable park button so there's no extra clicks
            parkButton.Enabled = false;
            //Start activity to get location and go
            var startNavActiv = new Intent(this, typeof(FindNearestActivity));
            startNavActiv.PutExtra("lotIndex", lotIndex);
            startNavActiv.PutExtra("userToken",userToken);
            StartActivity(startNavActiv);
        }

        protected override void OnResume()
        {
            base.OnResume();
            //Reenable the park button
            parkButton.Enabled = true;
        }
    }
}

