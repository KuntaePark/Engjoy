const MonsterType = {
  RUNNER: "runner",
  CHASER: "chaser",
};

class Monster {
  constructor(id, x, y) {
    this.id = id;
    this.MonsterType = MonsterType;
    this.x = x;
    this.y = y;
    this.hp = hp;
    this.attacked = attacked;
  }

  toPacket() {
    return {
      id: this.id,
      MonsterType: this.MonsterType,
      x: this.x,
      y: this.y,
      hp: this.hp,
      attacked: this.attacked,
    };
  }
}

module.exports = { Monster };
