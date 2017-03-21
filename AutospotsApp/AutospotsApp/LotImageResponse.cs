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
using Newtonsoft.Json;

namespace AutospotsApp
{
    class LotImageResponse
    {
        public int lotID { get; set; }
        public string ImageBase64 { private get; set; }
        public byte[] Image
        {
            //get; set;
            get
            {
                if (ImageBase64 != null && ImageBase64 != "")
                {
                    byte[] image = Convert.FromBase64String(ImageBase64);
                    return image;
                }
                return null;
            }
        }
    }
}