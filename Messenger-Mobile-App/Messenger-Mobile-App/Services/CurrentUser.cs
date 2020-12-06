using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Services
{
    public class CurrentUser // just a model for now
    {
        User User { get; set; }
        Credential Credential { get; set; }
        bool loggedIn;

        public CurrentUser()
        {
            User = new User();
            Credential = new Credential();
        }

        public bool Register()
        {
            if(!ValidateUserName(User.Name))
            {
                return false;
            }
            if(!ValidatePassword(Credential.Password))
            {
                return false;
            }
            if(!ValidateEmail(User.Email))
            {
                return false;
            }

            // Here we should do the HTTP request to the API

            // will auto login after registration
            return Login();
        }

        public bool Login() // It should return false if something went wrong
        {
            if(!ValidateUserName(User.Name))
            {
                return false;
            }
            if (!ValidatePassword(Credential.Password))
            {
                return false;
            }

            // Here we should do the HTTP request to the API

            LoadProfileImage();

            loggedIn = true;
            return true;
        }

        public bool Logout() // It should return false if something went wrong
        {
            User = new User();
            Credential = new Credential();

            // Here we should do the HTTP request to the API

            loggedIn = false;
            return true;
        }

        public bool IsLoggedIn()
        {
            return loggedIn; // hard coded for now
        }

        public User GetUser()
        {
            return User;
        }

        // Profile image

        public void LoadProfileImage()
        {
            //User.ImgUrl = "profile1.jpg";
        }

        // Username Part

        public void SetUserName(string username)
        {
            if(!ValidateUserName(username))
            {
                return;
            }

            User.Name = username;
        }

        public static bool ValidateUserName(string username)
        {
            return UsernameValidator.Check(username);
        }

        public bool MatchUserName(string username)
        {
            return User.Name.Equals(username);
        }

        // Password Part

        public void SetPassword(string password)
        {
            if(!ValidatePassword(password))
            {
                return;
            }

            Credential.Password = password;
        }

        public static bool ValidatePassword(string password)
        {
            return PasswordValidator.Check(password);
        }

        public bool MatchPassword(string possiblePassword)
        {
            return Credential.Password.Equals(possiblePassword);
        }

        // Email Part

        public void SetEmail(string email)
        {
            if (!ValidateEmail(email))
            {
                return;
            }

            User.Email = email;
        }

        public static bool ValidateEmail(string email)
        {
            return EmailValidator.Check(email);
        }

        public bool MatchEmail(string email)
        {
            return User.Email.Equals(email);
        }
    }
}
