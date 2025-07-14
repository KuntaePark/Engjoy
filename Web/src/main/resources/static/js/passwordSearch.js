document.getElementById("resetform").addEventListener("submit", function(event){
    event.preventDefault();
    const email = document.getElementById("emailInput").value.trim();
    const msg = document.getElementById("msg");
    if(!email){
        msg.textContent = "이메일을 입력해주세요"
        msg.style.color = "red";
        return;
    }

    fetch("/api/send-reset-link", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email: email })
  })
  .then(res => res.text())
  .then(msg => {
    document.getElementById("msg").textContent = msg;
  });
});
