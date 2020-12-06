using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.Validators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<bool> RegisterAsync()
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

            // HTTP request to the API

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                    Register model = new Register
                    {
                        UserName = User.Name,
                        Email = User.Email,
                        Password = Credential.Password,
                        ConfirmPassword = Credential.Password
                    };

                    string json = JsonConvert.SerializeObject(model);
                    var ip = "10.0.2.2";
                    Dictionary<string, string> body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    using (var response = await httpClient.PostAsync(String.Format("http://{0}:49499/api/authentication/register", ip), new FormUrlEncodedContent(body)))
                    {
                        if (!response.StatusCode.ToString().Equals("OK"))
                        {
                            //return false; Ignoring the request for now
                        }
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                //return false; Ignoring the connection errors for now
            }

            // will auto login after registration
            return await LoginAsync();
        }

        public async Task<bool> LoginAsync() // It should return false if something went wrong
        {
            if(!ValidateUserName(User.Name))
            {
                return false;
            }
            if (!ValidatePassword(Credential.Password))
            {
                return false;
            }

            // HTTP request to the API
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                    Register model = new Register
                    {
                        UserName = User.Name,
                        Email = User.Email,
                        Password = Credential.Password,
                        ConfirmPassword = Credential.Password
                    };

                    string json = JsonConvert.SerializeObject(model);
                    var ip = "10.0.2.2";
                    Dictionary<string, string> body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    using (var response = await httpClient.PostAsync(String.Format("http://{0}:49499/api/authentication/login", ip), new FormUrlEncodedContent(body)))
                    {
                        if (!response.StatusCode.ToString().Equals("OK"))
                        {
                            //return false; Ignoring the request for now
                        }
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                //return false; Ignoring the connection errors for now
            }

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
