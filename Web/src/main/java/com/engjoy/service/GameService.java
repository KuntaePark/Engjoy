package com.engjoy.service;

import com.engjoy.entity.UserGameData;
import com.engjoy.repository.UserGameDataRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
@RequiredArgsConstructor
@Transactional
public class GameService {
    private final UserGameDataRepository userGameDataRepository;

    

}
