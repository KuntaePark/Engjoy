package com.engjoy.control;


import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.GetMapping;


@Controller

public class HomeController {
    @GetMapping("/")
    public void home(){
        //log.info("홈페이지입니다.");
    }

    @GetMapping("/login")
    public String login(){
        return "login";
    }
}
