const fs = require("fs");
const path = require("path");

function loadMapFromFile(mapName) {
  try {
    const mapPath = path.join(__dirname, "..", "maps", `${mapName}.json`);
    const mapFileContent = fs.readFileSync(mapPath, "utf8");
    const mapData = JSON.parse(mapFileContent);
    console.log(`Map ${mapName} loaded successfully.`);
    return new Set(mapData.colliders.map((c) => `${c.x}, ${c.y}`));
  } catch (error) {
    console.error(`Failed to load map from file: ${mapName}`, error);
    throw error;
  }
}

module.exports = {
  loadMapFromFile,
};