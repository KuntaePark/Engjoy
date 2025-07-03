package com.engjoy.control;


import com.engjoy.dto.UserGameDataDto;
import com.engjoy.service.GameService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;
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
}
