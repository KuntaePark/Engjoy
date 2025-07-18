package com.engjoy.control;


import com.engjoy.dto.UserGameDataDto;
import com.engjoy.service.GameService;
import com.fasterxml.jackson.core.JsonProcessingException;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.*;

import java.security.Principal;
import java.util.HashMap;
import java.util.Map;

@Controller
@RequiredArgsConstructor
public class GameController {
    private final GameService gameService;

    @GetMapping("/game")
    public String getGamePage() {
        return "game";
    }

    @GetMapping("/game/user/data")
    @ResponseBody
    public ResponseEntity<UserGameDataDto> getUserGameData(Principal principal) {
        String email = principal.getName();
        UserGameDataDto dto = gameService.getUserGameData(email);

        return ResponseEntity.ok(dto);
    }

    @PostMapping("/game/user/customization")
    @ResponseBody
    public ResponseEntity<UserGameDataDto> saveCustomizationData(Principal principal, @RequestBody HashMap<String, Integer> data) {
        String email = principal.getName();
        UserGameDataDto dto = gameService.saveCustomizationData(email, data);
        return ResponseEntity.ok(dto);
    }

    @PostMapping("/game/match/join/{gameId}")
    public ResponseEntity<String> matchJoin(Principal principal, @PathVariable("gameId") int gameId) throws JsonProcessingException {
        //매칭 큐에 해당 플레이어 추가, 인증되지 않은 유저라면 403 뜸
        String email = principal.getName();
        Long id = gameService.allowMatch(email, gameId);
        return ResponseEntity.ok(id.toString());
    }


    @PostMapping("/game/lobby/join")
    public ResponseEntity<String> lobbyJoin(Principal principal) throws JsonProcessingException {
        String email = principal.getName();
        Long id = gameService.allowLobby(email);
        return ResponseEntity.ok(id.toString());
    }
}
