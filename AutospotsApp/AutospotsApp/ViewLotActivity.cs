using System;
using System.Text;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net;
using Newtonsoft.Json;
using Android.Graphics;

namespace AutospotsApp
{
    [Activity(Label = "ViewLotActivity")]
    public class ViewLotActivity : Activity//, ScaleGestureDetector.IOnScaleGestureListener
    {
        int lotIndex;
        WebClient mClient;
        WebClient mClient1;
        Object[][] lotList;
        Spinner lotSelector;
        //ScaleGestureDetector sgd;
        ImageView lotView;
        //float lastTouchX;
        //float lastTouchY;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Initialize web clients for downloading data from server
            mClient = new WebClient();
            mClient1 = new WebClient();
            //Download lot list
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            mClient.DownloadDataCompleted += MClient_DownloadLotListCompleted;
            //sgd = new ScaleGestureDetector(this, this);
            //Calculate device-independent pixels
            float dps = this.Resources.DisplayMetrics.Density;
            //Create main layout
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            //Load custom font
            Typeface tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
            //Create Autospots label for top left side of the screen
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            titleText.SetTypeface(tf, TypefaceStyle.Normal);
            mainlayout.AddView(titleText);
            //Create main title text label
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.ViewLotsButtonText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            menuTitleText.SetTypeface(tf, TypefaceStyle.Normal);
            mainlayout.AddView(menuTitleText);
            //Initialize lot list drop down menu
            lotSelector = new Spinner(this);
            lotSelector.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(lotSelector_ItemSelected);
            lotSelector.SetPadding(0, 0, 0, (int)(25* dps));
            mainlayout.AddView(lotSelector);
            //Initialize image view to show lot image
            lotView = new ImageView(this);
            lotView.SetScaleType(ImageView.ScaleType.Center);
            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            mainlayout.AddView(lotView,lp);
            //Set main layout to be content view
            SetContentView(mainlayout);
        }

        private void MClient_DownloadLotListCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                //Decode and deserialize lot list
                string json = Encoding.UTF8.GetString(e.Result);
                lotList = JsonConvert.DeserializeObject<Object[][]>(json);
                string[] lotNames = new string[lotList.Length];
                for (int i = 0; i < lotList.Length; i++)
                {
                    lotNames[i] = (string)lotList[i][0];
                }
                //Put lot list in lot list drop down menu
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotSelector.Adapter = adapter;
            }
            //Catch JSON errors
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        private void lotSelector_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //Calculate lot index of selected lot
            int[] lotIndices = new int[lotList.Length];
            for (int i = 0; i < lotList.Length; i++)
            {
                lotIndices[i] = Convert.ToInt32(lotList[i][1]);
            }
            lotIndex = lotIndices[e.Position];
            //Download image of selected lot
            mClient1.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/image/"+lotIndex+"/"));
            mClient1.DownloadDataCompleted += MClient_DownloadImageResponseCompleted;
        }

        private void MClient_DownloadImageResponseCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                //Decode and deserialize lot image
                string json = Encoding.UTF8.GetString(e.Result);
                LotImageResponse li = JsonConvert.DeserializeObject<LotImageResponse>(json);
                //Turn image into a bit map
                Bitmap imbm = BitmapFactory.DecodeByteArray(li.Image, 0, li.Image.Length);
                imbm = imbm.Copy(Bitmap.Config.Argb8888, true);
                //Figure out dimensions of image
                AndroidBitmapInfo inf = imbm.GetBitmapInfo();
                //Figure out largest possible size image could be on screen
                int screenwidth = this.Resources.DisplayMetrics.WidthPixels;
                int heightpx = (screenwidth * (int)inf.Height) / (int)inf.Width;
                //Create a scaled up version of the bitmap
                Bitmap bm = Bitmap.CreateScaledBitmap(imbm, screenwidth, heightpx, false);
                bm.Width = screenwidth;
                bm.Height = heightpx;
                //Set image view as the drawable for the Image view
                lotView.SetImageBitmap(bm);
            }
            //Catch JSON errors
            catch (System.Reflection.TargetInvocationException err)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
                Android.Util.Log.Debug("ViewLotActivity", err.GetBaseException().ToString());
                Android.Util.Log.Debug("ViewLotActivity", err.GetType().ToString());
                Android.Util.Log.Debug("ViewLotActivity", err.Message);
            }
        }

        //Couldn't get zooming to work
        /*public override bool OnTouchEvent(MotionEvent ev)
        {
            float y = ev.GetY();
            if (y >= lotView.GetY() && y <= (lotView.GetY() + lotView.Height))
            {
                Android.Util.Log.Debug("ViewLotActivity", "Detected touch on lotview. Set lasttouchx and y.");
                lastTouchX = ev.GetX();
                lastTouchY = ev.GetY();
            }
            return true;
        }

        
        bool ScaleGestureDetector.IOnScaleGestureListener.OnScale(ScaleGestureDetector detector)
        {
            lotView.PivotX = lastTouchX;
            lotView.PivotY = lastTouchY;
            lotView.ScaleX = detector.ScaleFactor;
            lotView.ScaleY = detector.ScaleFactor;
            return true;
        }

        bool ScaleGestureDetector.IOnScaleGestureListener.OnScaleBegin(ScaleGestureDetector detector)
        {
            return true;
        }

        void ScaleGestureDetector.IOnScaleGestureListener.OnScaleEnd(ScaleGestureDetector detector)
        {
            
        }*/
    }
}