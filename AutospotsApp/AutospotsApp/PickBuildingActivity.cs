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
    [Activity(Label = "PickBuildingActivity")]
    public class PickBuildingActivity : Activity
    {
        static readonly string TAG = "X:" + typeof(PickBuildingActivity).Name;
        int pos;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            // Create your application here
            float dps = Resources.DisplayMetrics.Density;
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            mainlayout.AddView(titleText);
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.NearBuildingButtonText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            //ViewGroup.LayoutParams ps1 = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            TextView buildingPickerLabel = new TextView(this);
            buildingPickerLabel.Text = GetString(Resource.String.SelectABuildingText);
            mainlayout.AddView(buildingPickerLabel);
            Spinner buildingChooser = new Spinner(this);
            //statChooser.SetBackgroundColor(Android.Graphics.Color.Black);
            buildingChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(buildingChooser_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.SelectBuildingArray, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            buildingChooser.Adapter = adapter;
            buildingChooser.SetPadding(0, 0, 0, 40 * (int)dps);
            mainlayout.AddView(buildingChooser);
            Button okButton = new Button(this);
            okButton.Text = "OK";
            okButton.Click += delegate{
                ClickOk();
            };
            
            mainlayout.AddView(okButton);
            SetContentView(mainlayout);
        }

        private void buildingChooser_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            pos = e.Position;
            Android.Util.Log.Debug(TAG,"List position "+pos+" chosen.");
        }

        private void ClickOk()
        {
            var nbActivity = new Intent(this, typeof(NearBuildingActivity));
            nbActivity.PutExtra("TargetBuilding", pos);
            StartActivity(nbActivity);
        }
    }
}