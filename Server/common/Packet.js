function makePacket(type, payload) {
    return JSON.stringify({
        type,
        payload: JSON.stringify(payload)
    })
}

module.exports = {makePacket};