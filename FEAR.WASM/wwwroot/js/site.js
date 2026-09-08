function scrollToBottom(elementId) {
    setTimeout(() => {
        var element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    }, 200);
}

window.saveAsFile = function (fileName, contentBase64) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:application/octet-stream;base64,' + contentBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};