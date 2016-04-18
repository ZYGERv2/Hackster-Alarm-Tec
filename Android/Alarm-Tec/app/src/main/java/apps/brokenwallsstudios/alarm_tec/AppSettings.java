package apps.brokenwallsstudios.alarm_tec;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;

public class AppSettings {
    public static String AppSettingsName = "ReloxSettings";

    public static String IsScreenOnSetting = "IsScreenOn";
    public static String IsPhoneRingingSetting = "IsPhoneRinging";

    public static String UserIdSetting = "UserId";
    public static String UserEmailSetting = "UserEmail";

    public static String DeviceIdSetting = "DeviceId";

    public boolean IsScreenOn = true;
    public boolean IsPhoneRinging = false;
    public int UserId = 0;
    public int DeviceId = 0;
    public String UserEmail = null;
    public String DeviceName = null;

    private SharedPreferences AppPreferences;

    public AppSettings(Context context){
        AppPreferences = context.getSharedPreferences(AppSettingsName, Activity.MODE_PRIVATE);
        LoadSettings();
    }

    public void SaveSettings(){
        SharedPreferences.Editor editor = AppPreferences.edit();
        editor.putBoolean(IsScreenOnSetting, IsScreenOn);
        editor.putBoolean(IsPhoneRingingSetting, IsPhoneRinging);
        editor.putInt(UserIdSetting, UserId);
        editor.putString(UserEmailSetting, UserEmail);
        editor.putInt(DeviceIdSetting, DeviceId);
        editor.commit();
    }

    public void LoadSettings(){
        IsScreenOn = AppPreferences.getBoolean(IsScreenOnSetting, true);
        IsPhoneRinging = AppPreferences.getBoolean(IsPhoneRingingSetting, false);
        UserId = AppPreferences.getInt(UserIdSetting, 0);
        UserEmail = AppPreferences.getString(UserEmailSetting, null);
        DeviceId = AppPreferences.getInt(DeviceIdSetting, 0);
        DeviceName = android.os.Build.MODEL;
    }
}
