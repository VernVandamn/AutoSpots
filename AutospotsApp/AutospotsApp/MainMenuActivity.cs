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
using Android.Util;
using System.Net.Sockets;

namespace AutospotsApp
{
    [Activity(Label = "MainMenuActivity")]
    public class MainMenuActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            // Create your application here
            float dps = this.Resources.DisplayMetrics.Density;
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            mainlayout.AddView(titleText);
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.MenuText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            //ViewGroup.LayoutParams ps1 = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            Button nearestParkingButton = new Button(this);
            nearestParkingButton.Text = GetString(Resource.String.NearestSpotButtonText);
            nearestParkingButton.Gravity = GravityFlags.CenterHorizontal;
            nearestParkingButton.Click += delegate {
                StartActivity(typeof(FindNearestActivity));
            };
            nearestParkingButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(nearestParkingButton, ps);
            Button nearBuildingButton = new Button(this);
            nearBuildingButton.Text = GetString(Resource.String.NearBuildingButtonText);
            nearBuildingButton.Gravity = GravityFlags.CenterHorizontal;
            nearBuildingButton.Click += delegate {
                StartActivity(typeof(PickBuildingActivity));
            };
            nearBuildingButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(nearBuildingButton, ps);
            Button inLotButton = new Button(this);
            inLotButton.Text = GetString(Resource.String.InLotButtonText);
            inLotButton.Gravity = GravityFlags.CenterHorizontal;
            inLotButton.Click += delegate {
                StartActivity(typeof(PickLotActivity));
            };
            inLotButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(inLotButton, ps);
            Button planTripButton = new Button(this);
            planTripButton.Text = GetString(Resource.String.PlanTripButtonText);
            planTripButton.Gravity = GravityFlags.CenterHorizontal;
            planTripButton.Click += delegate{ 
                StartActivity(typeof(PlanTripActivity));
            };
            planTripButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(planTripButton, ps);
            Button viewLotsButton = new Button(this);
            viewLotsButton.Text = GetString(Resource.String.ViewLotsButtonText);
            viewLotsButton.Gravity = GravityFlags.CenterHorizontal;
            viewLotsButton.Click += delegate {
                StartActivity(typeof(ViewLotActivity));
            };
            viewLotsButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(viewLotsButton, ps);
            Button statsButton = new Button(this);
            statsButton.Text = GetString(Resource.String.StatisticsButtonText);
            statsButton.Gravity = GravityFlags.CenterHorizontal;
            statsButton.Click += delegate {
                StartActivity(typeof(StatsActivity));
            };
            statsButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(statsButton, ps);
            Button preferencesButton = new Button(this);
            preferencesButton.Text = GetString(Resource.String.PreferencesButtonText);
            preferencesButton.Gravity = GravityFlags.CenterHorizontal;
            preferencesButton.Click += delegate {
                StartActivity(typeof(PreferencesActivity));
            };
            preferencesButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(preferencesButton,ps);
            SetContentView(mainlayout);
        }
    }
}