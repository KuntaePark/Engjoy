package com.engjoy.service;

import jakarta.annotation.PostConstruct;
import org.java_websocket.WebSocketAdapter;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;
import org.springframework.stereotype.Service;

import java.net.URI;
import java.net.URISyntaxException;

@Service
public class WebSocketService extends WebSocketClient {

    public WebSocketService() throws URISyntaxException {

        super(new URI("ws://localhost:7779"));
    }

    @PostConstruct
    public void init() {
        new Thread(() -> {
            try {
                this.connectBlocking();
                System.out.println("Connected to game server.");
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }).start();
    }

    @Override
    public void onOpen(ServerHandshake handshakedata) {
        System.out.println("WebSocket connection established.");
    }

    @Override
    public void onMessage(String message) {
        System.out.println("Received from game server: " + message);
    }

    @Override
    public void onClose(int code, String reason, boolean remote) {
        System.out.println("WebSocket closed: " + reason);
        // 자동 재연결 로직을 원한다면 여기에 재시도 추가 가능
    }

    @Override
    public void onError(Exception ex) {
        System.err.println("WebSocket error:");
        ex.printStackTrace();
    }

    public void requestPlayerMatch(String playerId) {
        if (this.isOpen()) {
            this.send(playerId);
        } else {
            System.err.println("WebSocket not connected. Match info not sent.");
        }
    }
}
