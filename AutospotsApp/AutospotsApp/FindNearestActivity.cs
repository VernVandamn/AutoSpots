using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using System.Net.Sockets;
using System.Threading.Tasks;
using Android.Util;
using System.Threading;
using System.Net;
using Java.Util;
using Newtonsoft.Json;

namespace AutospotsApp
{
    [Activity(Label = "FindNearestActivity")]
    public class FindNearestActivity : Activity, ILocationListener
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
        //-----------------------
        //TcpClient client;
        //NetworkStream stream; //Creats a NetworkStream (used for sending and receiving data)
        //byte[] datalength = new byte[16]; // creates a new byte with length 4 ( used for receivng data's length)
        bool spotReceived;

        WebClient mclient;
        WebClient mClient1;
        Uri requestUrl;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main2);
            mClient1 = new WebClient();
            mClient1.DownloadDataAsync(new Uri("http://jamesljenk.pythonanywhere.com/getid/"));
            mClient1.DownloadDataCompleted += MClient_DownloadIdDataCompleted;
            statusText = FindViewById<TextView>(Resource.Id.ParkingStatusTextView);
            lotIndex = Intent.GetIntExtra("lotIndex",-1);
            //lotIndex = savedInstanceState.GetInt("lotIndex");
            if (lotIndex == -1)
            {
                statusText.Text = "You have not chosen a default lot. Please go back to the main menu and plan a trip or specify a default lot.";
                //Toast.MakeText(this, "You have not chosen a default lot. Please go back to the main menu and plan a trip or specify a default lot.",ToastLength.Long);
            }
            else
            { 
                spotReceived = false;
                mclient = new WebClient();
                InitializeLocationManager();
            }
        }

        private void MClient_DownloadIdDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                string json = Encoding.UTF8.GetString(e.Result);
                Log.Debug("GETID", "GETID json = " + json);
                UserId resp = JsonConvert.DeserializeObject<UserId>(json);
                userToken = resp.userID;
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + _locationProvider + ".");
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_locationManager != null && _locationProvider != null)
            {
                _locationManager.RequestLocationUpdates(_locationProvider, 5000, 10, this);//(_locationProvider, 0, 0, this);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (!spotReceived)
            {
                if (_locationManager != null)
                    _locationManager.RemoveUpdates(this);
                StopService(new Intent(this, typeof(NavUpdateService)));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Log.Debug("FindNearestActivity", "Destroying this activity");
            if (_locationManager != null)
                _locationManager.RemoveUpdates(this);
            StopService(new Intent(this,typeof(NavUpdateService)));
        }



        public void OnLocationChanged(Location location)
        {
            if (!spotReceived)
            {
                _currentLocation = location;
                if (_currentLocation == null)
                {
                    statusText.Text = "Unable to determine your location. Ensure your location service is enabled and try again in a short while.";
                }
                else
                {
                    statusText.Text = GetString(Resource.String.WaitingForSpotText);
                    //_locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
                    //Address address = await ReverseGeocodeCurrentLocation();
                    //DisplayAddress(address);
                    double lat = _currentLocation.Latitude;
                    double lon = _currentLocation.Longitude;
                    Log.Debug(TAG, "Got lat=" + lat + ", long=" + lon);
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
            //Toast.MakeText(this, "Connected", ToastLength.Short).Show();
            //Send lat,long to server
            //String s = String.Format("NEAREST_{0}_{1}", lat, lon);
            //clientSend(s);
            request = new SpotRequest();
            request.latitude = lat;
            request.longitude = lon;
            request.userID = userToken;
            request.lotID = lotIndex;
            //string json = request.ToString();
            //byte[] data = Encoding.ASCII.GetBytes(json);
            requestUrl = new Uri(request.ToString());
            while (mclient.IsBusy)
            {
                Thread.Sleep(1000);
            }
            Log.Debug("SpotRequest", "Sending SpotRequest: " + request.ToString());
            mclient.DownloadDataAsync(requestUrl);
            mclient.DownloadDataCompleted += Mclient_DownloadDataCompleted;
        }

        private void Mclient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e) //Received LocationDirective
        {
            try { 
                string json = Encoding.UTF8.GetString(e.Result);
                //string json = "{{\"latitude\": 40.769817, \"longitude\": -111.846035, \"lotID\": 0, \"userID\": 1}}";
                Log.Debug("ReceiveLocationDirective", "Received json: "+json);
                response = JsonConvert.DeserializeObject<LocationDirective>(json);
                Log.Debug("JsonConvertResponse", "Convert json: " + response);
                //if (response.userID != userToken)
                //{
                //    Toast.MakeText(this, "Response from server was not meant for this user! Go back and try again.", ToastLength.Long).Show();
                //    return;
                //}
                //else if (response.lotID != lotIndex) // WE ARE ASSUMING LOTINDEX == LocationDirective.lotID
                //{
                //    Toast.MakeText(this, "Response from server was for wrong lot! Go back and try again.", ToastLength.Long).Show();
                //    return;
                //}
                //else
                //{
                    spotReceived = true;
                    statusText.Text = GetString(Resource.String.LetsGoText);
                    StartNavService(response.latitude, response.longitude);
                    Log.Debug("StartMapIntent", "Starting map intent with lat:" + response.latitude + ", long:" + response.longitude);
                    var geoUri = Android.Net.Uri.Parse("geo:" + response.latitude + "," + response.longitude + "?z=21");
                    var mapIntent = new Intent(Intent.ActionView, geoUri);
                    StartActivity(mapIntent);
                //}
            }
            catch (System.Reflection.TargetInvocationException)
            {
                Toast.MakeText(this, "There was an error retrieving data from the server. Please close the app and try again later.", ToastLength.Long).Show();
            }
        }

        private void StartNavService(double startLat, double startLon)
        {
            Intent navUpIntent = new Intent(this, typeof(NavUpdateService));
            navUpIntent.PutExtra("startLat", startLat);
            navUpIntent.PutExtra("startLon", startLon);
            navUpIntent.PutExtra("userID", userToken);
            navUpIntent.PutExtra("lotID", lotIndex);
            StartService(navUpIntent);
        }

        /*private void clientReceive()
        {
            try
            {
                stream = client.GetStream(); //Gets The Stream of The Connection
                if (stream.CanRead)
                {
                    // Reads NetworkStream into a byte buffer.
                    byte[] bytes = new byte[client.ReceiveBufferSize];

                    // Read can return anything from 0 to numBytesToRead. 
                    // This method blocks until at least one byte is read.
                    stream.Read(bytes, 0, (int)client.ReceiveBufferSize);

                    // Returns the data received from the host to the console.
                    string returndata = Encoding.UTF8.GetString(bytes);

                    //string parkinginfo = "There are " + opens + " parking spaces open out of " + total + "!";

                    Application.SynchronizationContext.Post(_ => {
                        statusText.SetText(GetString(Resource.String.LetsGoText), TextView.BufferType.Normal);
                        this.StartNavService();
                    }, null);

                    //Start GPS navigation
                    Log.Debug(TAG, "Received \'" + returndata+"\' from server!");
                    if (returndata.Contains("SPOT"))
                        spotReceived = true;

                    returndata = returndata.Replace('\0',' ').Trim();
                    string[] info = returndata.Split('_');
                    string lon = info[1];
                    string lat = info[2];

                    var geoUri = Android.Net.Uri.Parse("geo:" +lat+","+lon+"?z=23");
                    var mapIntent = new Intent(Intent.ActionView, geoUri);
                    StartActivity(mapIntent);
                }
                else
                {
                    Console.WriteLine("You cannot read data from this stream.");
                    Toast.MakeText(this, "Can't connect to server", ToastLength.Short).Show();
                    client.Close();

                    // Closing the tcpClient instance does not close the network stream.
                    stream.Close();
                    return;
                }
                stream.Close();
            }
            catch (Exception ex)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                Log.Debug(TAG, ex.ToString());
            }
        }

        public void clientSend(string msg)
        {
            try
            {
                stream = client.GetStream(); //Gets The Stream of The Connection
                byte[] data; // creates a new byte without mentioning the size of it cuz its a byte used for sending
                data = Encoding.Default.GetBytes(msg); // put the msg in the byte ( it automaticly uses the size of the msg )
                int length = data.Length; // Gets the length of the byte data
                byte[] datalength = new byte[4]; // Creates a new byte with length of 4
                datalength = BitConverter.GetBytes(length); //put the length in a byte to send it
                stream.Write(datalength, 0, 4); // sends the data's length
                stream.Write(data, 0, data.Length); //Sends the real data
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
            }
        }*/

        public void OnProviderDisabled(string provider)
        {
            Toast.MakeText(this, GetString(Resource.String.LocationDisabledToastText), ToastLength.Long).Show();
            Intent i = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            StartActivity(i);
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            
        }
    }
}