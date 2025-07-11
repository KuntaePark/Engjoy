function existingNickname() {
  const nickname = document.getElementById("nicknameInput").value.trim();
  const result = document.getElementById("nicknameExistResult");

  if (!nickname) {
    result.textContent = "닉네임을 입력해주세요.";
    result.style.color = "gray";
    return;
  }

  fetch(`/api/check-nickname?nickname=${encodeURIComponent(nickname)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "회원님의 닉네임이 확인되었습니다!";
        result.style.color = "green";
      } else {
        result.textContent = "존재하지 않는 닉네임입니다. 다시 입력해주세요.";
        result.style.color = "red";
      }
    })
    .catch(err => {
      result.textContent = "서버 오류가 발생했습니다.";
      result.style.color = "gray";
      console.error(err);
    });
}


function nicknameCheck() {
  const newNickname = document.getElementById("newNicknameInput").value.trim();
  const result = document.getElementById("nicknameCheckResult");

  if (!newNickname) {
    result.textContent = "새 닉네임을 입력해주세요.";
    result.style.color = "gray";
    return;
  }

  fetch(`/api/check-nickname?nickname=${encodeURIComponent(newNickname)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "이미 존재하는 닉네임입니다.";
        result.style.color = "red";
      } else {
        result.textContent = "사용 가능한 닉네임입니다!";
        result.style.color = "green";
      }
    })
    .catch(err => {
      result.textContent = "서버 오류가 발생했습니다.";
      result.style.color = "gray";
      console.error(err);
    });
}

function existingEmail() {
  const email = document.getElementById("emailInput").value.trim();
  const result = document.getElementById("emailExistResult");

  if (!email) {
    result.textContent = "이메일을 입력해주세요.";
    result.style.color = "gray";
    return;
  }

  fetch(`/api/check-email?email=${encodeURIComponent(email)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "회원님의 이메일이 확인되었습니다!";
        result.style.color = "green";
      } else {
        result.textContent = "존재하지 않는 이메일입니다. 다시 입력해주세요.";
        result.style.color = "red";
      }
    })
    .catch(err => {
      result.textContent = "서버 오류가 발생했습니다.";
      result.style.color = "gray";
      console.error(err);
    });
}


function emailCheck() {
  const newEmail = document.getElementById("newEmailInput").value.trim();
  const result = document.getElementById("emailCheckResult");

  if (!newEmail) {
    result.textContent = "새 이메일을 입력해주세요.";
    result.style.color = "gray";
    return;
  }

  fetch(`/api/check-email?email=${encodeURIComponent(newEmail)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "이미 존재하는 이메일입니다.";
        result.style.color = "red";
      } else {
        result.textContent = "사용 가능한 이메일입니다!";
        result.style.color = "green";
      }
    })
    .catch(err => {
      result.textContent = "서버 오류가 발생했습니다.";
      result.style.color = "gray";
      console.error(err);
    });
}

document.addEventListener("DOMContentLoaded", function () {
  const saveBtn = document.getElementById("save");
  if (saveBtn) {
    saveBtn.addEventListener("click", function () {
      const nickname = document.getElementById("newNicknameInput").value.trim();
      const email = document.getElementById("newEmailInput").value.trim();

      const params = new URLSearchParams();
      params.append("nickname", nickname);
      params.append("email", email);

      fetch("/myInfoChange", {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
        },
        body: params
      })
      .then(response => {
        if (response.redirected) {
          window.location.href = response.url;
        } else {
          return response.text().then(text => {
            alert("응답 메시지: " + text);
          });
        }
      })
      .catch(err => {
        alert("오류 발생: " + err);
      });
    });
  }
});



saveBtn.addEventListener("click", function () {
  const nickname = document.getElementById("newNicknameInput").value.trim();
  const email = document.getElementById("newEmailInput").value.trim();

  fetch("/myInfoChange", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
    },
    body: new URLSearchParams({
      nickname: nickname,
      email: email,
      password: "새로운 비밀번호 값도 필요하다면 여기에 추가"
    }),
  })
  .then(response => {
    if (response.redirected) {
      window.location.href = response.url;
    } else {
      alert("저장 완료!");
    }
  })
  .catch(error => {
    alert("저장 실패: " + error);
  });
});


