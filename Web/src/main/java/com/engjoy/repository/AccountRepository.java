package com.engjoy.repository;

import com.engjoy.entity.Account;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

@Repository
public interface AccountRepository extends JpaRepository<Account, Long> {
    //이메일로 검색
    public Account findByEmail(String email);
}
