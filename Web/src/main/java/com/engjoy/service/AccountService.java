package com.engjoy.service;

import com.engjoy.entity.Account;
import com.engjoy.repository.AccountRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.security.core.userdetails.User;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.core.userdetails.UserDetailsService;
import org.springframework.security.core.userdetails.UsernameNotFoundException;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
public class AccountService implements UserDetailsService {
    private final AccountRepository accountRepository;

    @Override
    public UserDetails loadUserByUsername(String username) throws UsernameNotFoundException {
        //해당 유저의 이메일로
        Account account = accountRepository.findByEmail(username);

        if(account == null) {
            throw new UsernameNotFoundException(username);
        }

        return User.builder()
                .username(account.getEmail())
                .password(account.getPassword())
                .build();
    }
}
