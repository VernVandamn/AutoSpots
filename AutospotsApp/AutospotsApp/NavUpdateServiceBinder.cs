using Android.OS;

namespace AutospotsApp
{
    //Class used by the NavUpdateService to bind to main activity
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