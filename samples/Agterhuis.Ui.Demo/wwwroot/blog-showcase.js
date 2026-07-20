window.blogShowcase = window.blogShowcase || (() => {
    const revealObservers = new Map();
    const articleHandlers = new Map();
    const reelHandlers = new Map();

    function prefersReducedMotion() {
        return window.agtTheme?.prefersReducedMotion?.() === true;
    }

    function getSessionKey(element, suffix) {
        const id = element.id || element.getAttribute("data-blog-key") || "blog-item";
        return `blog-${suffix}-${id}`;
    }

    function revealImmediately(root) {
        root.querySelectorAll("[data-blog-reveal]").forEach((item) => item.classList.add("is-visible"));
        root.querySelectorAll("[data-blog-terminal]").forEach((item) => {
            const text = item.getAttribute("data-terminal-final") || item.textContent || "";
            item.textContent = text;
            item.classList.add("is-typed");
        });
    }

    function ensureBlogKeys(root) {
        root.querySelectorAll("[data-blog-reveal]").forEach((item, index) => {
            if (!item.getAttribute("data-blog-key")) {
                item.setAttribute("data-blog-key", `reveal-${index}`);
            }
        });

        root.querySelectorAll("[data-blog-terminal]").forEach((item, index) => {
            if (!item.getAttribute("data-blog-key")) {
                item.setAttribute("data-blog-key", `terminal-${index}`);
            }
        });
    }

    function typeTerminal(element) {
        const doneKey = getSessionKey(element, "typed");
        if (sessionStorage.getItem(doneKey) === "1") {
            element.textContent = element.getAttribute("data-terminal-final") || element.textContent || "";
            element.classList.add("is-typed");
            return;
        }

        if (element.dataset.typing === "running") {
            return;
        }

        const script = element.getAttribute("data-terminal-script") || "";
        if (!script) {
            return;
        }

        element.dataset.typing = "running";
        element.textContent = "";
        const chars = script.split("");
        let index = 0;

        const timer = window.setInterval(() => {
            if (element.dataset.typing !== "running") {
                window.clearInterval(timer);
                return;
            }

            element.textContent += chars[index] || "";
            index += 1;
            if (index >= chars.length) {
                window.clearInterval(timer);
                element.dataset.typing = "done";
                element.classList.add("is-typed");
                element.setAttribute("data-terminal-final", script);
                sessionStorage.setItem(doneKey, "1");
            }
        }, 14);

        element.dataset.typingTimer = `${timer}`;
    }

    function pauseTerminal(element) {
        const timerRaw = element.dataset.typingTimer;
        if (!timerRaw) {
            return;
        }

        const timer = Number.parseInt(timerRaw, 10);
        if (Number.isFinite(timer)) {
            window.clearInterval(timer);
        }

        if (element.dataset.typing === "running") {
            element.dataset.typing = "paused";
        }
    }

    function initMotion(rootSelector) {
        const root = document.querySelector(rootSelector);
        if (!root) {
            return false;
        }

        ensureBlogKeys(root);

        if (prefersReducedMotion()) {
            revealImmediately(root);
            return true;
        }

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                const target = entry.target;
                if (entry.isIntersecting) {
                    if (target.hasAttribute("data-blog-reveal")) {
                        const once = target.getAttribute("data-blog-reveal-once") !== "false";
                        target.classList.add("is-visible");
                        if (once) {
                            sessionStorage.setItem(getSessionKey(target, "reveal"), "1");
                        }

                        if (once) {
                            observer.unobserve(target);
                        }
                    }

                    if (target.hasAttribute("data-blog-terminal")) {
                        typeTerminal(target);
                    }

                    return;
                }

                if (target.hasAttribute("data-blog-terminal")) {
                    pauseTerminal(target);
                }
            });
        }, {
            root: null,
            threshold: 0.16,
            rootMargin: "0px 0px -8% 0px"
        });

        root.querySelectorAll("[data-blog-reveal], [data-blog-terminal]").forEach((item) => {
            if (item.hasAttribute("data-blog-reveal")) {
                const once = item.getAttribute("data-blog-reveal-once") !== "false";
                if (once && sessionStorage.getItem(getSessionKey(item, "reveal")) === "1") {
                    item.classList.add("is-visible");
                    return;
                }
            }

            if (item.hasAttribute("data-blog-terminal") && sessionStorage.getItem(getSessionKey(item, "typed")) === "1") {
                const finalText = item.getAttribute("data-terminal-final") || item.getAttribute("data-terminal-script") || item.textContent || "";
                item.textContent = finalText;
                item.classList.add("is-typed");
                return;
            }

            observer.observe(item);
        });

        initReels(root, rootSelector);

        revealObservers.set(rootSelector, observer);
        return true;
    }

    function disposeMotion(rootSelector) {
        const observer = revealObservers.get(rootSelector);
        if (!observer) {
            return;
        }

        observer.disconnect();
        revealObservers.delete(rootSelector);

        const disposeReels = reelHandlers.get(rootSelector);
        if (disposeReels) {
            disposeReels();
            reelHandlers.delete(rootSelector);
        }
    }

    function initReels(root, rootSelector) {
        const tracks = Array.from(root.querySelectorAll("[data-blog-reel]"));
        if (tracks.length === 0) {
            return;
        }

        const cleanups = [];

        tracks.forEach((track) => {
            let dragging = false;
            let pointerId = null;
            let startX = 0;
            let startScrollLeft = 0;

            const updateEdgeState = () => {
                const parent = track.closest(".blog-reel");
                if (!parent) {
                    return;
                }

                const maxScroll = track.scrollWidth - track.clientWidth;
                parent.classList.toggle("is-at-start", track.scrollLeft <= 4);
                parent.classList.toggle("is-at-end", maxScroll <= 4 || track.scrollLeft >= maxScroll - 4);
            };

            const onPointerDown = (event) => {
                if (window.matchMedia("(max-width: 767px)").matches) {
                    return;
                }

                if (event.pointerType === "mouse" && event.button !== 0) {
                    return;
                }

                dragging = true;
                pointerId = event.pointerId;
                startX = event.clientX;
                startScrollLeft = track.scrollLeft;
                track.classList.add("is-dragging");

                if (typeof track.setPointerCapture === "function") {
                    track.setPointerCapture(pointerId);
                }
            };

            const onPointerMove = (event) => {
                if (!dragging) {
                    return;
                }

                const delta = event.clientX - startX;
                track.scrollLeft = startScrollLeft - delta;
            };

            const onPointerUp = (event) => {
                if (!dragging) {
                    return;
                }

                dragging = false;
                track.classList.remove("is-dragging");

                if (pointerId !== null && typeof track.releasePointerCapture === "function") {
                    try {
                        track.releasePointerCapture(pointerId);
                    } catch {
                        // Ignore capture release failures.
                    }
                }

                pointerId = null;
                updateEdgeState();
            };

            const onResize = () => updateEdgeState();

            track.addEventListener("pointerdown", onPointerDown);
            track.addEventListener("pointermove", onPointerMove);
            track.addEventListener("pointerup", onPointerUp);
            track.addEventListener("pointercancel", onPointerUp);
            track.addEventListener("scroll", updateEdgeState, { passive: true });
            window.addEventListener("resize", onResize);

            updateEdgeState();

            cleanups.push(() => {
                track.removeEventListener("pointerdown", onPointerDown);
                track.removeEventListener("pointermove", onPointerMove);
                track.removeEventListener("pointerup", onPointerUp);
                track.removeEventListener("pointercancel", onPointerUp);
                track.removeEventListener("scroll", updateEdgeState);
                window.removeEventListener("resize", onResize);
                track.classList.remove("is-dragging");
            });
        });

        if (cleanups.length > 0) {
            reelHandlers.set(rootSelector, () => {
                cleanups.forEach((cleanup) => cleanup());
            });
        }
    }

    function initArticle(rootSelector) {
        const root = document.querySelector(rootSelector);
        if (!root) {
            return false;
        }

        const progressBar = document.querySelector("[data-blog-progress]");
        const sections = Array.from(root.querySelectorAll("section[id]"));
        const tocLinks = Array.from(root.querySelectorAll("[data-blog-toc-link]"));

        if (sections.length === 0) {
            return false;
        }

        const updateProgress = () => {
            const rect = root.getBoundingClientRect();
            const scrollTop = window.scrollY + window.innerHeight * 0.16;
            const start = window.scrollY + rect.top;
            const end = start + root.scrollHeight - window.innerHeight;
            const ratio = end <= start ? 1 : Math.min(1, Math.max(0, (scrollTop - start) / (end - start)));

            if (progressBar) {
                progressBar.style.setProperty("--blog-progress", `${(ratio * 100).toFixed(2)}%`);
            }
        };

        const spy = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) {
                    return;
                }

                const activeId = entry.target.id;
                tocLinks.forEach((link) => {
                    const isCurrent = link.getAttribute("href") === `#${activeId}`;
                    if (isCurrent) {
                        link.setAttribute("aria-current", "location");
                    } else {
                        link.removeAttribute("aria-current");
                    }
                });
            });
        }, {
            threshold: 0.5,
            rootMargin: "-15% 0px -50% 0px"
        });

        sections.forEach((section) => spy.observe(section));
        updateProgress();
        window.addEventListener("scroll", updateProgress, { passive: true });

        articleHandlers.set(rootSelector, { spy, updateProgress });
        return true;
    }

    function disposeArticle(rootSelector) {
        const handler = articleHandlers.get(rootSelector);
        if (!handler) {
            return;
        }

        handler.spy.disconnect();
        window.removeEventListener("scroll", handler.updateProgress);
        articleHandlers.delete(rootSelector);
    }

    async function copyText(text) {
        if (!navigator.clipboard || !navigator.clipboard.writeText) {
            return false;
        }

        try {
            await navigator.clipboard.writeText(text || "");
            return true;
        } catch {
            return false;
        }
    }

    function navigateWithTransition(url) {
        const target = (url || "").toString();
        if (!target) {
            return false;
        }

        if (prefersReducedMotion() || typeof document.startViewTransition !== "function" || typeof window.Blazor?.navigateTo !== "function") {
            window.Blazor?.navigateTo?.(target, false, false);
            return true;
        }

        try {
            document.startViewTransition(() => {
                window.Blazor.navigateTo(target, false, false);
            });
            return true;
        } catch {
            window.Blazor?.navigateTo?.(target, false, false);
            return true;
        }
    }

    return {
        initMotion,
        disposeMotion,
        initArticle,
        disposeArticle,
        copyText,
        navigateWithTransition
    };
})();
