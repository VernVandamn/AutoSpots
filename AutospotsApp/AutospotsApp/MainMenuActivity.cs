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
using System.Net.Sockets;

namespace AutospotsApp
{
    [Activity(Label = "MainMenuActivity")]
    public class MainMenuActivity : Activity
    {
        static readonly string TAG = "X:" + typeof(MainMenuActivity).Name;
        TcpClient client;
        NetworkStream stream; //Creats a NetworkStream (used for sending and receiving data)
        byte[] datalength = new byte[16]; // creates a new byte with length 4 ( used for receivng data's length)
        bool spotReceived;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            // Create your application here
            spotReceived = false;
            float dps = this.Resources.DisplayMetrics.Density;
            LinearLayout mainlayout = new LinearLayout(this);
            mainlayout.Orientation = Orientation.Vertical;
            TextView titleText = new TextView(this);
            titleText.Text = GetString(Resource.String.MainTitleText);
            mainlayout.AddView(titleText);
            TextView menuTitleText = new TextView(this);
            menuTitleText.Text = GetString(Resource.String.MenuText);
            menuTitleText.Gravity = GravityFlags.Center;
            menuTitleText.TextSize = 30.0f;
            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
            ViewGroup.LayoutParams ps = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            //ViewGroup.LayoutParams ps1 = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            mainlayout.AddView(menuTitleText, ps);
            Button nearestParkingButton = new Button(this);
            nearestParkingButton.Text = GetString(Resource.String.NearestSpotButtonText);
            nearestParkingButton.Gravity = GravityFlags.CenterHorizontal;
            nearestParkingButton.Click += delegate {
                StartActivity(typeof(FindNearestActivity));
            };
            nearestParkingButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(nearestParkingButton, ps);
            Button nearBuildingButton = new Button(this);
            nearBuildingButton.Text = GetString(Resource.String.NearBuildingButtonText);
            nearBuildingButton.Gravity = GravityFlags.CenterHorizontal;
            nearBuildingButton.Click += delegate {
                StartActivity(typeof(NearBuildingActivity));
            };
            nearBuildingButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(nearBuildingButton, ps);
            Button inLotButton = new Button(this);
            inLotButton.Text = GetString(Resource.String.PlanTripButtonText);
            inLotButton.Gravity = GravityFlags.CenterHorizontal;
            inLotButton.Click += delegate {
                StartActivity(typeof(InLotActivity));
            };
            inLotButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(inLotButton, ps);
            Button planTripButton = new Button(this);
            planTripButton.Text = GetString(Resource.String.PlanTripButtonText);
            planTripButton.Gravity = GravityFlags.CenterHorizontal;
            planTripButton.Click += delegate{ 
                StartActivity(typeof(PlanTripActivity));
            };
            planTripButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(planTripButton, ps);
            Button viewLotsButton = new Button(this);
            viewLotsButton.Text = GetString(Resource.String.ViewLotsButtonText);
            viewLotsButton.Gravity = GravityFlags.CenterHorizontal;
            viewLotsButton.Click += delegate {
                StartActivity(typeof(ViewLotActivity));
            };
            viewLotsButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(viewLotsButton, ps);
            Button statsButton = new Button(this);
            statsButton.Text = GetString(Resource.String.StatisticsButtonText);
            statsButton.Gravity = GravityFlags.CenterHorizontal;
            statsButton.Click += delegate {
                StartActivity(typeof(StatsActivity));
            };
            statsButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(statsButton, ps);
            Button preferencesButton = new Button(this);
            preferencesButton.Text = GetString(Resource.String.PreferencesButtonText);
            preferencesButton.Gravity = GravityFlags.CenterHorizontal;
            preferencesButton.Click += delegate {
                StartActivity(typeof(PreferencesActivity));
            };
            preferencesButton.SetPadding(0, (int)(20 * dps), 0, (int)(20 * dps));
            mainlayout.AddView(preferencesButton,ps);
            SetContentView(mainlayout);
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

                    /*Application.SynchronizationContext.Post(_ => {
                        statusText.SetText(GetString(Resource.String.LetsGoText), TextView.BufferType.Normal);
                    }, null);*/

                    //Start GPS navigation
                    Log.Debug(TAG, "Received \'" + returndata + "\' from server!");
                    if (returndata.Contains("SPOT"))
                        spotReceived = true;
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
                Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
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

    }
}