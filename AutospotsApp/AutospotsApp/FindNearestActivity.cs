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
using Android.Locations;
using Android.Util;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using Android.Graphics;

namespace AutospotsApp
{
    [Activity(Label = "FindNearestActivity")]
    public class FindNearestActivity : Activity, ILocationListener, IDialogInterfaceOnClickListener
    {

        static readonly string TAG = "X:" + typeof(FindNearestActivity).Name;
        TextView statusText;
        Location _currentLocation;
        LocationManager _locationManager;
        string _locationProvider;
        int userToken;
        int lotIndex;
        LocationDirective response;
        SpotRequest request;
        AlertDialog alert;
        bool spotReceived;
        WebClient mclient;
        WebClient mClient1;
        Uri requestUrl;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main2);
            //Load custom font
            Typeface tf = Typeface.CreateFromAsset(Assets, "transformersmovie.ttf");
            TextView titleText = FindViewById<TextView>(Resource.Id.TitleTextView);
            //Set font of Autospots label in top left corner
            titleText.SetTypeface(tf, TypefaceStyle.Normal);
            TextView parkMeTitleText = FindViewById<TextView>(Resource.Id.ParkMeTitleTextView);
            //Set font of Title text
            parkMeTitleText.SetTypeface(tf, TypefaceStyle.Normal);
            mClient1 = new WebClient();
            //Download an ID from the server
            mClient1.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/getid/"));
            mClient1.DownloadDataCompleted += MClient_DownloadIdDataCompleted;
            //Assign the status text to a variable so it can be changed later
            statusText = FindViewById<TextView>(Resource.Id.ParkingStatusTextView);
            //Figure out which lot we're parking in
            lotIndex = Intent.GetIntExtra("lotIndex",-1);
            //If the user hasn't chosen a lot to park in...
            if (lotIndex == -1)
            {
                //Display error text
                statusText.Text = "You have not chosen a default lot. Please go back to the main menu and select \"Find spot in specific lot\" option or specify a default lot in the Preferences menu.";
            }
            //Parking lot has been chosen
            else
            { 
                spotReceived = false;
                //Initialize web client used to download location coordinates
                mclient = new WebClient();
                //Start Location services
                InitializeLocationManager();
            }
        }

        //Build and display an alert to ask user to turn on GPS setting
        private void showDisableGPSAlert()
        {
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);
            alertDialogBuilder.SetMessage("GPS is disabled in your device. High accuracy GPS is required to use this feature.")
                .SetCancelable(false)
                .SetPositiveButton("Go to Settings to enable GPS", this);
            alert = alertDialogBuilder.Create();
            alert.Show();
        }

        private void MClient_DownloadIdDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                //Assign downloaded data to a string
                string json = Encoding.UTF8.GetString(e.Result);
                Log.Debug("GETID", "GETID json = " + json);
                //Deserialize string from JSON
                UserId resp = JsonConvert.DeserializeObject<UserId>(json);
                //Assign downloaded ID to global variable
                userToken = resp.userID;
            }
            //JSON error catch
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        bool InitializeLocationManager()
        {
            //Assign a handle to the location service
            _locationManager = (LocationManager)GetSystemService(LocationService);
            //If GPS is turned off...
            if (!_locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                //Show error
                showDisableGPSAlert();
                return false;
            }
            //GPS is on
            else
            {
                //Define fine granularity of GPS
                Criteria criteriaForLocationService = new Criteria
                {
                    Accuracy = Accuracy.Fine
                };
                //Get list of acceptable location providers
                IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);
                //If there are any acceptable location providers
                if (acceptableLocationProviders.Any())
                {
                    //Assign the first location provider as the location provider we will use
                    _locationProvider = acceptableLocationProviders.First();
                }
                //There are no acceptable location providers
                else
                {
                    //No acceptable location provider
                    _locationProvider = string.Empty;
                }
                Log.Debug(TAG, "Using " + _locationProvider + ".");
                //Report that location services are started
                return true;
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                //If the location manager has been initialized
                if (_locationManager != null)
                {
                    //Assign a new location provider
                    _locationProvider = _locationManager.GetProviders(new Criteria{Accuracy = Accuracy.Fine}, true).First();
                    //Request location updates
                    _locationManager.RequestLocationUpdates(_locationProvider, 5000, 10, this);//(_locationProvider, 0, 0, this);
                }
            }
            catch (Exception e)
            {
                Log.Debug("FindNearestActivity", e.ToString());
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            //If we haven't received a spot yet...
            if (!spotReceived)
            {
                //If location manager has been initialized...
                if (_locationManager != null)
                    //Stop location updates
                    _locationManager.RemoveUpdates(this);
                //Stop background GPS monitoring service
                StopService(new Intent(this, typeof(NavUpdateService)));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //If the No GPS alert has been created...
            if (alert != null)
                //Destroy it to avoid memory leaks
                alert = null;
            Log.Debug("FindNearestActivity", "Destroying this activity");
            //If location manager has been initialized...
            if (_locationManager != null)
                //Remove GPS updates
                _locationManager.RemoveUpdates(this);
            //Stop background GPS monitoring service
            StopService(new Intent(this,typeof(NavUpdateService)));
        }

        public void OnLocationChanged(Location location)
        {
            //If we haven't received a spot yet...
            if (!spotReceived)
            {
                // Assign location to local variable
                _currentLocation = location;
                //If location is null...
                if (_currentLocation == null)
                {
                    //Something bad happened and we didn't actually get a location.
                    statusText.Text = "Unable to determine your location. Ensure your location service is enabled and try again in a short while.";
                }
                //We received a non-null location
                else
                {
                    //Update status text
                    statusText.Text = GetString(Resource.String.WaitingForSpotText);
                    //Extract latitude and longitude of received location
                    double lat = _currentLocation.Latitude;
                    double lon = _currentLocation.Longitude;
                    Log.Debug(TAG, "Got lat=" + lat + ", long=" + lon);
                    //Send current location to server
                    SendCurLocToServer(lat, lon);

                }
            }
            else //I have received a spot and the location has updated
            {
                //Do I do anything? I don't think so. The service should take care of this
            }
        }

        void SendCurLocToServer(double lat,double lon)
        {
            //Initialize global spot request variable and fill in details
            request = new SpotRequest();
            request.latitude = lat;
            request.longitude = lon;
            request.userID = userToken;
            request.lotID = lotIndex;
            requestUrl = new Uri(request.ToString());
            //Check to make sure web client isn't still sending something
            while (mclient.IsBusy)
            {
                Thread.Sleep(1000);
            }
            Log.Debug("SpotRequest", "Sending SpotRequest: " + request.ToString());
            //Request spot from server
            mclient.DownloadDataAsync(requestUrl);
            mclient.DownloadDataCompleted += Mclient_DownloadDataCompleted;
        }

        private void Mclient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e) //Received LocationDirective
        {
            try
            {
                //Received JSONified Location Directive
                string json = Encoding.UTF8.GetString(e.Result);
                Log.Debug("ReceiveLocationDirective", "Received json: " + json);
                //Deserialize data
                response = JsonConvert.DeserializeObject<LocationDirective>(json);
                Log.Debug("JsonConvertResponse", "Convert json: " + response);
                //Check to see we didn't receive someone else's spot
                if (response.userID != userToken)
                {
                    Toast.MakeText(this, "Response from server was not meant for this user! Go back and try again.", ToastLength.Long).Show();
                    return;
                }
                //Check to see if we got the wrong lot
                else if (response.lotID != lotIndex) // WE ARE ASSUMING LOTINDEX == LocationDirective.lotID
                {
                    Toast.MakeText(this, "Response from server was for wrong lot! Go back and try again.", ToastLength.Long).Show();
                    return;
                }
                //Everything looks ok with our parking spot assignment
                else
                {
                    //Flip spot received global flag
                    spotReceived = true;
                    //Server sends back (0,0) if a lot is full, check if the lot's full
                    if (response.latitude != 0 && response.longitude != 0)
                    {
                        //Lot's not full, start GPS navigation to the spot!
                        statusText.Text = GetString(Resource.String.LetsGoText);
                        StartNavService(response.latitude, response.longitude);
                        Log.Debug("StartMapIntent", "Starting map intent with lat:" + response.latitude + ", long:" + response.longitude);
                        var geoUri = Android.Net.Uri.Parse("geo:" + response.latitude + "," + response.longitude + "?q="+ response.latitude+","+ response.longitude);
                        var mapIntent = new Intent(Intent.ActionView, geoUri); //"geo:<lat>,<long>?q=<lat>,<long>"
                        StartActivity(mapIntent);
                    }
                    //Lot is full
                    else
                    {
                        //Break the bad news
                        statusText.Text = "Looks like this lot is full! Please go back to the main menu and try a different lot.";
                    }
                }
            }
            //Catch JSON error
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        //Helper method to start up navigation
        private void StartNavService(double startLat, double startLon)
        {
            Intent navUpIntent = new Intent(this, typeof(NavUpdateService));
            navUpIntent.PutExtra("startLat", startLat);
            navUpIntent.PutExtra("startLon", startLon);
            navUpIntent.PutExtra("userID", userToken);
            navUpIntent.PutExtra("lotID", lotIndex);
            StartService(navUpIntent);
        }

        public void OnProviderDisabled(string provider)
        {
            //Prompt user to turn GPS back on
            showDisableGPSAlert();
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            
        }

        //Opens GPS settings when alert button is pressed
        void IDialogInterfaceOnClickListener.OnClick(IDialogInterface dialog, int which)
        {
            Intent callGPSSettingIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(callGPSSettingIntent);
        }
    }
}