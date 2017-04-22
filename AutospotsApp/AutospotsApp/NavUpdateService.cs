using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Util;
using Android.Locations;
using System.Net;
using Newtonsoft.Json;

namespace AutospotsApp
{
    /// <summary>
    /// This is the service that runs in the background while the user is following the navigation instructions to guide
    /// them closer to the lot. This service continually send the user's location back to the server until they are close
    /// enough to the parking lot to be assigned a parking spot. The service ends when the user is assigned a spot.
    /// </summary>
    [Service]
    class NavUpdateService : Service, ILocationListener,, IDialogInterfaceOnClickListener
    {
        LocationManager lmanager;
        static readonly int noteID = 49;
        NavUpdateServiceBinder binder;
        WebClient mClient;
        double startLat;
        double startLon;
        int userID;
        int lotID;
        LocationDirective response;

        public override void OnCreate()
        {
            base.OnCreate();
            //Create location manager
            lmanager = (LocationManager)GetSystemService(LocationService);
            //Make GPS provider our location provider
            string Provider = LocationManager.GpsProvider;
            //Request location updates
            lmanager.RequestLocationUpdates(Provider, 10000, 10, this);
            //Initialize web client for communicating with the server
            mClient = new WebClient();
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new NavUpdateServiceBinder(this);
            return binder;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            //If the location manager has been initialized...
            if (lmanager != null)
            {
                //Unsubscribe from location updates
                lmanager.RemoveUpdates(this);
            }
            //Stop Foreground notification
            StopForeground(true);
            Log.Debug("NavUpdateService", "NavUpdateService stopped.");
            //Stop this service
            StopService(new Intent(this, typeof(NavUpdateService)));
        }

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            //Parse location info sent to this service from calling activity to make sure there was no error
            Log.Debug("NavUpdateService", "NavUpdateService started");
            startLat = intent.GetDoubleExtra("startLat",-1);
            startLon = intent.GetDoubleExtra("startLon",-1);
            userID = intent.GetIntExtra("userID", -1);
            lotID = intent.GetIntExtra("lotID", -1);
            if (startLat == -1 || startLon == -1 || userID == -1 || lotID == -1)
            {
                Toast.MakeText(this,"Received bad info from initiating activity. Please try again.",ToastLength.Long).Show();
            }
            //Build foreground notification to show while service is running then show it
            var notification = new Notification.Builder(this)
                .SetContentTitle(Resources.GetString(Resource.String.ApplicationName))
                .SetContentText(GetString(Resource.String.ForeNotificationText))
                .SetSmallIcon(Resource.Drawable.AutospotsIcon24x24).SetOngoing(true)
                .Build();
            StartForeground(noteID, notification);
            return StartCommandResult.Sticky;
        }

        void ILocationListener.OnLocationChanged(Location location)
        {
            //Send location update to server and parse response
            double lat = location.Latitude;
            double lon = location.Longitude;
            Log.Debug("NavUpdateService", "Got location update: lat ="+lat+", lon="+lon+".");
            mClient.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/spot/coords/"+lat+"/"+lon+"/"+lotID+"/"+userID+"/"));
            mClient.DownloadDataCompleted += MClient_DownloadUpdateDataCompleted;

        }

        private void MClient_DownloadUpdateDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try {
                //Decode and deserialize server update
                string json = Encoding.UTF8.GetString(e.Result);
                Log.Debug("NavUpdateService", "Received json: " + json);
                response = JsonConvert.DeserializeObject<LocationDirective>(json);
                Log.Debug("NavUpdateService", "Converted json: " + response);
                //We got someone else's update
                if (response.userID != userID)
                {
                    Toast.MakeText(this, "Response from server was not meant for this user! Go back and try again.", ToastLength.Long).Show();
                    return;
                }
                //We got a response for another lot
                else if (response.lotID != lotID)
                {
                    Toast.MakeText(this, "Response from server was for wrong lot! Go back and try again.", ToastLength.Long).Show();
                    return;
                }
                //We're close enough to the lot and we've been assigned a spot!
                else if (response.latitude != startLat || response.longitude != startLon)
                {
                    //Parse response into a location then begin navigation to that spot
                    Log.Debug("NavUpdateService\\StartMapIntent", "Starting map intent with lat:" + response.latitude + ", long:" + response.longitude);
                    var geoUri = Android.Net.Uri.Parse("geo:" + response.latitude + "," + response.longitude + "?q=" + response.latitude + "," + response.longitude);
                    var mapIntent = new Intent(Intent.ActionView, geoUri);
                    Toast.MakeText(this, "Your spot has been assigned!", ToastLength.Long).Show();
                    StartActivity(mapIntent);
                    StopForeground(true);
                    lmanager = null;
                    StopService(new Intent(this, typeof(NavUpdateService)));
                }
                
            }
            //Catch JSON parse errors
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        void ILocationListener.OnProviderDisabled(string provider)
        {
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);
            alertDialogBuilder.SetMessage("GPS is disabled in your device. High accuracy GPS is required to use this feature.")
                .SetCancelable(false)
                .SetPositiveButton("Go to Settings to enable GPS", this);
            AlertDialog alert = alertDialogBuilder.Create();
            alert.Show();
        }

        void ILocationListener.OnProviderEnabled(string provider)
        {
            
        }

        void ILocationListener.OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            
        }

        void IDialogInterfaceOnClickListener.OnClick(IDialogInterface dialog, int which)
        {
            Intent callGPSSettingIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(callGPSSettingIntent);
        }
    }
}