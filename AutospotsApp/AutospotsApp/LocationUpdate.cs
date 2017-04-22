namespace AutospotsApp
{
    //JSON class for sending user's current location to the server.
    //User's location is used to determine if user is close enough to be assigned a spot
    class LocationUpdate
    {
        public int userID { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int lotID { get; set; }

        public override string ToString()
        {
            return "http://jamesljenk.pythonanywhere.com/spot/coords/" + latitude + "/" + longitude + "/" + lotID + "/"+userID + "/";
        }
    }
}