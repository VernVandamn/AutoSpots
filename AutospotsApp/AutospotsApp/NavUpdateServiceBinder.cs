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
    class NavUpdateServiceBinder : Binder
    {
        NavUpdateService service;

        public NavUpdateServiceBinder(NavUpdateService service)
        {
            this.service = service;
        }

        public NavUpdateService GetDemoService()
        {
            return service;
        }

    }
}