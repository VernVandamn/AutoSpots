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

namespace AutospotsApp
{
    [Activity(Label = "PlanTripActivity")]
    public class PlanTripActivity : Activity
    {
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
            menuTitleText.Text = GetString(Resource.String.PlanTripMenuText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText);
            // Create your application here
            TextView selectBuildingMenuLabel = new TextView(this);
            selectBuildingMenuLabel.Text = GetString(Resource.String.SelectABuildingText);
            mainlayout.AddView(selectBuildingMenuLabel);
            Spinner buildingChooser = new Spinner(this);
            //statChooser.SetBackgroundColor(Android.Graphics.Color.Black);
            buildingChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(buildingChooser_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.SelectBuildingArray, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            buildingChooser.Adapter = adapter;
            mainlayout.AddView(buildingChooser);
            TextView orLabel = new TextView(this);
            orLabel.Text = GetString(Resource.String.or);
            orLabel.SetPadding(0, (int)(30 * dps), 0, (int)(30 * dps));
            mainlayout.AddView(orLabel);
            TextView selectParkingLotMenuLabel = new TextView(this);
            selectParkingLotMenuLabel.Text = GetString(Resource.String.SelectAParkingLotText);
            mainlayout.AddView(selectParkingLotMenuLabel);
            Spinner parkingLotChooser = new Spinner(this);
            //statChooser.SetBackgroundColor(Android.Graphics.Color.Black);
            parkingLotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(parkingLotChooser_ItemSelected);
            var adapter2 = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.SelectParkingLotArray, Android.Resource.Layout.SimpleSpinnerItem);

            adapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            parkingLotChooser.Adapter = adapter2;
            mainlayout.AddView(parkingLotChooser);
            SetContentView(mainlayout);
        }

        private void buildingChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            //string toast = string.Format("The planet is {0}", spinner.GetItemAtPosition(e.Position));
            //Toast.MakeText(this, toast, ToastLength.Long).Show();
        }

        private void parkingLotChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            //string toast = string.Format("The planet is {0}", spinner.GetItemAtPosition(e.Position));
            //Toast.MakeText(this, toast, ToastLength.Long).Show();
        }
    }
}