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

    const hasUpper = /[A-Z]/.test(password1);
     const hasLower = /[a-z]/.test(password1);
     const hasDigit = /[0-9]/.test(password1);
     const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password1);

  let count = 0;
  if (hasUpper) count++;
  if (hasLower) count++;
  if (hasDigit) count++;
  if (hasSpecial) count++;

  if (password1.length < 8 || password1.length > 32) {
    alert("비밀번호는 8자 이상 32자 이하로 입력해주세요.");
    return;
  }

  if (count < 2) {
    alert("비밀번호는 영문 대/소문자, 숫자, 기호 중 2가지 이상을 포함해야 합니다.");
    return;
  }

  if (password1 !== password2) {
    alert("비밀번호가 일치하지 않습니다.");
    return;
  }

  alert("비밀번호가 조건에 맞고 일치합니다!");
}




