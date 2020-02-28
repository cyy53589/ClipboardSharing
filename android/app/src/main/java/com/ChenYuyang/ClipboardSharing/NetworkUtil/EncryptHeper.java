package com.ChenYuyang.ClipboardSharing.NetworkUtil;

import android.util.Base64;

import javax.crypto.Cipher;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class EncryptHeper{
    // Encrypt: string -> utf8 bytes --DES--> DES bytes -> base64
    static SecretKey secretKey;
    static byte[] IV = new byte[] {  0x12, 0x34, 0x56, 0x78, 0x11, 0x13, 0x14, 0x15 };
    static{
        String keyStr = "Zxkl;kenshyl".substring(0,8);
        try {
            byte[] decodedKey = keyStr.getBytes("UTF8");
            /* Decodes a Base64 encoded String into a byte array */
            /* Constructs a secret key from the given byte array */
            secretKey = new SecretKeySpec(decodedKey, 0,
                    decodedKey.length, "DES");
        }catch (Exception e){
        }
    }
    public static String Encrypt(String content) {
        try {
            Cipher desCipher = Cipher.getInstance("DES/CBC/PKCS5Padding");
            desCipher.init(Cipher.ENCRYPT_MODE, secretKey,new IvParameterSpec(IV));
            return Base64.encodeToString(desCipher.doFinal(content.getBytes("UTF8")), Base64.DEFAULT);
        } catch (Exception e) {
            return null;
        }
    }
    public static String Decrypt(String content){
        try{
            Cipher desCipher = Cipher.getInstance("DES/CBC/PKCS5Padding");
            desCipher.init(Cipher.DECRYPT_MODE, secretKey,new IvParameterSpec(IV));
            return new String(desCipher.doFinal(Base64.decode(content,Base64.DEFAULT)));
        }catch (Exception e){
            return null;
        }
    }
}
