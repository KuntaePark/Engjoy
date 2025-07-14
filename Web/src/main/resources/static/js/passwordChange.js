document.addEventListener("DOMContentLoaded", function (){
document.getElementById("passwordChangeForm").addEventListener("submit", function(event){

    const form = document.getElementById("passwordChangeForm");
    const pw1 = document.getElementById("passwordFirst").value;
    const pw2 = document.getElementById("passwordSecond").value;
    const result = document.getElementById("result");

    form.addEventListener("submit", function (e) {
      const pw1 = pw1Input.value.trim();
      const pw2 = pw2Input.value.trim();

      let hasError = false;

      if (!pw1 || !pw2) {
        result.textContent = "※비밀번호를 모두 입력해주세요.";
        result.style.color = "red";
        hasError = true;
      } else if (pw1 !== pw2) {
        result.textContent = "※비밀번호가 일치하지 않습니다.";
        result.style.color = "red";
        hasError = true;
      } else if (pw1.length < 8 || pw1.length > 32) {
        result.textContent = "※비밀번호는 8~32자 사이여야 합니다.";
        result.style.color = "red";
        hasError = true;
      }

      if (hasError) {
        e.preventDefault();
      }
    });


