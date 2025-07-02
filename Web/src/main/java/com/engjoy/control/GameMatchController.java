package com.engjoy.control;

import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.PostMapping;

import java.security.Principal;

@Controller
public class GameMatchController {

    @PostMapping("/match/join")
    public String matchJoin(Principal principal) {
        return "match/join";
    }
}
