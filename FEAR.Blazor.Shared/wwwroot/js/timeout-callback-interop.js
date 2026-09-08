function setTimeoutCallback(dotNetObject, methodName, delay) {
    if (typeof dotNetObject !== 'object' || typeof methodName !== 'string' || typeof delay !== 'number') {
        throw new Error('Invalid arguments provided to setTimeoutCallback');
    }
    const callback = () => {
        dotNetObject.invokeMethodAsync(methodName)
            .catch(error => console.error('Error invoking method:', error));
    };
    return setTimeout(callback, delay);
}

function clearTimeoutCallback(timeoutId) {
    if (typeof timeoutId !== 'number') {
        throw new Error('Invalid timeoutId provided to clearTimeoutCallback');
    }
    clearTimeout(timeoutId);
}

function setIntervalCallback(dotNetObject, methodName, interval) {
    if (typeof dotNetObject !== 'object' || typeof methodName !== 'string' || typeof interval !== 'number') {
        throw new Error('Invalid arguments provided to setIntervalCallback');
    }
    const callback = () => {
        dotNetObject.invokeMethodAsync(methodName)
            .catch(error => console.error('Error invoking method:', error));
    };
    return setInterval(callback, interval);
}

function clearIntervalCallback(intervalId) {
    if (typeof intervalId !== 'number') {
        throw new Error('Invalid intervalId provided to clearIntervalCallback');
    }
    clearInterval(intervalId);
}