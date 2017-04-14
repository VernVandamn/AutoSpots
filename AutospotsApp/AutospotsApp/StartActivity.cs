using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System;

namespace AutospotsApp
{
    [Activity(Label = "AutospotsApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class StartActivity : Activity
    {
        Button parkButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            //var activity = this.Context as Activity;
            parkButton = FindViewById<Button>(Resource.Id.ParkMeButton);
            parkButton.Click += StartNavigation;

            ImageButton menubutton = FindViewById<ImageButton>(Resource.Id.MainMenuButton);
            menubutton.Click += delegate {
                StartActivity(typeof(MainMenuActivity));
            };
        }

        public void StartNavigation(object sender, EventArgs e)
        {
            parkButton.Enabled = false;
            this.StartActivity(typeof(FindNearestActivity));
        }

        protected override void OnResume()
        {
            base.OnResume();
            parkButton.Enabled = true;
        }
    }
}

