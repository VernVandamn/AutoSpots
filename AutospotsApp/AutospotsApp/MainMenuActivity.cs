using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Preferences;
using Android.Graphics;

namespace AutospotsApp
{
    [Activity(Label = "MainMenuActivity")]
    public class MainMenuActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Initialize stuff to save app-wide preferences
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            //Calculate device-independent pixel size
            float dps = this.Resources.DisplayMetrics.Density;
            //Create a new layout
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            //Set background image
            mainlayout.Background = GetDrawable(Resource.Drawable.streets1);
            //Load custom font
            Typeface tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
            //Create Autospots label in top left of the screen and add it to the main layout
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            titleText.SetTextColor(Android.Graphics.Color.Black);
            titleText.SetTypeface(tf, TypefaceStyle.Normal);
            mainlayout.AddView(titleText);
            //Create Main Menu text label, format it and put it in the layout
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.MenuText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.SetTextSize(ComplexUnitType.Dip, 35);
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            menuTitleText.SetTextColor(Android.Graphics.Color.Black);
            menuTitleText.SetTypeface(tf, TypefaceStyle.Normal);
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            //Create button to park in preferred lot
            Button nearestParkingButton = new Button(this);
            nearestParkingButton.Text = GetString(Resource.String.NearestSpotButtonText);
            nearestParkingButton.Gravity = GravityFlags.CenterHorizontal;
            //Assign delegate method to run on button click
            nearestParkingButton.Click += delegate {
                int lotInd = prefs.GetInt("defaultlot",-1);
                var nearact = new Intent(this, typeof(FindNearestActivity));
                nearact.PutExtra("lotIndex", lotInd);
                StartActivity(nearact);
            };
            nearestParkingButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(nearestParkingButton, ps);
            //Create button to bring up the menu to select a parking lot to park in
            Button inLotButton = new Button(this);
            inLotButton.Text = GetString(Resource.String.InLotButtonText);
            inLotButton.Gravity = GravityFlags.CenterHorizontal;
            //Assign delegate method to run on button click
            inLotButton.Click += delegate {
                StartActivity(typeof(PickLotActivity));
            };
            inLotButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(inLotButton, ps);
            // Create button to bring up the menu to view most recent parking lot images
            Button viewLotsButton = new Button(this);
            viewLotsButton.Text = GetString(Resource.String.ViewLotsButtonText);
            viewLotsButton.Gravity = GravityFlags.CenterHorizontal;
            //Assign delegate method to run on button click
            viewLotsButton.Click += delegate {
                StartActivity(typeof(ViewLotActivity));
            };
            viewLotsButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(viewLotsButton, ps);
            //Create button to bring up the menu to view parking lot capacity stats
            Button statsButton = new Button(this);
            statsButton.Text = GetString(Resource.String.StatisticsButtonText);
            statsButton.Gravity = GravityFlags.CenterHorizontal;
            //Assign delegate method to run on button click
            statsButton.Click += delegate {
                StartActivity(typeof(StatsActivity));
            };
            statsButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(statsButton, ps);
            //Create button to bring up the menu to choose a preferred lot
            Button preferencesButton = new Button(this);
            preferencesButton.Text = GetString(Resource.String.PreferencesButtonText);
            preferencesButton.Gravity = GravityFlags.CenterHorizontal;
            //Assign delegate method to run on button click
            preferencesButton.Click += delegate {
                StartActivity(typeof(PreferencesActivity));
            };
            preferencesButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(preferencesButton,ps);
            //Set view as the main layout
            SetContentView(mainlayout);
        }
    }
}