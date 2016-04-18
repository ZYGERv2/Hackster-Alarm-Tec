using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.ServiceModel.Activation;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;
using Alarm_Tec_Service.Data;
using Alarm_Tec_Service.Models;
using System.Speech;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Alarm_Tec_Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AlarmTecService : IAlarmTecService
    {
        /// <summary>
        /// Authenticate User
        /// </summary>
        /// <param name="userStream"></param>
        /// <returns></returns>
        [WebInvoke(Method = "POST", UriTemplate = "Login", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public User Login(Stream userStream)
        {
            User user = null;
            try
            {
                StreamReader streamReader = new StreamReader(userStream);
                var streamStr = streamReader.ReadToEnd();
                user = new JavaScriptSerializer().Deserialize<User>(streamStr);

                user = AlarmTecDataManager.LoginUser(user);
            }
            catch (Exception ex)
            {
                //Email.ErrorLogEmail("RegisterUser: " + ex.Message);
            }

            return user;
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="userStream"></param>
        /// <returns></returns>
        [WebInvoke(Method = "POST", UriTemplate = "RegisterUser", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public bool RegisterUser(Stream userStream)
        {
            try
            {
                StreamReader streamReader = new StreamReader(userStream);
                var streamStr = streamReader.ReadToEnd();
                var user = new JavaScriptSerializer().Deserialize<User>(streamStr);

                // Validate that the password meets the required length; this is my preference and you can totally change it
                if (user.Password.Length < 6) return false;

                // Register user account
                return AlarmTecDataManager.RegisterUser(user);
            }
            catch (Exception ex)
            {
                // Log error message
                //Email.ErrorLogEmail("RegisterUser: " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Validate if an email has already been registered
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [WebGet(UriTemplate = "IsEmailRegistered/{email}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        public bool IsEmailRegistered(string email)
        {
            return AlarmTecDataManager.IsEmailRegistered(email);
        }

        /// <summary>
        /// This will add a new alarm to the user's queque for devices to pick up
        /// </summary>
        /// <param name="AddAlarm">Item Json Object Stream</param>
        [WebInvoke(Method = "POST", UriTemplate = "AddAlarm", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void AddAlarm(Stream alarmStream)
        {
            try
            {
                // Deserialize json string coming in to an Item
                var streamReader = new StreamReader(alarmStream);
                var streamStr = streamReader.ReadToEnd();
                var alarm = new JavaScriptSerializer().Deserialize<Alarm>(streamStr);

                if (!AlarmTecDataManager.AddAlarm(alarm)) throw new Exception("Failed to add new Alarm.");
            }
            catch (Exception ex)
            {
                // Log error message
                //Email.ErrorLogEmail("AddItem: " + ex.Message);
            }
        }

        [WebInvoke(Method = "POST", UriTemplate = "UpdateAlarm", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void UpdateAlarm(Stream alarmStream)
        {
            try
            {
                // Deserialize json string coming in to an Item
                var streamReader = new StreamReader(alarmStream);
                var streamStr = streamReader.ReadToEnd();
                var alarm = new JavaScriptSerializer().Deserialize<Alarm>(streamStr);

                if (!AlarmTecDataManager.UpdateAlarm(alarm)) throw new Exception("Failed to update Alarm.");
            }
            catch (Exception ex)
            {
                // Log error message
                //Email.ErrorLogEmail("AddItem: " + ex.Message);
            }
        }

        [WebGet(UriTemplate = "GetWeather?lat={lat}&lon={lon}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Stream GetWeather(string lat, string lon)
        {

            string url = "http://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&lon="+lon+
                         "&mode=xml&APPID=fac89f65b18f4a6c47596246f45b9359&units=imperial";
            var request = HttpWebRequest.Create(url);
            var web_result = request.GetResponse();
            var result = new StreamReader(web_result.GetResponseStream()).ReadToEnd();

            var temp = string.Empty;
            var tempMin = string.Empty;
            var tempMax = string.Empty;
            var clouds = string.Empty;
            var weather = string.Empty;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(result);
            XmlNodeList xnList = xml.SelectNodes("/current");
            foreach (XmlNode xn in xnList)
            {
                try
                {
                    temp = xn["temperature"].Attributes["value"].Value;
                    tempMin = xn["temperature"].Attributes["min"].Value;
                    tempMax = xn["temperature"].Attributes["max"].Value;

                    clouds = xn["clouds"].Attributes["name"].Value;
                    weather = xn["weather"].Attributes["value"].Value;
                }
                catch (Exception)
                {
                }
            }

            var weatherFeed = string.Format("Current weather is {0} degrees fahrenheit with a minimum of {1} degrees and max of {2} degrees. Expect {3} and {4} today.", temp, tempMin, tempMax, clouds, weather);
            return GenerateAudioFileFromText(weatherFeed);
        }

        private Stream GenerateAudioFileFromText(string weatherFeed)
        {

            var audioFile = DateTime.UtcNow.TimeOfDay + ".wav";
            var audioFilePath = Path.Combine(HostingEnvironment.MapPath("~/"), audioFile.Replace(':', '_'));

            var t = new System.Threading.Thread(() =>
            {
                SpeechSynthesizer _synthesizer = new SpeechSynthesizer();
                _synthesizer.SetOutputToWaveFile(audioFilePath);
                _synthesizer.Speak(weatherFeed);
                _synthesizer.Dispose();
            });
            t.Start();
            t.Join();
            
            String headerInfo = "attachment; filename=" + audioFile;
            WebOperationContext.Current.OutgoingResponse.Headers["Content-Disposition"] = headerInfo;
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/octet-stream";

            return File.OpenRead(audioFilePath);
        }

        [WebGet(UriTemplate = "GetNews", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Stream GetNews()
        {
            string url = "http://rss.cnn.com/rss/edition.rss";
            XmlReader reader = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            var i = 0;
            var newsFeed = "Todays top headlines...";
            foreach (SyndicationItem item in feed.Items)
            {
                if (i < 3)
                {
                    newsFeed += item.Title.Text + "...";
                }
                i++;
            }

            return GenerateAudioFileFromText(newsFeed);
        }

        [WebGet(UriTemplate = "GetNewsTest", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Stream GetNewsTest()
        {
            string url = "http://rss.cnn.com/rss/edition.rss";
            XmlReader reader = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            var i = 0;
            var newsFeed = "Todays top headlines...";
            foreach (SyndicationItem item in feed.Items)
            {
                if (i < 1)
                {
                    newsFeed += item.Title.Text + "...";
                }
                i++;
            }

            return GenerateAudioFileFromText(newsFeed);
        }

        [WebGet(UriTemplate = "GetAlarms?userId={userId}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public List<Alarm> GetAlarms(int userId)
        {
            return AlarmTecDataManager.GetAlarms(userId);
        }

        [WebGet(UriTemplate = "GetAlarm?alarmId={alarmId}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Alarm GetAlarm(int alarmId)
        {
            return AlarmTecDataManager.GetAlarm(alarmId);
        }

        [WebGet(UriTemplate = "RemoveAlarm?alarmId={alarmId}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public void RemoveAlarm(int alarmId)
        {
            AlarmTecDataManager.RemoveAlarm(alarmId);
        }
    }
}
