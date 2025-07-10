package com.engjoy.service;

import com.engjoy.dto.WebSocketPacket;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.HashMap;
import java.util.concurrent.atomic.AtomicBoolean;


public class ServerSocket extends WebSocketClient {

    private final AtomicBoolean reconnecting = new AtomicBoolean(false);

    public ServerSocket(String url) throws URISyntaxException {
        super(new URI(url));
    }

    public void init() {
        new Thread(() -> {
            try {
                this.connectBlocking();
                System.out.println("Connected to game server.");
            } catch (InterruptedException e) {
                System.out.println("Initial Connection interrupted.");
                tryReconnect();
            }
        }).start();
    }

    @Override
    public void onOpen(ServerHandshake handshakedata) {
        System.out.println("WebSocket connection established.");
        HashMap<String, String> obj = new HashMap<>();
        obj.put("id", "WEBSERVER");
        ObjectMapper mapper = new ObjectMapper();
        try {
            String payloadJson = mapper.writeValueAsString(obj);
            sendPacket("auth", payloadJson);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
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

    public void sendPacket(String type, String payloadJson) {
        WebSocketPacket wsPacket = new WebSocketPacket(type, payloadJson);
        if (this.isOpen()) {
            this.send(wsPacket.toJson());
        } else {
            System.err.println("WebSocket not connected. Info not sent.");
        }
    }
}
