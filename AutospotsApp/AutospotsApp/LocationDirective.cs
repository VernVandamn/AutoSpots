namespace AutospotsApp
{
    //JSON class for receiving locations from the server
    class LocationDirective
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int lotID { get; set; }
        public int userID { get; set; }
    }
}