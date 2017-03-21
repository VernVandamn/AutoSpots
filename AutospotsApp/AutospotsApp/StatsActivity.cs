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
using OxyPlot.Xamarin.Android;
using System.Net;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AutospotsApp
{
    [Activity(Label = "StatsActivity")]
    public class StatsActivity : Activity
    {
        PlotView pv;
        WebClient lotClient;
        WebClient statClient;
        string[] lotNames;
        Spinner lotChooser;
        Spinner dayChooser;
        int lotIndex;
        int dayIndex;
        float[] stats;

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
            menuTitleText.Text = GetString(Resource.String.StatisticsMenuText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            lotClient = new WebClient();
            lotChooser = new Spinner(this);
            lotIndex = 0;
            lotClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/lots/"));
            lotClient.DownloadDataCompleted += MClient_DownloadLotDataCompleted;
            lotChooser.SetPadding(0, 0, 0, (int)(20 * dps));
            mainlayout.AddView(lotChooser);
            dayChooser = new Spinner(this);
            dayIndex = 0;
            dayChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_DayItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.StatsDayArray, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            dayChooser.Adapter = adapter;
            mainlayout.AddView(dayChooser);
            Button retrieveButton = new Button(this);
            retrieveButton.Text = "OK";
            retrieveButton.Click += RetrieveButton_Click;
            mainlayout.AddView(retrieveButton);

            pv = new PlotView(this);
            mainlayout.AddView(pv);
            statClient = new WebClient();
            //Show some statistic form


            SetContentView(mainlayout);
        }

        private void RetrieveButton_Click(object sender, EventArgs e)
        {
            lotClient = new WebClient();
            lotClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/stats/"+lotIndex+"/"+dayIndex+"/"));
            lotClient.DownloadDataCompleted += MClient_DownloadStatDataCompleted;
        }

        private void spinner_DayItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            dayIndex = e.Position;
        }

        private void MClient_DownloadLotDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try { 
                string json1 = Encoding.UTF8.GetString(e.Result);
                lotNames = JsonConvert.DeserializeObject<string[]>(json1);
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lotNames);
                lotChooser.Adapter = adapter;
                lotChooser.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_LotItemSelected);
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
                Android.Util.Log.Debug("StatsActivity", "JSON parse exception");
            }
        }

        private void spinner_LotItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            lotIndex = e.Position;
        }

        private void MClient_DownloadStatDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            //try { 
                string json = Encoding.UTF8.GetString(e.Result);
                stats = JsonConvert.DeserializeObject<float[]>(json);
                pv.Model = CreateModel();
            /*}
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }*/
        }

        private PlotModel CreateModel()
        {
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
            TimeSpan[] times = new TimeSpan[48];
            TimeSpan half = TimeSpan.FromMinutes(30);
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = TimeSpan.FromMinutes(30*i);
            }

            var plotModel = new PlotModel { Title = lotNames[lotIndex] +" Lot Capacity: "+daystring };

            plotModel.Axes.Add(new TimeSpanAxis { Position = AxisPosition.Bottom, Maximum = TimeSpanAxis.ToDouble(times[47]), Minimum = TimeSpanAxis.ToDouble(times[0])});
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 1, Minimum = 0 });

            var series1 = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White
            };
            for (int i = 0; i < stats.Length; i++)
            {
                series1.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(times[i]), stats[i]));
            }

            plotModel.Series.Add(series1);

            return plotModel;
        }
    }
}