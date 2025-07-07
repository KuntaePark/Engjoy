package com.engjoy.service;

import com.engjoy.dto.UserGameDataDto;
import com.engjoy.repository.AccountRepository;
import com.engjoy.repository.UserGameDataRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.LinkedList;
import java.util.Queue;

@Service
@RequiredArgsConstructor
@Transactional
public class GameService {
    private final UserGameDataRepository userGameDataRepository;
    private final AccountRepository accountRepository;

    private final WebSocketService socketService;

    public UserGameDataDto getUserGameData(String email) {
        return UserGameDataDto.from(userGameDataRepository.findByAccount_Email(email));
    }

    public Long allowMatch(String email) {
        Long id = accountRepository.findByEmail(email).getId();

        socketService.requestPlayerMatch(id);
        return id;
    }

}
