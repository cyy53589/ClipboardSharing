package com.ChenYuyang.ClipboardSharing.NetworkUtil;
import android.content.Context;

import com.alibaba.fastjson.JSON;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;

public class Configuration {
    public Configuration(){;}
    public Configuration(Context context){
        try{
            fileLocaltion = context.getFilesDir().getAbsolutePath() + "/" + fileName;
            FileInputStream stream  = new FileInputStream(fileLocaltion);
            BufferedReader input = new BufferedReader(new InputStreamReader(stream));
            StringBuilder sb = new StringBuilder();
            String tmp;
            while((tmp = input.readLine()) != null) sb.append(tmp);
            Configuration t = JSON.parseObject(sb.toString(),Configuration.class);
            setServerIpAddress(t.getServerIpAddress());
            setUsername(t.getUsername());
            input.close();
        }
        catch (Exception e){
            SaveToFile();
        }

    }
    public void SaveToFile(){
        try{
            BufferedWriter output = new BufferedWriter(new OutputStreamWriter(new FileOutputStream(fileLocaltion)));
            output.write(JSON.toJSON(this).toString());
            output.close();
        }catch (Exception e){
            e.printStackTrace();
        }
    }

    public String getServerIpAddress() {
        return ServerIpAddress;
    }

    public void setServerIpAddress(String serverIpAddress) {
        ServerIpAddress = serverIpAddress;
    }

    public String getUsername() {
        return Username;
    }

    public void setUsername(String username) {
        Username = username;
    }

    public void SetConfig(String ipaddress,String username){
        setUsername(username);
        setServerIpAddress(ipaddress);
        SaveToFile();
    }
    private String ServerIpAddress =  "127.0.0.1:5358"; // default
    private String Username = "CyyChen"; // default
    static String fileName = "main.config";
    static String fileLocaltion = "/data/user/0/com.chenyuyang.clipboardsharing/files/main.config";
}
