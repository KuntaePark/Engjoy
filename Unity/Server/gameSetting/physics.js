/*
    simple physics simulating library.
*/

class Vector2 {
  constructor(x, y) {
    this.x = x;
    this.y = y;
  }
}

//collide circle size
const colliderRadius = 0.25;

const keywordInteractionRadius = 1.0;

const interactionDistSq = (colliderRadius + keywordInteractionRadius) ** 2;

//collision checking function
function checkCollision(p1, p2) {
  //simple circle collision
  const diffX = Math.abs(p1.x - p2.x);
  const diffY = Math.abs(p1.y - p2.y);
  return diffX ** 2 + diffY ** 2 <= (colliderRadius * 2) ** 2;
}

function isPlayerCloseToKeyword(playerPosition, keywordPosition) {
  const distSq =
    (playerPosition.x - keywordPosition.x) ** 2 +
    (playerPosition.y - keywordPosition.y) ** 2;
  return distSq <= interactionDistSq;
}

function squareDistance(p1, p2) {
  return (p1.x - p2.x) ** 2 + (p1.y - p2.y) ** 2;
}

module.exports = {
  Vector2,
  checkCollision,
  isPlayerCloseToKeyword,
  squareDistance,
  interactionDistSq,
  colliderRadius,
};
