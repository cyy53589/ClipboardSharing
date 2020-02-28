package com.ChenYuyang.ClipboardSharing.NetworkUtil;

import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.util.Log;

import java.net.URI;
import java.net.URISyntaxException;

import org.java_websocket.handshake.ServerHandshake;

public abstract class WebSocketClient extends org.java_websocket.client.WebSocketClient {
    public static boolean validIp(String uri){
        try{
            int portIndex = uri.indexOf(":");
            if(Integer.valueOf(uri.substring(portIndex+1)) > 66000)throw new NumberFormatException();
            for(String i : uri.substring(0,portIndex).split("."))
                if(Integer.valueOf(i) > 256) throw new NumberFormatException();
            return true;
        }catch (NumberFormatException e){
            return false;
        }
    }
    public WebSocketClient(URI uri,String username){
        super(uri);
        this.username = username;
        this.uri = uri;
    }

    @Override
    public void onOpen(ServerHandshake sh) {
        Message m = new Message();
        m.setAct(Message.Action.BindDevice);
        m.setUsername(username);
        send(m.ToEncryJson());
    }

    @Override
    public void onClose(int i,String s,boolean b){;}

    @Override
    public void onError(Exception e){;}

    public void sendContent(String content){
        Log.d("Client","发到内容" + content);

        String encryMsg = new Message(Message.Action.SendCopy,username,content).ToEncryJson();
        send(encryMsg);
    }
    private String username;

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public URI getUri() {
        return uri;
    }

    public void setUri(URI uri) {
        this.uri = uri;
    }

    private URI uri;
}
