namespace AutospotsApp
{
    //JSON object for requesting parking spots
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