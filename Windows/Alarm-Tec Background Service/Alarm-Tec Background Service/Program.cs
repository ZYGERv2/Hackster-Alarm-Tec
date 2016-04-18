using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alarm_Tec_Background_Service.Models;
using Newtonsoft.Json;

namespace Alarm_Tec_Background_Service
{
    class Program
    {
        static void Main(string[] args)
        {
            float longitude = 0.0f;
            float latitude = 0.0f;

            SerialPort serialPort = new SerialPort("COM7");
            serialPort.Open();
            while (true)
            {
                string read = serialPort.ReadLine();
                Console.WriteLine(read);

                if (read.Contains("GPS"))
                {
                    //get gps coordinates
                    Console.WriteLine("Getting GPS");
                    var latitudeAndLongitudeData = read.Replace("GPS:","").Split(',');
                    if (latitudeAndLongitudeData.Length > 0)
                    {
                        Console.WriteLine("Got GPS");
                        var latitudeStr = latitudeAndLongitudeData[0].Replace("latitude=", "");
                        var longitudeStr = latitudeAndLongitudeData[1].Replace("longitude=", "");
                        latitude = float.Parse(latitudeStr);
                        longitude = float.Parse(longitudeStr);
                    }
                }

                Console.WriteLine("Getting JSON");
                if (read.Contains("{") && read.Contains("}"))
                {
                    Console.WriteLine("Got JSON");
                    var alarm = JsonConvert.DeserializeObject<Alarm>(read);

                    if (!string.IsNullOrWhiteSpace(alarm.WakeUpTime) && DateTime.Parse(alarm.WakeUpTime).TimeOfDay<=DateTime.Now.TimeOfDay)
                    {
                        if (alarm.WantsWeather)
                        {
                            if (longitude != 0 && latitude != 0)
                            {
                                string url = string.Format("http://alarmtecservice.cloudapp.net/AlarmTecService/GetWeather?lat={0}&lon={1}", latitude, longitude);
                                using (var client = new WebClient())
                                {
                                    client.DownloadFile(url, "weather.wav");
                                }
                                System.Media.SoundPlayer player = new System.Media.SoundPlayer("weather.wav");
                                player.PlaySync();
                            }
                        }
                        
                        if (alarm.WantsNews)
                        {
                            string url = "http://alarmtecservice.cloudapp.net/AlarmTecService/GetNews";
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(url, "news.wav");
                            }
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer("news.wav");
                            player.PlaySync();
                        }
                    }
                }
            }
        }
    }
}
