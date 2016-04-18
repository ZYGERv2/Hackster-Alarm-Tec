using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using Alarm_Tec_Service.Models;

namespace Alarm_Tec_Service
{
    [ServiceContract]
    public interface IAlarmTecService
    {
        [OperationContract]
        bool RegisterUser(Stream userStream);

        [OperationContract]
        bool IsEmailRegistered(string email);

        [OperationContract]
        User Login(Stream userStream);

        [OperationContract]
        void AddAlarm(Stream alarmStream);

        [OperationContract]
        List<Alarm> GetAlarms(int userId);

        [OperationContract]
        Alarm GetAlarm(int alarmId);

        [OperationContract]
        void RemoveAlarm(int alarmId);

        [OperationContract]
        void UpdateAlarm(Stream alarmStream);

        [OperationContract]
        Stream GetWeather(string lat, string lon);

        [OperationContract]
        Stream GetNews();

        [OperationContract]
        Stream GetNewsTest();
    }
}
