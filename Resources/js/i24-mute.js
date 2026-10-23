(function() {
    // How long (ms) after each injection to keep retrying the mute.
    // Once this window closes the interval is cleared for good (for this page
    // load), so unmuting via the UI sticks and the feed is not re-muted later.
    const MAX_MUTE_MS = 30000; // 30 seconds

    const tryMute = () => {
        const vid = document.querySelector('video');
        if (vid) {
            vid.muted = true;
            vid.volume = 0;
            // Mute applied for this load; stop retrying.
            if (window._muteInt) {
                clearInterval(window._muteInt);
                window._muteInt = null;
            }
        }
    };

    // Retry every 500ms until the video element is ready, then mute once.
    window._muteInt = setInterval(tryMute, 500);

    // Safety net: stop trying after MAX_MUTE_MS even if the video never appeared
    // or after a successful mute, so this script never keeps running indefinitely.
    setTimeout(() => {
        if (window._muteInt) {
            clearInterval(window._muteInt);
            window._muteInt = null;
        }
    }, MAX_MUTE_MS);
})();
