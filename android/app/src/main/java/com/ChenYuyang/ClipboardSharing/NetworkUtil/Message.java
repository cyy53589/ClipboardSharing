package com.ChenYuyang.ClipboardSharing.NetworkUtil;
import com.alibaba.fastjson.JSON;


public class Message {
    public Message(){;}
    public Message(Action act,String usrname,String clipboardContent){
        setAct(act);
        this.Username = usrname;
        this.ClipboardContent = clipboardContent;
    }
    public String ToJson(){
        return JSON.toJSONString(this);
    }
    public String ToEncryJson(){
        return EncryptHeper.Encrypt(this.ToJson());
    }
    public static Message FromJson(String json){
        return JSON.parseObject(json,Message.class);
    }
    public static Message FromEncryJson(String encryJson){
        return FromJson(EncryptHeper.Decrypt(encryJson));
    }
    public enum Action{
        Error(0),SendCopy(1),BindDevice(2);
        final int value;
        Action(int v){this.value=v;}
        public int getValue(){return value;}
    }

    public int getAct() {
        return Act;
    }

    public void setAct(int act) {
        this.Act = act;
    }
    public void setAct(Action act){
        setAct(act.getValue());
    }

    public String getClipboardContent() {
        return ClipboardContent;
    }

    public void setClipboardContent(String clipboardContent) {
        ClipboardContent = clipboardContent;
    }

    public String getUsername() {
        return Username;
    }

    public void setUsername(String username) {
        Username = username;
    }

    public String getErrorMsg() {
        return ErrorMsg;
    }

    public void setErrorMsg(String errorMsg) {
        ErrorMsg = errorMsg;
    }

    private String ClipboardContent;
    private String Username;
    private String ErrorMsg;
    private int Act;
}
