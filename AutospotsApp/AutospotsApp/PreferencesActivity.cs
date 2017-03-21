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
using Android.Preferences;
using Newtonsoft.Json;
using System.Net;

namespace AutospotsApp
{
    [Activity(Label = "PreferencesActivity")]
    public class PreferencesActivity : Activity
    {
        //TODO: Save preferences to disk
        Spinner lotChooser;
        string[] lotNames;
        WebClient mClient;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            float dps = this.Resources.DisplayMetrics.Density;
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            mainlayout.AddView(titleText);
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.PreferencesButtonText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            //ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText);
            mClient = new WebClient();
            TextView spinnerLabel = new TextView(this);
            spinnerLabel.Text = "Choose default lot:";
            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            mainlayout.AddView(spinnerLabel, lp);
            lotChooser = new Spinner(this);
            lotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(lotChooser_ItemSelected);
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadLotDataCompleted;
            mainlayout.AddView(lotChooser);
            // Create your application here

            SetContentView(mainlayout);
        }

        private void lotChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutInt("defaultlot", e.Position);
            editor.Apply();
        }

        private void MClient_DownloadLotDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try {
                string json1 = Encoding.UTF8.GetString(e.Result);
                lotNames = JsonConvert.DeserializeObject<string[]>(json1);
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotChooser.Adapter = adapter;
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }
    }
}