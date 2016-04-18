package apps.brokenwallsstudios.alarm_tec;

import android.app.Activity;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.TimePicker;
import android.widget.Toast;

import com.google.gson.Gson;

import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;

import apps.brokenwallsstudios.alarm_tec.model.Alarm;

public class AlarmDetailActivity extends Activity {

    AppSettings appSettings;

    Switch mAlarmGetWeather;
    Switch mAlarmGetNews;

    TimePicker mAlarmWakeUpTimePicker;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_alarm_detail);

        appSettings = new AppSettings(this);

        final int alarmId = getIntent().getExtras().getInt("alarmId");
        final String alarmName = getIntent().getExtras().getString("alarmName");
        final String alarmCreateDate = getIntent().getExtras().getString("alarmCreateDate");
        String alarmWakeUpTime = getIntent().getExtras().getString("alarmWakeupTime");
        boolean alarmWantsWeather = getIntent().getExtras().getBoolean("alarmWantsWeather", false);
        boolean alarmWantsNews = getIntent().getExtras().getBoolean("alarmWantsNews", false);

        TextView mAlarmNameText = (TextView) findViewById(R.id.alarmNameText);
        TextView mAlarmDateText = (TextView) findViewById(R.id.alarmDateText);

        mAlarmGetWeather = (Switch) findViewById(R.id.getWeatherSwitch);
        mAlarmGetNews = (Switch) findViewById(R.id.getNewsSwitch);

        mAlarmWakeUpTimePicker = (TimePicker) findViewById(R.id.wakeupTimePicker);

        Button mSaveAlarmButton = (Button) findViewById(R.id.saveAlarmButton);
        mSaveAlarmButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {

                Alarm alarm = new Alarm();
                alarm.Id = alarmId;
                alarm.Name = alarmName;
                alarm.CreateDate = alarmCreateDate;
                alarm.WantsNews = mAlarmGetNews.isChecked();
                alarm.WantsWeather = mAlarmGetWeather.isChecked();
                Date date = new Date();
                Calendar calendar = GregorianCalendar.getInstance();
                calendar.setTime(date);
                calendar.set(Calendar.HOUR, mAlarmWakeUpTimePicker.getHour());
                calendar.set(Calendar.MINUTE, mAlarmWakeUpTimePicker.getMinute());
                //MM/dd/yyyy hh:mm:ss aa
                alarm.WakeUpTime = calendar.get(Calendar.MONTH)+"/"+calendar.get(Calendar.DAY_OF_MONTH)+"/"+calendar.get(Calendar.YEAR)
                        + " " +calendar.get(Calendar.HOUR_OF_DAY) + ":" + calendar.get(Calendar.MINUTE) +":"+calendar.get(Calendar.SECOND)
                        +" ";
                if(calendar.get(Calendar.AM_PM) == 0) {
                    alarm.WakeUpTime += "AM";
                }else {
                    alarm.WakeUpTime += "PM";
                }

                alarm.IsEnabled = true;
                alarm.UserId = appSettings.UserId;

                // update alarm
                UpdateAlarmTask mUpdateAlarmTask = new UpdateAlarmTask(alarm);
                mUpdateAlarmTask.execute((Void) null);
            }
        });

        Button mUnregisterAlarmButton = (Button) findViewById(R.id.unregisterDeviceButton);
        mUnregisterAlarmButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                // Unregister device
                UnregisterDeviceTask mUnregisterDeviceTask = new UnregisterDeviceTask(alarmId);
                mUnregisterDeviceTask.execute((Void) null);
            }
        });

        mAlarmNameText.setText(alarmName);
        mAlarmDateText.setText("Added: " + alarmCreateDate);

        mAlarmGetWeather.setChecked(alarmWantsWeather);
        mAlarmGetNews.setChecked(alarmWantsNews);

        if(alarmWakeUpTime != null && !alarmWakeUpTime.isEmpty()) {
            SimpleDateFormat dateFormat = new SimpleDateFormat("MM/dd/yyyy hh:mm:ss aa");
            try {
                Date date1 = dateFormat.parse(alarmWakeUpTime);
                Calendar calendar = GregorianCalendar.getInstance();
                calendar.setTime(date1);
                mAlarmWakeUpTimePicker.setHour(calendar.get(Calendar.HOUR));
                mAlarmWakeUpTimePicker.setMinute(calendar.get(Calendar.MINUTE));
            }catch(Exception ex){

            }
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        //getMenuInflater().inflate(R.menu.menu_device_detail, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        //if (id == R.id.action_settings) {
          //  return true;
        //}

        return super.onOptionsItemSelected(item);
    }


    public class UnregisterDeviceTask extends AsyncTask<Void, Void, Boolean> {

        private final int AlarmId;

        UnregisterDeviceTask(int alarmId) {
            AlarmId = alarmId;
        }

        @Override
        protected Boolean doInBackground(Void... params) {
            try {
                URL url = new URL("http://alarmtecservice.cloudapp.net/AlarmTecService/RemoveAlarm?alarmId="+ AlarmId);
                URLConnection connection = url.openConnection();
                connection.setRequestProperty("Content-Type", "application/json");
                connection.setConnectTimeout(5000);
                connection.setReadTimeout(5000);

                BufferedReader in = new BufferedReader(new InputStreamReader(connection.getInputStream()));
                StringBuilder sb = new StringBuilder();
                String line;
                while ((line = in.readLine()) != null) {
                    sb.append(line);
                }
                in.close();
                return Boolean.parseBoolean(sb.toString());
            } catch (Exception e) {
                System.out.println("\nError while calling service");
                System.out.println(e);
            }
            return false;
        }

        @Override
        protected void onPostExecute(final Boolean success) {
            if (success) {
                finish();
            }else{
                Toast.makeText(getBaseContext(), "Unable to unregister the alarm. Please try again later.", Toast.LENGTH_LONG).show();
            }
        }
    }

    public class UpdateAlarmTask extends AsyncTask<Void, Void, Boolean> {

        private final Alarm alarm;

        UpdateAlarmTask(Alarm _alarm) {
            alarm = _alarm;
        }

        @Override
        protected Boolean doInBackground(Void... params) {
            try {
                URL url = new URL("http://alarmtecservice.cloudapp.net/AlarmTecService/UpdateAlarm");
                HttpURLConnection connection = (HttpURLConnection)url.openConnection();
                connection.setRequestMethod("POST");
                connection.setRequestProperty("Content-Type", "application/json");
                connection.setConnectTimeout(5000);
                connection.setReadTimeout(5000);

                Gson gson = new Gson();
                DataOutputStream wr = new DataOutputStream(connection.getOutputStream ());
                String parsed = gson.toJson(alarm, Alarm.class);

                wr.writeBytes(parsed);

                wr.flush();
                wr.close();

                BufferedReader in = new BufferedReader(new InputStreamReader(connection.getInputStream()));
                StringBuilder sb = new StringBuilder();
                String line;
                while ((line = in.readLine()) != null) {
                    sb.append(line);
                }
                in.close();
                return true;
            } catch (Exception e) {
                System.out.println("\nError while calling service");
                System.out.println(e);
            }
            return false;
        }

        @Override
        protected void onPostExecute(final Boolean success) {
            if (success) {
                finish();
            }else{
                Toast.makeText(getBaseContext(), "Unable to update the alarm. Please try again later.", Toast.LENGTH_LONG).show();
            }
        }
    }
}