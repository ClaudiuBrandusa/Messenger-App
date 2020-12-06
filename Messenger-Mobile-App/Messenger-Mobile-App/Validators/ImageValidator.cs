using System;

namespace Messenger_Mobile_App.Validators
{
    public class ImageValidator
    {
        public static string DefaultContactProfileImage = "default_user_image.png";
        public static string ValidateImageUrl(string url)
        {
            // we will just check if the url is not empty for now
            if (string.IsNullOrEmpty(url))
            {
                url = DefaultContactProfileImage;
            }
            return url;
        }
    }
}
