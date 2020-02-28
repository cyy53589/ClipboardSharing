package com.ChenYuyang.ClipboardSharing;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;

import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AutoCompleteTextView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import com.ChenYuyang.ClipboardSharing.NetworkUtil.BroadcastConstant;
import com.ChenYuyang.ClipboardSharing.NetworkUtil.Configuration;
import com.ChenYuyang.ClipboardSharing.NetworkUtil.WebSocketClient;
import com.ClipboardSharing.R;

/**
 * A login screen that offers login via email/password.
 */
public class LoginActivity extends AppCompatActivity  {

    // UI references.
    private AutoCompleteTextView ipView;
    private EditText usernameView;
    private View mProgressView;
    private View mLoginFormView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);
        Configuration config = new Configuration(getApplicationContext());
        // Set up the login form.
        ipView = (AutoCompleteTextView) findViewById(R.id.address);
        usernameView = (EditText) findViewById(R.id.username);
        ipView.setText(config.getServerIpAddress());
        usernameView.setText(config.getUsername());

        Button mEmailSignInButton = (Button) findViewById(R.id.email_sign_in_button);
        mEmailSignInButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View view) {
                String ip = ipView.getText().toString();
                String username = usernameView.getText().toString();
                config.SetConfig(ip,username);

                if(!WebSocketClient.validIp(ip))return;
                Intent intent = new Intent(BroadcastConstant.Restart);
                intent.putExtra("ipaddress",ip);
                intent.putExtra("username",username);
                sendBroadcast(intent);
                Toast.makeText(getApplicationContext(), "Set Server Done", Toast.LENGTH_SHORT).show();
                finish();
            }
        });

        mLoginFormView = findViewById(R.id.login_form);
        mProgressView = findViewById(R.id.login_progress);
    }
}

