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
    [Activity(Label = "ViewLotActivity")]
    public class ViewLotActivity : Activity
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
            menuTitleText.Text = GetString(Resource.String.ViewLotsButtonText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            //ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText);

            // Create your application here

            SetContentView(mainlayout);
        }
    }
}