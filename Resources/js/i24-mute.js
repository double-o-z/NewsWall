(function() {
    const tryMute = () => {
        const vid = document.querySelector('video');
        if (vid) {
            vid.muted = true;
            vid.volume = 0;
            clearInterval(window._muteInt);
        }
    };
    window._muteInt = setInterval(tryMute, 500);
})();
