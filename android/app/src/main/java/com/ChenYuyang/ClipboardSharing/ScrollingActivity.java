package com.ChenYuyang.ClipboardSharing;

import android.app.ActivityManager;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.TextView;
import android.widget.Toast;

import com.ChenYuyang.ClipboardSharing.NetworkUtil.BroadcastConstant;
import com.ChenYuyang.ClipboardSharing.NetworkUtil.Configuration;
import com.ChenYuyang.ClipboardSharing.NetworkUtil.WebSocketClient;
import com.ClipboardSharing.R;
import com.ChenYuyang.ClipboardSharing.Services.ClipboardSharingService;

import java.net.URI;
import java.util.concurrent.ExecutionException;

public class ScrollingActivity extends AppCompatActivity {
    WebSocketClient client;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Configuration config = new Configuration(getApplicationContext());

        setContentView(R.layout.activity_scrolling);
        //Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        //setSupportActionBar(toolbar);
        if(!isMyServiceRunning(ClipboardSharingService.class)) {
            String ip = config.getServerIpAddress(), username = config.getUsername();
            Intent intent = new Intent(this, ClipboardSharingService.class);
            intent.putExtra("ipaddress", ip);
            intent.putExtra("username", username);
            startService(intent);
        }
        setClient();
    }

    void setClient(){
        try{
            Configuration configuration = new Configuration(getApplicationContext());
            client = new WebSocketClient(new URI(String.format("ws://%s", configuration.getServerIpAddress())),configuration.getUsername()) {
                @Override
                public void onMessage(String message) {
                    return;
                }
            };
            client.connect();
        }catch (Exception e){;}
    }

    @Override
    protected void onResume(){
        if(client != null){
            client.close();
        }
        setClient();
        super.onResume();
    }
    @Override
    protected void onDestroy (){
        //sendBroadcast(new Intent(BroadcastConstant.Stop));
        if(client!=null){
            client.close();
        }
        super.onDestroy();
    }


    public void onClickTakeCamera(View view) {
        Intent intent = new Intent();
        intent.setClass(ScrollingActivity.this,ScanQRCode.class );
        startActivity(intent);;
    }

    public void onClickSettings(View view) {
        Intent intent = new Intent();
        intent.setClass(ScrollingActivity.this,LoginActivity.class);
        startActivity(intent);
    }

    public void SendToServer(View view) {
        if(client!=null && client.isOpen()) {
            client.sendContent(((TextView) findViewById(R.id.toSend)).getText().toString());
            Toast.makeText(this, "send", Toast.LENGTH_SHORT).show();
            ((TextView)findViewById(R.id.toSend)).setText("");
        }
    }

    private boolean isMyServiceRunning(Class<?> serviceClass) {
        try {
            ActivityManager manager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
            for (ActivityManager.RunningServiceInfo service : manager.getRunningServices(Integer.MAX_VALUE)) {
                if (serviceClass.getName().equals(service.service.getClassName())) {
                    return true;
                }
            }
            return false;
        }catch (Exception e){return false;}
    }
}
