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

namespace AutospotsApp
{
    class SpotRequest
    {
        public int userID { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int lotID { get; set; }

        public override string ToString() {
            return "http://jamesljenk.pythonanywhere.com/spot/coords/" + latitude + "/" + longitude + "/" + lotID + "/" +userID + "/";
        }
    }
}