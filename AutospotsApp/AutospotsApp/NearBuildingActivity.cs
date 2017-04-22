//using System;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Views;
//using Android.Widget;
//using Android.Util;
//using System.Net.Sockets;
//using System.Threading;

//namespace AutospotsApp
//{
//    [Activity(Label = "NearBuildingActivity")]
//    public class NearBuildingActivity : Activity
//    {
//        static readonly string TAG = "X:" + typeof(NearBuildingActivity).Name;
//        TextView statusText;
//        TcpClient client;
//        NetworkStream stream; //Creats a NetworkStream (used for sending and receiving data)
//        byte[] datalength = new byte[16]; // creates a new byte with length 4 ( used for receivng data's length)

//        protected override void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);
//            RequestWindowFeature(WindowFeatures.NoTitle);
//            int buildingIndex = Intent.GetIntExtra("TargetBuilding",0);
//            float dps = this.Resources.DisplayMetrics.Density;
//            LinearLayout mainLayout = new LinearLayout(this);
//            mainLayout.Orientation = Orientation.Vertical;
//            TextView titleText = new TextView(this);
//            titleText.Text = GetString(Resource.String.MainTitleText);
//            mainLayout.AddView(titleText);
//            TextView menuTitleText = new TextView(this);
//            menuTitleText.Text = GetString(Resource.String.NearBuildingButtonText);
//            menuTitleText.Gravity = GravityFlags.Center;
//            menuTitleText.TextSize = 30.0f;
//            menuTitleText.SetPadding(0, (int)(20 * dps), 0, (int)(30 * dps));
//            mainLayout.AddView(menuTitleText, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
//            statusText = new TextView(this);
//            statusText.Text = GetString(Resource.String.WaitingForSpotText);
//            mainLayout.AddView(statusText);
//            SetContentView(mainLayout);
//            SendSpotRequest(buildingIndex);
//        }

//        private void SendSpotRequest(int buildingIndex)
//        {
//            try
//            {
//                client = new TcpClient("autospots.otzo.com", 22032);
//            }
//            catch (SocketException)
//            {
//                Toast.MakeText(this, "Connection Timed out. Returning to main menu.", ToastLength.Short).Show();
//                base.OnBackPressed();
//            }
//            clientSend("BUILDING_"+buildingIndex);
//            ThreadPool.QueueUserWorkItem(o => clientReceive()); //Starts Receiving When Connected
//        }


//        private void clientReceive()
//        {
//            try
//            {
//                stream = client.GetStream(); //Gets The Stream of The Connection
//                if (stream.CanRead)
//                {
//                    // Reads NetworkStream into a byte buffer.
//                    byte[] bytes = new byte[client.ReceiveBufferSize];

//                    // Read can return anything from 0 to numBytesToRead. 
//                    // This method blocks until at least one byte is read.
//                    stream.Read(bytes, 0, (int)client.ReceiveBufferSize);

//                    // Returns the data received from the host to the console.
//                    string returndata = Encoding.UTF8.GetString(bytes);

//                    //string parkinginfo = "There are " + opens + " parking spaces open out of " + total + "!";

//                    Application.SynchronizationContext.Post(_ => {
//                        statusText.SetText(GetString(Resource.String.LetsGoText), TextView.BufferType.Normal);
//                    }, null);

//                    //Start GPS navigation
//                    Log.Debug(TAG, "Received \'" + returndata + "\' from server!");
//                    if (returndata.Contains("SPOT"))
//                    { }
//                    else
//                    {
//                        Application.SynchronizationContext.Post(_ =>
//                        {
//                            statusText.SetText("Got back response from server. Please try again.", TextView.BufferType.Normal);
//                        }, null);
//                    }

//                    returndata = returndata.Replace('\0', ' ').Trim();
//                    string[] info = returndata.Split('_');
//                    string lon = info[1];
//                    string lat = info[2];

//                    var geoUri = Android.Net.Uri.Parse("geo:" + lat + "," + lon + "?z=23");
//                    var mapIntent = new Intent(Intent.ActionView, geoUri);
//                    StartActivity(mapIntent);
//                }
//                else
//                {
//                    Console.WriteLine("You cannot read data from this stream.");
//                    Toast.MakeText(this, "Can't connect to server", ToastLength.Short).Show();
//                    client.Close();

//                    // Closing the tcpClient instance does not close the network stream.
//                    stream.Close();
//                    return;
//                }
//                stream.Close();
//            }
//            catch (Exception ex)
//            {
//                //Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
//                Log.Debug(TAG, ex.ToString());
//            }
//        }

//        public void clientSend(string msg)
//        {
//            try
//            {
//                stream = client.GetStream(); //Gets The Stream of The Connection
//                byte[] data; // creates a new byte without mentioning the size of it cuz its a byte used for sending
//                data = Encoding.Default.GetBytes(msg); // put the msg in the byte ( it automaticly uses the size of the msg )
//                int length = data.Length; // Gets the length of the byte data
//                byte[] datalength = new byte[4]; // Creates a new byte with length of 4
//                datalength = BitConverter.GetBytes(length); //put the length in a byte to send it
//                stream.Write(datalength, 0, 4); // sends the data's length
//                stream.Write(data, 0, data.Length); //Sends the real data
//            }
//            catch (Exception ex)
//            {
//                Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
//            }
//        }
//    }
//}