package com.engjoy.service;

import com.engjoy.dto.WebSocketPacket;
import jakarta.annotation.PostConstruct;
import org.java_websocket.WebSocketAdapter;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;
import org.springframework.stereotype.Service;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.concurrent.atomic.AtomicBoolean;

@Service
public class WebSocketService extends WebSocketClient {

    private final AtomicBoolean reconnecting = new AtomicBoolean(false);

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
                System.out.println("Initial Conection interrupted.");
                tryReconnect();
            }
        }).start();
    }

    @Override
    public void onOpen(ServerHandshake handshakedata) {
        System.out.println("WebSocket connection established.");
        WebSocketPacket webSocketPacket = new WebSocketPacket("auth","WEBSERVER");
        String packet = webSocketPacket.toJson();
        if(packet != null) {
            this.send(packet);
        }
    }

    @Override
    public void onMessage(String message) {
        System.out.println("Received from game server: " + message);
    }

    @Override
    public void onClose(int code, String reason, boolean remote) {
        System.out.println("WebSocket closed: " + reason);
        tryReconnect();
    }

    @Override
    public void onError(Exception ex) {
        System.err.println("WebSocket error: "+ ex.getMessage());
    }

    private void tryReconnect() {
        if(reconnecting.getAndSet(true)) return;

        new Thread(() -> {
            try {
                while (true) {
                    System.out.println("Trying to reconnect...");

                    try {
                        this.reconnectBlocking(); // reconnect 시도
                    } catch (Exception e) {
                        System.err.println("Exception during reconnect: " + e.getMessage());
                    }

                    if (this.isOpen()) {
                        System.out.println("Reconnected to game server.");
                        break; // 연결 성공 → 루프 종료
                    }

                    System.err.println("Reconnect failed. Retrying in 5 seconds...");
                    Thread.sleep(5000);
                }
            } catch (InterruptedException ignored) {
            } finally {
                reconnecting.set(false);
            }
        }).start();
    }

    public void requestPlayerMatch(Long playerId) {
        WebSocketPacket wsPacket = new WebSocketPacket("auth_allow", playerId.toString());
        if (this.isOpen()) {
            this.send(wsPacket.toJson());
        } else {
            System.err.println("WebSocket not connected. Match info not sent.");
        }
    }
}
