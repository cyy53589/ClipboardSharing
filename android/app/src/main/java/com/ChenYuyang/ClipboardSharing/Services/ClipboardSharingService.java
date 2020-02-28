package com.ChenYuyang.ClipboardSharing.Services;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;
import android.os.IBinder;
import android.util.Log;

import com.ChenYuyang.ClipboardSharing.NetworkUtil.Message;
import com.ChenYuyang.ClipboardSharing.NetworkUtil.WebSocketClient;
import com.ChenYuyang.ClipboardSharing.NetworkUtil.BroadcastConstant;
import com.ClipboardSharing.R;

import java.net.URI;
import java.net.URISyntaxException;

public class ClipboardSharingService extends Service {
    WebSocketClient webSocketClient;

    private final BroadcastReceiver receiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            if(action.equals(BroadcastConstant.Restart)){
                String ipaddress = intent.getStringExtra("ipaddress");
                String username = intent.getStringExtra("username");
                if(webSocketClient != null)webSocketClient.close();
                setWebSocketClient(ipaddress,username);
            }
            else if(action.equals(BroadcastConstant.Stop)){
                stopSelf();
            }
        }

    };
    @Override
    public IBinder onBind(Intent intent){
        return null;
    }
    //客户端调用unBindeService()方法断开服务绑定时执行该方法
    @Override
    public boolean onUnbind(Intent intent) {
        return super.onUnbind(intent);
    }
    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        String CHANNEL_ID = "com.ChenYuyang.ClipboardSharing";
        String CHANNEL_NAME = "SC";
        NotificationChannel notificationChannel = null;
        if(Build.VERSION.SDK_INT>=Build.VERSION_CODES.O){
            notificationChannel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationManager.IMPORTANCE_HIGH);
            NotificationManager notificationManager = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
            notificationManager.createNotificationChannel(notificationChannel);
        }
        Notification.Builder builder = new Notification.Builder(this,CHANNEL_ID);

        builder.setOngoing(true);

        builder.setContentTitle("ClipboardSharing")
                .setSmallIcon(R.mipmap.ic_launcher);
        builder.setPriority(Notification.PRIORITY_MAX);
        startForeground(1,builder.build());
        setWebSocketClient(intent.getStringExtra("ipaddress"),intent.getStringExtra("username"));
        return super.onStartCommand(intent, flags, startId);
    }
    void setWebSocketClient(String ipaddress,String username){
        if(ipaddress == null || username == null || !WebSocketClient.validIp(ipaddress))return;
        ClipboardManager cm = (ClipboardManager)getSystemService(Context.CLIPBOARD_SERVICE);
        try {
            webSocketClient = new WebSocketClient(new URI("ws://"+ipaddress), username) {
                @Override
                public void onMessage(String message) {
                    Message msg = Message.FromEncryJson(message);
                    Log.d("Client","收到信息 "+ msg.ToJson());
                    if(!msg.getUsername().equals(username) || msg.getAct() != Message.Action.SendCopy.getValue())return;
                    cm.setPrimaryClip(ClipData.newPlainText(BroadcastConstant.ID,msg.getClipboardContent()));
                }
            };
            webSocketClient.connect();
        }catch (URISyntaxException e){
            Log.e("E", String.format("URI Wrong Format. Ip %s. Mgs %s", ipaddress,e.getMessage() ));
        }

    }

    @Override
    public void onCreate(){
        IntentFilter filter = new IntentFilter();
        filter.addAction(BroadcastConstant.Restart);
        filter.addAction(BroadcastConstant.Stop);
        registerReceiver(receiver, filter);

        ClipboardManager cm = (ClipboardManager)getSystemService(Context.CLIPBOARD_SERVICE);
        cm.addPrimaryClipChangedListener(new ClipboardManager.OnPrimaryClipChangedListener() {
            @Override
            public void onPrimaryClipChanged() {
                Log.d("Clipboard Changed Event","Happen");
                try {
                    CharSequence label = cm.getPrimaryClipDescription().getLabel();
                    if(label != null && label.equals(BroadcastConstant.ID)) return;
                    if(webSocketClient == null)return;
                    ClipData cd = cm.getPrimaryClip();
                    if (cd.getItemCount() < 1) return;
                    webSocketClient.sendContent(cd.getItemAt(0).getText().toString());
                }catch (Exception e){
                    e.printStackTrace();
                }
            }
        });
        Log.d("Service","启动成功");

        super.onCreate();
    }


    //服务被销毁时执行的方法，且只执行一次
    @Override
    public void onDestroy() {
        unregisterReceiver(receiver);
        stopForeground(true);
        super.onDestroy();
    }
}
