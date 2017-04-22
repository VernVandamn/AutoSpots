using System;
using System.Text;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using OxyPlot.Xamarin.Android;
using System.Net;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Android.Graphics;

namespace AutospotsApp
{
    [Activity(Label = "StatsActivity")]
    public class StatsActivity : Activity
    {
        PlotView pv;
        WebClient lotClient;
        WebClient statClient;
        Object[][] lotList;
        Spinner lotChooser;
        Spinner dayChooser;
        int lotIndex;
        int dayIndex;
        int[] lotIndices;
        Object[] theLot;
        float[] stats;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Calculate device-independent pixel size
            float dps = this.Resources.DisplayMetrics.Density;
            //Create main layout
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            //Create Autospots label for top left side of the screen
            TextView titleText = new TextView(this);
            //Load custom font
            Typeface tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
            titleText.Text = GetString(Resource.String.MainTitleText);
            titleText.SetTypeface(tf, TypefaceStyle.Normal);
            mainlayout.AddView(titleText);
            //Create main title text view
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.StatisticsMenuText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            menuTitleText.SetTypeface(tf, TypefaceStyle.Normal);
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            //Initialize web client for downloading lot list
            lotClient = new WebClient();
            //Initialize drop down menu
            lotChooser = new Spinner(this);
            //Initialize global lot index variable
            lotIndex = 0;
            //Download lot list
            lotClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            lotClient.DownloadDataCompleted += MClient_DownloadLotDataCompleted;
            lotChooser.SetPadding(0, 0, 0, (int)(20 * dps));
            mainlayout.AddView(lotChooser);
            //Create drop down menu for choosing the day of the week
            dayChooser = new Spinner(this);
            dayIndex = 0;
            //Create event handler for when a day of the week is chosen from drop down menu
            dayChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_DayItemSelected);
            //Put the days of the week in the day selector drop down menu
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.StatsDayArray, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            dayChooser.Adapter = adapter;
            dayChooser.SetPadding(0, 0, 0, (int)(20 * dps));
            mainlayout.AddView(dayChooser);
            //Create stat retrieval button
            Button retrieveButton = new Button(this);
            retrieveButton.Text = "OK";
            //Assign method to run on button click
            retrieveButton.Click += RetrieveButton_Click;
            mainlayout.AddView(retrieveButton);
            //Initialize statistics plot
            pv = new PlotView(this);
            mainlayout.AddView(pv);
            //Set main layour as the content view
            SetContentView(mainlayout);
        }

        private void RetrieveButton_Click(object sender, EventArgs e)
        {
            //Initialize web client for downloading stats data
            lotClient = new WebClient();
            //Download stats data
            lotClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/stats/" + lotIndex + "/" + dayIndex + "/"));
            lotClient.DownloadDataCompleted += MClient_DownloadStatDataCompleted;
        }

        private void spinner_DayItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //Set global day index
            dayIndex = e.Position;
        }

        private void MClient_DownloadLotDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                //Decode and deserialize lot list
                string json1 = Encoding.UTF8.GetString(e.Result);
                lotList = JsonConvert.DeserializeObject<Object[][]>(json1);
                string[] lotNames = new string[lotList.Length];
                for (int i = 0; i < lotList.Length; i++)
                {
                    lotNames[i] = (string)lotList[i][0];
                }
                //Put lot list in lot list drop down menu
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotChooser.Adapter = adapter;
                //Create event handler for selecting a lot
                lotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_LotItemSelected);
            }
            //Catch JSON errors
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
                Android.Util.Log.Debug("StatsActivity", "JSON parse exception");
            }
        }

        private void spinner_LotItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //Calculate the lot index from the lot list
            lotIndices = new int[lotList.Length];
            for (int i = 0; i < lotList.Length; i++)
            {
                lotIndices[i] = Convert.ToInt32(lotList[i][1]);
            }
            lotIndex = lotIndices[e.Position];
            theLot = lotList[e.Position];
        }

        private void MClient_DownloadStatDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            //Decode and deserialize statistics data
            string json = Encoding.UTF8.GetString(e.Result);
            stats = JsonConvert.DeserializeObject<float[]>(json);
            //Create model with statistics data
            pv.Model = CreateModel();
        }

        private PlotModel CreateModel()
        {
            //Decide which day of the week was selected and create a matching string
            string daystring = "";
            switch (dayIndex)
            {
                case 0:
                    daystring = "Monday";
                    break;
                case 1:
                    daystring = "Tuesday";
                    break;
                case 2:
                    daystring = "Wednesday";
                    break;
                case 3:
                    daystring = "Thursday";
                    break;
                case 4:
                    daystring = "Friday";
                    break;
            }
            //Setup objects to populate the statistics plot
            TimeSpan[] times = new TimeSpan[48];
            TimeSpan half = TimeSpan.FromMinutes(30);
            //Create an array for times to display on bottom of plot
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = TimeSpan.FromMinutes(30 * i);
            }
            //Create new model with title
            var plotModel = new PlotModel { Title = (string)theLot[0] + " Lot Capacity: " + daystring };
            //Add axes to the plot
            plotModel.Axes.Add(new TimeSpanAxis { Position = AxisPosition.Bottom, Maximum = TimeSpanAxis.ToDouble(times[47]), Minimum = TimeSpanAxis.ToDouble(times[0]) });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 1, Minimum = 0 });
            //Create a line
            var series1 = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White
            };
            //Add points to line
            for (int i = 0; i < stats.Length; i++)
            {
                series1.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(times[i]), stats[i]));
            }
            //Add line to plot
            plotModel.Series.Add(series1);
            //Return completed plot
            return plotModel;
        }
    }
}