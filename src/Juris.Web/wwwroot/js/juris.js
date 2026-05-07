/* Juris client-side helpers
   Provides:
   - session id generation (anonymous tracking)
   - analytics tracking via fetch POST to /api/analytics
   - section dwell tracking on profile pages
*/
(function () {
    const STORAGE_SESSION = 'juris_session';
    const STORAGE_UNI = 'juris_uni';
    const STORAGE_YEAR = 'juris_year';

    function getSessionId() {
        let id = localStorage.getItem(STORAGE_SESSION);
        if (!id) {
            id = (crypto && crypto.randomUUID) ? crypto.randomUUID()
                : ('xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
                    const r = Math.random() * 16 | 0;
                    return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
                }));
            localStorage.setItem(STORAGE_SESSION, id);
        }
        return id;
    }

    function track(payload) {
        try {
            payload = Object.assign({
                sessionId: getSessionId(),
                university: localStorage.getItem(STORAGE_UNI) || null,
                yearOfStudy: localStorage.getItem(STORAGE_YEAR) || null,
                path: window.location.pathname,
                referrer: document.referrer || null
            }, payload || {});
            const blob = new Blob([JSON.stringify(payload)], { type: 'application/json' });
            if (navigator.sendBeacon) {
                navigator.sendBeacon('/api/analytics/track', blob);
            } else {
                fetch('/api/analytics/track', {
                    method: 'POST',
                    body: blob,
                    keepalive: true,
                    headers: { 'Content-Type': 'application/json' }
                }).catch(() => { });
            }
        } catch (e) { /* silent */ }
    }

    window.juris = {
        track: track,
        sessionId: getSessionId,
        setIdentity: function (university, yearOfStudy) {
            if (university) localStorage.setItem(STORAGE_UNI, university);
            if (yearOfStudy) localStorage.setItem(STORAGE_YEAR, yearOfStudy);
        }
    };

    // Auto: page view
    document.addEventListener('DOMContentLoaded', function () {
        track({ type: 'PageView' });
    });

    // Section dwell tracker — observes [data-track-section] elements
    document.addEventListener('DOMContentLoaded', function () {
        const dwell = new Map();
        const io = new IntersectionObserver(entries => {
            entries.forEach(e => {
                const key = e.target.getAttribute('data-track-section');
                if (!key) return;
                if (e.isIntersecting) {
                    dwell.set(key, { start: Date.now(), firmId: e.target.getAttribute('data-firm-id') });
                } else if (dwell.has(key)) {
                    const d = dwell.get(key);
                    const seconds = Math.round((Date.now() - d.start) / 1000);
                    if (seconds > 0) {
                        track({
                            type: 'SectionDwell',
                            sectionKey: key,
                            dwellSeconds: seconds,
                            firmId: d.firmId
                        });
                    }
                    dwell.delete(key);
                }
            });
        }, { threshold: 0.5 });

        document.querySelectorAll('[data-track-section]').forEach(el => io.observe(el));

        // On unload, flush any remaining dwell sections
        window.addEventListener('pagehide', function () {
            dwell.forEach((d, key) => {
                const seconds = Math.round((Date.now() - d.start) / 1000);
                if (seconds > 0) {
                    track({ type: 'SectionDwell', sectionKey: key, dwellSeconds: seconds, firmId: d.firmId });
                }
            });
        });
    });

    // Auto: track [data-track-click="EventType"] elements
    document.addEventListener('click', function (e) {
        const target = e.target.closest('[data-track-click]');
        if (!target) return;
        const eventType = target.getAttribute('data-track-click');
        const firmId = target.getAttribute('data-firm-id');
        const schoolId = target.getAttribute('data-school-id');
        const articleId = target.getAttribute('data-article-id');
        const meta = target.getAttribute('data-track-meta');
        const sectionKey = target.getAttribute('data-section-key');
        track({
            type: eventType,
            firmId: firmId,
            schoolId: schoolId,
            articleId: articleId,
            sectionKey: sectionKey,
            metadataJson: meta
        });
    });
})();
