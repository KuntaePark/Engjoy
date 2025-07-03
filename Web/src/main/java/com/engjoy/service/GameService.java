package com.engjoy.service;

import com.engjoy.dto.UserGameDataDto;
import com.engjoy.repository.UserGameDataRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Transactional
public class GameService {
    private final UserGameDataRepository userGameDataRepository;

    public UserGameDataDto getUserGameData(String email) {
        return UserGameDataDto.from(userGameDataRepository.findByAccount_Email(email));
    }

    

}
