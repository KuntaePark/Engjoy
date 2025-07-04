function existingNickname() {
  const nickname = document.getElementById("nicknameInput").value.trim();
  const result = document.getElementById("nicknameResult");

  if (!nickname) {
    result.textContent = "닉네임을 입력해주세요.";
    result.style.color = "gray";
    return;
  }

  fetch(`/api/check-nickname?nickname=${encodeURIComponent(nickname)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "회원님의 닉네임이 확인되었습니다!.";
        result.style.color = "red";
      } else {
        result.textContent = "존재하지 않는 닉네임입니다.다시 입력해주세요"
      }
    })
    .catch(err => {
      result.textContent = "서버 오류가 발생했습니다.";
      result.style.color = "gray";
      console.error(err);
    });
}



function nicknameCheck(){
    const newNickname = document.getElementById("newNicknameInput").value.trim();
    const result = document.getElementById("nicknameResult");
    if (!newNickname){
        result.textContent = "새 닉네임을 입력해주세요"
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
  const result = document.getElementById("emailResult");

  if (!email) {
    result.textContent = "이메일을 입력해주세요.";
    result.style.color = "gray";
    return;
  }

  fetch(`/api/check-email?email=${encodeURIComponent(email)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "회원님의 이메일이 확인되었습니다!.";
        result.style.color = "red";
      } else {
        result.textContent = "존재하지 않는 이메일 입니다. 다시 입력해주세요.";
        result.style.color = "green";
      }
    })
    .catch(err => {
      result.textContent = "서버 오류가 발생했습니다.";
      result.style.color = "gray";
      console.error(err);
    });
}





function emailCheck(){
    const newEmail = document.getElementById("newEmailInput").value.trim();
    const result = document.getElementById("emailResult");
    if( !newEmail) {
        result.textContent = "새 이메일을 입력해주세요"
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
    const nicknameCheckBtn = document.getElementById("nicknameCheckBtn");

    if (nicknameCheckBtn) {
        nicknameCheckBtn.addEventListener("click", nicknameCheck);
    }
});


