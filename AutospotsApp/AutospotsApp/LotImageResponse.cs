using System;

namespace AutospotsApp
{
    //JSON class for receiving lot images
    class LotImageResponse
    {
        public int lotID { get; set; }
        public string ImageBase64 { private get; set; }
        public byte[] Image
        {
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