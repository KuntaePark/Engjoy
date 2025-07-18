const preciseIntervals = new Map();
let nextPreciseIntervalId = 1;

function preciseSetInterval(callback, intervalMs) {
    const id = nextPreciseIntervalId++;
    let expected = performance.now() + intervalMs;

    function step() {
        if (!preciseIntervals.has(id)) return; // 이미 중단되었으면 종료

        const drift = performance.now() - expected;
        try {
            callback();
        }
        catch(err) {
            console.error(`preciseSetInterval callback error: ${err.stack || err}`);
            // 안전하게 타이머 종료
            clearPreciseInterval(id);
            return;
        }
        
    if (!preciseIntervals.has(id)) return; // 이미 중단되었으면 종료

        expected += intervalMs;

        // 다음 step 예약
        const timeoutId = setTimeout(step, Math.max(0, intervalMs - drift));
        preciseIntervals.set(id, timeoutId);
    }

    // 첫 step 예약
    const timeoutId = setTimeout(step, intervalMs);
    preciseIntervals.set(id, timeoutId);

    return id;
}

function clearPreciseInterval(id) {
    if (preciseIntervals.has(id)) {
        clearTimeout(preciseIntervals.get(id)); // 실제 timeout 제거
        preciseIntervals.delete(id);            // 관리용 map에서 제거
    }
}


module.exports = {preciseSetInterval, clearPreciseInterval}