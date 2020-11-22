using System;

namespace Messenger_Mobile_App.Services
{
    public class ImageValidator
    {
        public static string DefaultContactProfileImage = "default_user_image.png";
        public static string ValidateImageUrl(string url)
        {
            // we will just check if the url is not empty for now
            if(String.IsNullOrEmpty(url))
            {
                url = DefaultContactProfileImage;
            }
            return url;
        }
    }
}
