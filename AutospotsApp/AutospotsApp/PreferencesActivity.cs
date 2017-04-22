using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Newtonsoft.Json;
using System.Net;
using Android.Graphics;

namespace AutospotsApp
{
    [Activity(Label = "PreferencesActivity")]
    public class PreferencesActivity : Activity
    {
        Spinner lotChooser;
        Object[][] lotList;
        WebClient mClient;
        ISharedPreferences prefs;
        ISharedPreferencesEditor editor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Calculate device-independent pixel size
            float dps = this.Resources.DisplayMetrics.Density;
            //Initialize object for managing app-wide preferences
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            //Create main layout
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            //Load custom font
            Typeface tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
            //Create Autospots label for top left corner
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            titleText.SetTypeface(tf, TypefaceStyle.Normal);
            mainlayout.AddView(titleText);
            //Create menu title text label
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.PreferencesButtonText);
            menuTitleText.SetTypeface(tf, TypefaceStyle.Normal);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 40.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            mainlayout.AddView(menuTitleText);
            //Initialize web client for downloading lot list
            mClient = new WebClient();
            //Create text label for drop down menu
            TextView spinnerLabel = new TextView(this);
            spinnerLabel.Text = "Choose default lot:";
            spinnerLabel.TextSize = 20.0f;
            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            mainlayout.AddView(spinnerLabel, lp);
            //Initialize the drop down menu
            lotChooser = new Spinner(this);
            lotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(lotChooser_ItemSelected);
            //Dowload lot list
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadLotDataCompleted;
            mainlayout.AddView(lotChooser);
            //Set main layout as content view
            SetContentView(mainlayout);
        }

        private void lotChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //Create preferences editor object
            editor = prefs.Edit();
            //Calculate lot index
            int[] lotIndices = new int[lotList.Length];
            for (int i = 0; i < lotList.Length; i++)
            {
                lotIndices[i] = Convert.ToInt32(lotList[i][1]);
            }
            //Save chosen lot as default lot
            editor.PutInt("defaultlot", lotIndices[e.Position]);
            editor.Apply();
        }

        private void MClient_DownloadLotDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try {
                //Decode and deserialize lot list
                string json1 = Encoding.UTF8.GetString(e.Result);
                lotList = JsonConvert.DeserializeObject<Object[][]>(json1);
                string[] lotNames = new string[lotList.Length];
                for (int i = 0; i < lotList.Length; i++)
                {
                    lotNames[i] = (string)lotList[i][0];
                }
                //FIll the drop down menu with the lot list
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotChooser.Adapter = adapter;
                //Show the current preferred lot as selected on the drop down menu
                int lotInd = prefs.GetInt("defaultlot", -1);
                int ind = 0;
                for (int i = 0; i < lotList.Length; i++)
                {
                    if (Convert.ToInt32(lotList[i][1]) == lotInd)
                        ind = i;
                }
                lotChooser.SetSelection(ind);
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }
    }
}