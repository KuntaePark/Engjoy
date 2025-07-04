document.getElementById("passwordChangeForm").addEventListener("submit", function(event){

    const pw1 = document.getElementById("passwordFirst").value;
    const pw2 = document.getElementById("passwordSecond").value;
    const result = document.getElementById("result");

    if(!pw1 || !pw2){
        event.preventDefault();
        result.innerHTML = "※비밀번호를 입력해주세요."

    }else if( pw1 !== pw2){
        event.preventDefault();
        result.innerHTML = "※비밀번호가 서로 일치하지 않습니다."
    }else if( pw1.length < 8 || pw1.length > 32){
        event.preventDefault();

        result.innerHTML = "※비밀번호를 8자 이상 32자 이하로 입력해주세요"
    }else{
        result.innerHTML = "비밀번호가 성공적으로 변경되었습니다."
    }
});

