// Intersection-observer helper for reels scroll-snap detection.
// Called from Home.razor via IJSRuntime.
window.EverestFlixReels = {
    _observer: null,
    _dotnetRef: null,
    _currentTarget: null,

    initialise: function (dotnetRef, containerSelector, itemSelector) {
        this.dispose();
        this._dotnetRef = dotnetRef;

        const container = document.querySelector(containerSelector);
        if (!container) return;

        const options = {
            root: container,
            threshold: [0.6],   // fire when >=60% visible
            rootMargin: "0px"
        };

        this._observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    if (this._currentTarget && this._currentTarget !== entry.target) {
                        const prev = this._currentTarget.querySelector("video");
                        if (prev) prev.pause();
                    }
                    this._currentTarget = entry.target;

                    const v = entry.target.querySelector("video");
                    if (v) {
                        v.muted = true;                                // required for autoplay
                        v.play().catch(() => { /* browser blocked, no-op */ });
                    }

                    const id = entry.target.getAttribute("data-video-id");
                    if (id && this._dotnetRef) {
                        this._dotnetRef.invokeMethodAsync("OnReelChanged", parseInt(id, 10));
                    }
                }
            });
        }, options);

        container.querySelectorAll(itemSelector).forEach(el => this._observer.observe(el));
    },

    dispose: function () {
        if (this._observer) {
            this._observer.disconnect();
            this._observer = null;
        }
        this._dotnetRef = null;
        this._currentTarget = null;
    }
};