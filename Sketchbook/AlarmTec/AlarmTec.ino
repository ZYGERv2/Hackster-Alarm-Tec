#include <LGPS.h>
#include <HttpClient.h>
#include <LTask.h>
#include <LWiFi.h>
#include <LWiFiClient.h>
#include <LDateTime.h>
#define WIFI_AP "Bullwinkle"
#define WIFI_PASSWORD "Wellc0me"
#define WIFI_AUTH LWIFI_WPA  // choose from LWIFI_OPEN, LWIFI_WPA, or LWIFI_WEP.
#define SITE_URL "alarmtecservice.cloudapp.net"

gpsSentenceInfoStruct info;
char buff[256];
double latitude;
double longitude;

char buffer_latitude[8];
char buffer_longitude[8];

LWiFiClient c;

unsigned int rtc;
unsigned int lrtc;
unsigned int rtc1;
unsigned int lrtc1;
char port[4]={0};
char connection_info[21]={0};
char ip[15]={0};             
int portnum;
int val = 0;

LWiFiClient c2;
HttpClient http(c2);

bool isReading;
bool successfulConnection;

void setup() {
  Serial.begin(115200);

  LTask.begin();
  LWiFi.begin();
  LGPS.powerOn();
  
  AP_connect();

  delay(60000);
}

void loop() {
   GPS_receive();

    Serial.print("Connecting site...");
  
  // if you get a connection, report back via serial:
  if (c2.connect(SITE_URL, 80)) {
    Serial.println("connected");  
    successfulConnection = true;  
  } else {
    // if you didn't get a connection to the server:
    Serial.println("connection failed");
  }
  
    // get enabled features and alarm time; settings
    Serial.println("Get alarm settings"); 
    http.get("alarmtecservice.cloudapp.net", "/AlarmTecService/GetAlarm?alarmId=1");
    http.skipResponseHeaders();
    String jsonResponse;
    while (http.connected()&& http.available() && http.endOfHeadersReached()) {   
      Serial.print("."); 
      isReading = true;
      char v = http.read();
      jsonResponse += v;
    }   

    Serial.println("");

    if(isReading){
      //reset to defaults
      isReading = false;
      successfulConnection = false;
      Serial.println("Got alarm settings");
      Serial.println(jsonResponse);
    }
    
  delay(60000);
}
