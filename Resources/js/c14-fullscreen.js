(function() {
    console.log('C14 fullscreen script injected');

    let attempts = 0;

    const tryFull = () => {
        attempts++;
        console.log('Attempt', attempts);

        const iframe = document.getElementById('univ-live-iframe');
        if (!iframe) return;

        const doc = iframe.contentWindow?.document;
        if (!doc) return;

        const btn = doc.querySelector('.rgpl-btn-fullscreen');
        if (!btn) return;

        console.log('Clicking fullscreen');
        btn.click();
        clearInterval(window._c14FullInt);
    };

    window._c14FullInt = setInterval(tryFull, 500);
})();
