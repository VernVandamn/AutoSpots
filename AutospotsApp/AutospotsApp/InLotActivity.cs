//using System;
//using System.Text;
//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Views;
//using Android.Widget;
//using System.Net.Sockets;
//using Android.Graphics;

//namespace AutospotsApp
//{
//    [Activity(Label = "InLotActivity")]
//    public class InLotActivity : Activity
//    {
//        static readonly string TAG = "X:" + typeof(InLotActivity).Name;
//        TextView statusText;

//        protected override void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);
//            RequestWindowFeature(WindowFeatures.NoTitle);
//            //
//            int lotIndex = Intent.GetIntExtra("TargetLot", 0);
//            float dps = this.Resources.DisplayMetrics.Density;
//            LinearLayout mainLayout = new LinearLayout(this);
//            mainLayout.Orientation = Orientation.Vertical;
//            TextView titleText = new TextView(this);
//            titleText.Text = GetString(Resource.String.MainTitleText);
//            mainLayout.AddView(titleText);
//            TextView menuTitleText = new TextView(this);
//            Typeface tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
//            menuTitleText.Text = GetString(Resource.String.InLotButtonText);
//            menuTitleText.Gravity = GravityFlags.Center;
//            menuTitleText.TextSize = 30.0f;
//            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
//            menuTitleText.SetTypeface(tf, TypefaceStyle.Normal);
//            mainLayout.AddView(menuTitleText, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
//            statusText = new TextView(this);
//            statusText.Text = GetString(Resource.String.WaitingForSpotText);
//            mainLayout.AddView(statusText);
//            SetContentView(mainLayout);
//            SendSpotRequest(lotIndex);
//        }

//        private void SendSpotRequest(int lotIndex)
//        {
//            var nearact = new Intent(this, typeof(FindNearestActivity));
//            nearact.PutExtra("lotIndex", lotIndex);
//            StartActivity(nearact);
//        }
//    }
//}