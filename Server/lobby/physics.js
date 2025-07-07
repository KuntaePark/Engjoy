export {colliderRadius}
export {Vector2, checkCollision}

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

//collision checking function
function checkCollision(p1, p2) {
    //simple circle collision
    const diffX = Math.abs(p1.x - p2.x);
    const diffY = Math.abs(p1.y - p2.y);
    return (diffX ** 2 + diffY ** 2) <= (colliderRadius * 2) ** 2;
}

//calculate distance squared
function squareDistance(p1, p2) {
    return (p1.x - p2.x) ** 2 + (p1.y - p2.y) ** 2;
}