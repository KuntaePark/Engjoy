const Physics = require("./physics.js");

const deltaTime = 1 / 40; //20 FPS _ 프레임

const speed = 10.0; // 속도

function update(gameState) {
  for (const id in gameState.players) {
    const p = gameState.players[id]; //키워드 들고 있으면 키워드 위치도 플레이어 위치로 고정!(동기화)

    if (p.holdingKeywordId && gameState.keywords[p.holdingKeywordId]) {
      const holdingKeyword = gameState.keywords[p.holdingKeywordId];

      holdingKeyword.x = p.x;

      holdingKeyword.y = p.y;
    } //플레이어 이동 계산

    const newX = p.x + p.inputH * deltaTime * speed;
    const newY = p.y + p.inputV * deltaTime * speed; //충돌 체크 로직

    let isColliding = false;

    for (const otherId in gameState.players) {
      if (id === otherId) continue;

      const otherPlayer = gameState.players[otherId];
      const myPos = new Physics.Vector2(newX, newY);

      const otherCurrentPos = new Physics.Vector2(otherPlayer.x, otherPlayer.y);

      if (Physics.checkCollision(myPos, otherCurrentPos, 30)) {
        console.log(`Player ${id} is colliding with ${otherId}`);

        isColliding = true;

        break;
      }
    }

    if (!isColliding) {
      p.x = newX;
      p.y = newY;
    } //입력값 초기화

    p.inputH = 0;
    p.inputV = 0; //상호작용 가능 상태 업데이트(키워드, 출구)

    updateInteractionState(p, gameState.keywords);
    updateExitInteractionState(p, gameState.exit);
  }
}

function updateExitInteractionState(player, exit) {
  if (!player.holdingKeywordId || !exit || exit.isOpen) {
    player.canInteractWithExit = false;

    return;
  }

  const distSq = Physics.squareDistance(
    { x: player.x, y: player.y },
    { x: exit.x, y: exit.y }
  );

  if (distSq < Physics.interactionDistSq) {
    player.canInteractWithExit = true;
  } else {
    player.canInteractWithExit = false;
  }
}

//플레이어와 키워드 간의 상호작용 상태 업데이트 함수

function updateInteractionState(player, keywords) {
  //키워드를 이미 들고 있는 상태라면 상호작용 불가

  if (player.holdingKeywordId) {
    player.interactableKeywordId = null;

    return;
  }

  let closestDistSq = Infinity;
  let closestKeywordId = null;

  for (const id in keywords) {
    const keyword = keywords[id]; //다른 사람이 들고 있는 키워드는 무시하세요. //남의 포켓몬을 빼앗으면 도둑!

    if (keyword.carrierId) continue;

    const distSq = Physics.squareDistance(
      { x: player.x, y: player.y },
      { x: keyword.x, y: keyword.y }
    );

    if (distSq < Physics.interactionDistSq && distSq < closestDistSq) {
      closestDistSq = distSq;

      closestKeywordId = id;
    }
  }

  player.interactableKeywordId = closestKeywordId;
}

//setInterval 함수

function startGameUpdate(gameState, broadcast) {
  setInterval(() => {
    //게임 로직 업데이트 실행!

    update(gameState); //업데이트된 전체 상태를 모든 클라이언트에게 브로드캐스트!

    const packet = gameState.getFullStatePacket();

    broadcast(JSON.stringify(packet));
  }, deltaTime * 1000);
}

module.exports = { startGameUpdate };
