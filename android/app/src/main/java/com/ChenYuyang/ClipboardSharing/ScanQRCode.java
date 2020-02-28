package com.ChenYuyang.ClipboardSharing;

import android.graphics.Bitmap;
import android.os.Vibrator;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Toast;

import com.ChenYuyang.ClipboardSharing.NetworkUtil.Message;
import com.ChenYuyang.ClipboardSharing.NetworkUtil.Configuration;
import com.ClipboardSharing.R;

import java.io.BufferedWriter;
import java.io.OutputStreamWriter;
import java.net.Socket;

import cn.bingoogolapple.qrcode.core.QRCodeView;
import cn.bingoogolapple.qrcode.zxing.ZXingView;

public class ScanQRCode extends AppCompatActivity implements QRCodeView.Delegate {

    private ZXingView mZXingView;

    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_scan_qrcode);

        mZXingView = findViewById(R.id.zxingview);
        mZXingView.setDelegate(this);
    }
    @Override
    protected void onStart() {
        super.onStart();

        mZXingView.startCamera(); // 打开后置摄像头开始预览，但是并未开始识别
        mZXingView.startSpotAndShowRect(); // 显示扫描框，并开始识别
    }

    @Override
    protected void onStop() {
        mZXingView.stopCamera(); // 关闭摄像头预览，并且隐藏扫描框
        super.onStop();
    }

    @Override
    protected void onDestroy() {
        mZXingView.onDestroy(); // 销毁二维码扫描控件
        super.onDestroy();
    }

    private void vibrate() {
        Vibrator vibrator = (Vibrator) getSystemService(VIBRATOR_SERVICE);
        vibrator.vibrate(200);
    }

    @Override
    public void onScanQRCodeSuccess(String result) {
        setTitle(getString(R.string.ScanDone));
        vibrate();

        String[] r = result.split(",");
        try {
            String connectCode = r[0];
            int port = Integer.valueOf(r[1]);
            String ip = r[2];
            new Thread(() -> {
                try {
                    Socket socket = new Socket(ip, port);
                    BufferedWriter output = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream()));
                    output.write(new Message(
                            Message.Action.BindDevice,
                            new Configuration(getApplicationContext()).getUsername(),
                            connectCode).ToJson());
                    output.flush();
                    socket.close();
                }catch (Exception e){
                    Log.e("E","Sending bind devices error");
                    e.printStackTrace();
                }
            }).start();
        }catch (Exception e) {
            Toast.makeText(this, "Error Format. " + result, Toast.LENGTH_LONG).show();
        }
        finish();
    }

    @Override
    public void onScanQRCodeOpenCameraError() {
        Log.e("ORC", "打开相机出错");
    }

    @Override
    public void onCameraAmbientBrightnessChanged(boolean isDark) {
        if (isDark) {
            mZXingView.getScanBoxView().setTipText("Dark");
        } else {
            }
    }


    public void test(View view) {
        finish();
    }
}
