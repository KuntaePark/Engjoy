package com.engjoy.service;

import com.engjoy.dto.UserGameDataDto;
import com.engjoy.repository.AccountRepository;
import com.engjoy.repository.UserGameDataRepository;
import jakarta.annotation.PostConstruct;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.net.URISyntaxException;

@Service
@RequiredArgsConstructor
@Transactional
public class GameService{
    private final UserGameDataRepository userGameDataRepository;
    private final AccountRepository accountRepository;

    private ServerSocket lobbyServerSocket;
    private ServerSocket matchServerSocket;

    @PostConstruct
    public void init() throws URISyntaxException {
        lobbyServerSocket = new ServerSocket("ws://localhost:7777");
        matchServerSocket = new ServerSocket("ws://localhost:7779");

        lobbyServerSocket.init();
        matchServerSocket.init();
    }

    public UserGameDataDto getUserGameData(String email) {
        return UserGameDataDto.from(userGameDataRepository.findByAccount_Email(email));
    }

    public Long allowMatch(String email) {
        Long id = accountRepository.findByEmail(email).getId();
        matchServerSocket.allowPlayer(id);
        return id;
    }

    public Long allowLobby(String email) {
        Long id = accountRepository.findByEmail(email).getId();
        lobbyServerSocket.allowPlayer(id);
        return id;
    }
}
