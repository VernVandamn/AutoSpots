using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net;
using Newtonsoft.Json;
using Android.Graphics;

namespace AutospotsApp
{
    [Activity(Label = "PickLotActivity")]
    public class PickLotActivity : Activity
    {
        int pos;
        WebClient mClient;
        Object[][] lotList;
        Spinner parkingLotChooser;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Initialize web client for downloading the list of lots
            mClient = new WebClient();
            //Download list of lots
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadDataCompleted;
            //Calculate device-independent pixel size
            float dps = Resources.DisplayMetrics.Density;
            //Create a main layout
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            //Load custom font
            Typeface tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
            //Create Autospots label for top left corner of the screen
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            titleText.SetTypeface(tf, TypefaceStyle.Normal);
            mainlayout.AddView(titleText);
            //Create menu text label
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.InLotButtonText);
            menuTitleText.SetTypeface(tf, TypefaceStyle.Normal);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            //Create a label for the drop down menu
            TextView buildingPickerLabel = new TextView(this);
            buildingPickerLabel.Text = GetString(Resource.String.SelectAParkingLotText);
            mainlayout.AddView(buildingPickerLabel);
            //Initialize the drop down menu of lots
            parkingLotChooser = new Spinner(this);
            parkingLotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(parkingLotChooser_ItemSelected);
            parkingLotChooser.SetPadding(0, 0, 0, (int)(40 * dps));
            mainlayout.AddView(parkingLotChooser);
            //Create the OK button
            Button okButton = new Button(this);
            okButton.Text = "OK";
            //Assign delegate method to run on button click
            okButton.Click += delegate {
                ClickOk();
            };
            mainlayout.AddView(okButton);
            //Set main layout to be the content view
            SetContentView(mainlayout);
        }

        //Download lot list
        private void MClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try {
                //Decode and deserialize lot list
                string json = Encoding.UTF8.GetString(e.Result);
                lotList = JsonConvert.DeserializeObject<Object[][]>(json);
                string[] lotNames = new string[lotList.Length];
                for (int i = 0; i < lotList.Length; i++)
                {
                    lotNames[i] = (string)lotList[i][0];
                }
                //Put lot list in the drop down menu
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                parkingLotChooser.Adapter = adapter;
            }
            //JSON parse error
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        private void parkingLotChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //Figure out lot index from the lot list
            int[] lotIndices = new int[lotList.Length];
            for (int i = 0; i < lotList.Length; i++)
            {
                lotIndices[i] = Convert.ToInt32(lotList[i][1]);
            }
            pos = lotIndices[e.Position];
        }

        private void ClickOk()
        {
            //Start activity to get location and go
            var nbActivity = new Intent(this, typeof(FindNearestActivity));
            nbActivity.PutExtra("lotIndex", pos);
            StartActivity(nbActivity);
        }
    }
}