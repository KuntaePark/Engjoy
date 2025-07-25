package com.engjoy.service;

import com.engjoy.dto.LobbyAuthDataDto;
import com.engjoy.dto.UserGameDataDto;
import com.engjoy.entity.UserGameData;
import com.engjoy.repository.AccountRepository;
import com.engjoy.repository.UserGameDataRepository;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import jakarta.annotation.PostConstruct;
import lombok.RequiredArgsConstructor;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.net.URISyntaxException;
import java.util.HashMap;
import java.util.Map;

@Service
@RequiredArgsConstructor
@Transactional
public class GameService{
    private final UserGameDataRepository userGameDataRepository;
    private final AccountRepository accountRepository;
    private final JdbcTemplate jdbcTemplate;

    private ServerSocket lobbyServerSocket;
    private ServerSocket matchServerSocket;

    @PostConstruct
    public void init() throws URISyntaxException {
        lobbyServerSocket = new ServerSocket("ws://localhost:7777");
        matchServerSocket = new ServerSocket("ws://localhost:7779");

        lobbyServerSocket.init();
        matchServerSocket.init();
    }

    public UserGameDataDto getUserGameDataDto(String email) {
        String sql = """
            SELECT *
             FROM (
                 SELECT\s
                     a.nickname,
                     u.game1score,
                     u.game2high_score,
                     u.gold,
                     u.body_type_index,
                     u.weapon_type_index,
                     RANK() OVER (ORDER BY u.game1score DESC, u.account_id ASC) AS ranking,
                     (RANK() OVER (ORDER BY u.game1score DESC, u.account_id ASC) - 1) / (COUNT(*) OVER()) AS ranking_percent,
                     a.email
                 FROM user_game_data u
                 JOIN account a ON u.account_id = a.account_id
             ) ranked
             WHERE email = ?;
        """;

        return jdbcTemplate.queryForObject(sql, new Object[]{email}, (rs, rowNum) ->
                new UserGameDataDto(
                        rs.getString("nickname"),
                        rs.getInt("game1score"),
                        rs.getInt("game2high_score"),
                        rs.getInt("gold"),
                        rs.getLong("ranking"),
                        rs.getFloat("ranking_percent"),
                        rs.getInt("body_type_index"),
                        rs.getInt("weapon_type_index")
                )
        );
    }

    public UserGameDataDto getUserGameData(String email) {
        return getUserGameDataDto(email);
    }

    public Long allowMatch(String email, Integer gameId) throws JsonProcessingException {
        Long id = accountRepository.findByEmail(email).get().getId();
        Map<String, Object> map = new HashMap<>();
        map.put("id", id);
        map.put("gameId", gameId);
        ObjectMapper mapper = new ObjectMapper();
        String payload = mapper.writeValueAsString(map);
        matchServerSocket.sendPacket("auth_allow", payload);
        return id;
    }

    public Long allowLobby(String email) throws JsonProcessingException {
        Long id = accountRepository.findByEmail(email).get().getId();
        UserGameData userGameData = userGameDataRepository.findByAccount_Email(email);
        ObjectMapper mapper = new ObjectMapper();
        String payload = mapper.writeValueAsString(LobbyAuthDataDto.from(userGameData));
        lobbyServerSocket.sendPacket("auth_allow", payload);
        return id;
    }

    public UserGameDataDto saveCustomizationData(String email, HashMap<String, Integer> data) {
        UserGameData userGameData = userGameDataRepository.findByAccount_Email(email);
        userGameData.setBodyTypeIndex(data.get("bodyTypeIndex"));
        userGameData.setWeaponTypeIndex(data.get("weaponTypeIndex"));
        UserGameData saved =  userGameDataRepository.save(userGameData);
        ObjectMapper mapper = new ObjectMapper();
        try {
            String payload = mapper.writeValueAsString(LobbyAuthDataDto.from(userGameData));
            lobbyServerSocket.sendPacket("customization_update", payload);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }

        return UserGameDataDto.from(saved);
    }
}
