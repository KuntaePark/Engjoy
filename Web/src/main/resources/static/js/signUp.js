function checkNickname(){
    const nickname = document.getElementById("nickname").value.trim();
    const result = document.getElementById("nicknameResult");
    if( !nickname){
        alert("닉네임을 입력해주세요")
        return;

    }
    fetch(`/api/check-nickname?nickname=${encodeURIComponent(nickname)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "이미 사용 중인 닉네임입니다.";
        result.style.color = "red";
      } else {
        result.textContent = "사용 가능한 닉네임입니다!";
        result.style.color = "green";
      }
    })
}


function checkEmail(){
    const email = document.getElementById("email").value.trim();
    const result = document.getElementById("emailResult");
    if( !email){
        alert("이메일을 입력해주세요")
        return;

    }
    fetch(`/api/check-email?email=${encodeURIComponent(email)}`)
    .then(response => response.json())
    .then(data => {
      if (data.exists) {
        result.textContent = "이미 사용 중인 이메일입니다.";
        result.style.color = "red";
      } else {
        result.textContent = "사용 가능한 이메일입니다!";
        result.style.color = "green";
      }
    })
}


function checkPassword(){
    const password1 = document.getElementById("password1").value;
    const password2 = document.getElementById("password2").value;
    if(password1 !== password2){
        alert("비밀번호가 일치하지 않습니다, 다시 확인해주세요.");
        return;
    }else{
        alert("비밀번호가 일치합니다")
    }
}

function join(){
    alert("회원가입이 완료되었습니다!")
}



