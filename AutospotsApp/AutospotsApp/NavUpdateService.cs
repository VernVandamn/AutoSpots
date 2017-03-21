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
using Android.Locations;
using System.Net;
using Newtonsoft.Json;

namespace AutospotsApp
{
    [Service]
    class NavUpdateService : Service, ILocationListener
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

            lmanager = (LocationManager)GetSystemService(LocationService);
            string Provider = LocationManager.GpsProvider;
            lmanager.RequestLocationUpdates(Provider, 10000, 10, this);
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
            if (lmanager != null)
            {
                lmanager.RemoveUpdates(this);
            }
            StopForeground(true);
            Log.Debug("NavUpdateService", "NavUpdateService stopped.");
            StopService(new Intent(this, typeof(NavUpdateService))); //Infinite loop????
        }

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug("NavUpdateService", "NavUpdateService started");
            startLat = intent.GetDoubleExtra("startLat",-1);
            startLon = intent.GetDoubleExtra("startLon",-1);
            userID = intent.GetIntExtra("userID", -1);
            lotID = intent.GetIntExtra("lotID", -1);
            if (startLat == -1 || startLon == -1 || userID == -1 || lotID == -1)
            {
                Toast.MakeText(this,"Received bad info from initiating activity. Please try again.",ToastLength.Long).Show();
            }
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
                string json = Encoding.UTF8.GetString(e.Result);
                Log.Debug("NavUpdateService", "Received json: " + json);
                response = JsonConvert.DeserializeObject<LocationDirective>(json);
                Log.Debug("NavUpdateService", "Converted json: " + response);
                if (response.userID != userID)
                {
                    Toast.MakeText(this, "Response from server was not meant for this user! Go back and try again.", ToastLength.Long).Show();
                    return;
                }
                else if (response.lotID != lotID)
                {
                    Toast.MakeText(this, "Response from server was for wrong lot! Go back and try again.", ToastLength.Long).Show();
                    return;
                }
                else if (response.latitude != startLat || response.longitude != startLon)
                {
                    Log.Debug("NavUpdateService\\StartMapIntent", "Starting map intent with lat:" + response.latitude + ", long:" + response.longitude);
                    var geoUri = Android.Net.Uri.Parse("geo:" + response.latitude + "," + response.longitude + "?z=21");
                    var mapIntent = new Intent(Intent.ActionView, geoUri);
                    Toast.MakeText(this, "Your spot has been assigned!", ToastLength.Long).Show();
                    StartActivity(mapIntent);
                    StopForeground(true);
                    lmanager = null;
                    StopService(new Intent(this, typeof(NavUpdateService)));
                }
                
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        void ILocationListener.OnProviderDisabled(string provider)
        {
            Toast.MakeText(this, GetString(Resource.String.LocationDisabledToastText), ToastLength.Long).Show();
            Intent i = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(i);
        }

        void ILocationListener.OnProviderEnabled(string provider)
        {
            
        }

        void ILocationListener.OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            
        }
    }
}