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
        //-----------------------
        TcpClient client;
        NetworkStream stream; //Creats a NetworkStream (used for sending and receiving data)
        byte[] datalength = new byte[16]; // creates a new byte with length 4 ( used for receivng data's length)
        bool spotReceived;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main2);
            statusText = FindViewById<TextView>(Resource.Id.ParkingStatusTextView);
            spotReceived = false;
            //_locationText = FindViewById<TextView>(Resource.Id.location_text);
            //FindViewById<TextView>(Resource.Id.get_address_button).Click += AddressButton_OnClick;

            InitializeLocationManager();
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
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        async void AddressButton_OnClick(object sender, EventArgs eventArgs)
        {
            if (_currentLocation == null)
            {
                //_addressText.Text = "Can't determine the current address. Try again in a few minutes.";
                return;
            }

            Address address = await ReverseGeocodeCurrentLocation();
            //DisplayAddress(address);
        }

        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        public void OnLocationChanged(Location location)
        {
            if (!spotReceived)
            {
                _currentLocation = location;
                if (_currentLocation == null)
                {
                    //_locationText.Text = "Unable to determine your location. Try again in a short while.";
                }
                else
                {
                    statusText.Text = GetString(Resource.String.WaitingForSpotText);
                    //_locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
                    //Address address = await ReverseGeocodeCurrentLocation();
                    //DisplayAddress(address);
                    string lat = _currentLocation.Latitude.ToString();
                    string lon = _currentLocation.Longitude.ToString();
                    Log.Debug(TAG, "Got lat=" + lat + ", long=" + lon);
                    SendCurLocToServer(lat, lon);

                }
            }
        }

        void SendCurLocToServer(string lat,string lon)
        {
            try
            {
                client = new TcpClient("autospots.otzo.com", 22032);
            }
            catch (SocketException)
            {
                Toast.MakeText(this, "Connection Timed out. Returning to main menu.", ToastLength.Short).Show();
                base.OnBackPressed();
            }
            //Toast.MakeText(this, "Connected", ToastLength.Short).Show();
            //Send lat,long to server
            String s = String.Format("NEAREST_{0}_{1}", lat, lon);
            clientSend(s);
            //textReceive.Text = null;
            ThreadPool.QueueUserWorkItem(o => clientReceive()); //Starts Receiving When Connected
        }

        private void clientReceive()
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
        }

        public void OnProviderDisabled(string provider)
        {
            
        }

        public void OnProviderEnabled(string provider)
        {
            
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            
        }
    }
}