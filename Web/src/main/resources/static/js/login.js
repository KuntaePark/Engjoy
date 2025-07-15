document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const emailInput = document.querySelector('input[type="email"]');
    const pwInput = document.querySelector('input[type="password"]');
    const errorBox = document.getElementById("error");

    form.addEventListener("submit", function (event) {
        const email = emailInput.value.trim();
        const pw = pwInput.value.trim();

        errorBox.textContent = "";
        errorBox.style.display = "none";

        if (!email && !pw) {
            event.preventDefault();
            showError("이메일과 비밀번호를 모두 입력하세요.");
            return;
        }

        if (!email) {
            event.preventDefault();
            showError("이메일을 입력하세요.");
            return;
        }

        if (!validateEmail(email)) {
            event.preventDefault();
            showError("올바른 이메일 형식이 아닙니다.");
            return;
        }

        if (!pw) {
            event.preventDefault();
            showError("비밀번호를 입력하세요.");
            return;
        }
    });

    function validateEmail(email) {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }

    function showError(message) {
        errorBox.textContent = message;
        errorBox.style.display = "block";
    }
});



