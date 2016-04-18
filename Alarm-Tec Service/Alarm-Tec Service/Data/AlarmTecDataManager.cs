using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alarm_Tec_Service.Models;
using static System.Web.Security.FormsAuthentication;

namespace Alarm_Tec_Service.Data
{
    public class AlarmTecDataManager
    {
        public static User LoginUser(User user)
        {
            using (var dbContext = new AlarmTecDataDataContext())
            {
                var hashedPassword = HashPasswordForStoringInConfigFile(user.Password, "MD5");
                var userDb = dbContext.AlarmTec_Users.ToList()
                    .FirstOrDefault(
                        g =>
                            string.Equals(g.UserEmail.ToLower(), user.Email.ToLower()) &&
                            string.Equals(g.UserPassword, hashedPassword));
                if (userDb != null)
                {
                    user.Id = userDb.UserId;
                    user.CreateDate = userDb.UserCreateDate.ToString();
                }
            }
            return user;
        }

        public static bool RegisterUser(User user)
        {
            try
            {
                using (var dbContext = new AlarmTecDataDataContext())
                {
                    var userDb = new AlarmTec_User()
                    {
                        UserEmail = user.Email,
                        UserPassword = HashPasswordForStoringInConfigFile(user.Password, "MD5"),
                        UserCreateDate = DateTime.UtcNow
                    };

                    dbContext.AlarmTec_Users.InsertOnSubmit(userDb);
                    dbContext.SubmitChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error message
            }
            return false;
        }

        public static bool IsEmailRegistered(string email)
        {
            bool result = false;
            try
            {
                using (var dbContext = new AlarmTecDataDataContext())
                {
                    result = dbContext.AlarmTec_Users.ToList().FirstOrDefault(g => string.Equals(g.UserEmail.ToLower(), email.ToLower())) != null;
                }
            }
            catch (Exception ex)
            {
                // Log error message
                //Email.ErrorLogEmail($"IsEmailRegistered {ex.Message} - {ex.StackTrace}");
            }
            return result;
        }

        public static bool AddAlarm(Alarm alarm)
        {
            try
            {
                using (var dbContext = new AlarmTecDataDataContext())
                {
                    // Create a new db Item object to store into the queue
                    var itemDb = new AlarmTec_Alarm()
                    {
                        AlarmIsEnabled = alarm.IsEnabled,
                        UserId = alarm.UserId,
                        AlarmWantsWeather = alarm.WantsWeather,
                        AlarmWantsNews = alarm.WantsNews,
                        AlarmWakeUpTime = alarm.WakeUpTime,
                        AlarmCreateDate = DateTime.UtcNow
                    };

                    // Insert our newly created Item and Submit the change to the db
                    dbContext.AlarmTec_Alarms.InsertOnSubmit(itemDb);
                    dbContext.SubmitChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error message
            }
            return false;
        }

        public static bool UpdateAlarm(Alarm alarm)
        {
            try
            {
                using (var dbContext = new AlarmTecDataDataContext())
                {
                    // Create a new db Item object to store into the queue
                    var alarmDb = dbContext.AlarmTec_Alarms.FirstOrDefault(i => i.AlarmId == alarm.Id);

                    if (alarmDb != null)
                    {
                        alarmDb.AlarmName = alarm.Name;
                        alarmDb.AlarmIsEnabled = alarm.IsEnabled;
                        alarmDb.AlarmWakeUpTime = alarm.WakeUpTime;
                        alarmDb.AlarmWantsNews = alarm.WantsNews;
                        alarmDb.AlarmWantsWeather = alarm.WantsWeather;
                    }

                    dbContext.SubmitChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error message
            }
            return false;
        }

        public static Alarm GetAlarm(int alarmId)
        {
            try
            {
                var alarm = new Alarm();
                using (var dbContext = new AlarmTecDataDataContext())
                {
                    var alarmDb = dbContext.AlarmTec_Alarms.FirstOrDefault(i => i.AlarmId == alarmId);

                    if (alarmDb != null)
                    {
                        alarm = new Alarm()
                        {
                            UserId = alarmDb.UserId,
                            Name = alarmDb.AlarmName,
                            Id = alarmDb.AlarmId,
                            CreateDate = alarmDb.AlarmCreateDate.ToString(),
                            WakeUpTime = alarmDb.AlarmWakeUpTime,
                            WantsWeather = alarmDb.AlarmWantsWeather ?? false,
                            WantsNews = alarmDb.AlarmWantsNews?? false,
                            IsEnabled = alarmDb.AlarmIsEnabled??false
                        };
                    }
                }
                return alarm;
            }
            catch (Exception ex)
            {
                // Log error message
            }
            return null;
        }

        public static List<Alarm> GetAlarms(int userId)
        {
            try
            {
                var alarms = new List<Alarm>();
                using (var dbContext = new AlarmTecDataDataContext())
                {
                    dbContext.AlarmTec_Alarms.Where(i => i.UserId == userId).ToList().ForEach(alarmDb =>
                    {
                        if (alarmDb != null)
                        {
                            alarms.Add(new Alarm()
                            {
                                UserId = alarmDb.UserId,
                                Name = alarmDb.AlarmName,
                                Id = alarmDb.AlarmId,
                                CreateDate = alarmDb.AlarmCreateDate.ToString(),
                                WakeUpTime = alarmDb.AlarmWakeUpTime,
                                WantsWeather = alarmDb.AlarmWantsWeather??false,
                                WantsNews = alarmDb.AlarmWantsNews??false,
                                IsEnabled = alarmDb.AlarmIsEnabled??false
                            });
                        }
                    });
                }
                return alarms;
            }
            catch (Exception ex)
            {
                // Log error message
            }
            return null;
        }

        public static void RemoveAlarm(int alarmId)
        {
            using (var dbContext = new AlarmTecDataDataContext())
            {
                var alarms = dbContext.AlarmTec_Alarms.Where(i => i.AlarmId == alarmId);
                if (alarms.Any())
                {
                    dbContext.AlarmTec_Alarms.DeleteAllOnSubmit(alarms);
                    dbContext.SubmitChanges();
                }
            }
        }
    }
}