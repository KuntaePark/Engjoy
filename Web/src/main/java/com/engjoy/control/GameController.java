package com.engjoy.control;


import com.engjoy.dto.UserGameDataDto;
import com.engjoy.service.GameService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.ResponseBody;

import java.security.Principal;

@Controller
@RequiredArgsConstructor
public class GameController {
    private final GameService gameService;

    @GetMapping("/game")
    public String getGamePage() {
        return "game";
    }

    @GetMapping("/game/test")
    @ResponseBody
    public ResponseEntity<UserGameDataDto> getUserGameData(Principal principal) {
        String email = principal.getName();
        UserGameDataDto dto = gameService.getUserGameData(email);

        return ResponseEntity.ok(dto);
    }

    @PostMapping("/game/test")
    @ResponseBody
    public ResponseEntity<String> testPost() {

        return ResponseEntity.ok("testing post.");
    }

    @PostMapping("/game/match/join")
    public ResponseEntity<String> matchJoin(Principal principal) {
        //매칭 큐에 해당 플레이어 추가, 인증되지 않은 유저라면 403 뜸
        String email = principal.getName();
        Long id = gameService.allowMatch(email);
        return ResponseEntity.ok(id.toString());
    }

    @PostMapping("/game/lobby/join")
    public ResponseEntity<String> lobbyJoin(Principal principal) {
        String email = principal.getName();
        Long id = gameService.allowLobby(email);
        return ResponseEntity.ok(id.toString());
    }
}
