window.agtTheme = window.agtTheme || (() => {
    const storageKey = "agt-ui-theme";
    const navStoragePrefix = "agt-ui-nav-section-";

    function normalizeTheme(theme, fallback = "plum-dark") {
        const raw = (theme || "").toString().trim().toLowerCase();
        if (!raw) {
            return fallback;
        }

        if (raw === "dark") {
            return "plum-dark";
        }

        if (raw === "light") {
            return "plum-light";
        }

        return raw;
    }

    function supportsViewTransitions() {
        return typeof document !== "undefined" && typeof document.startViewTransition === "function";
    }

    function prefersReducedMotion() {
        return typeof window !== "undefined"
            && typeof window.matchMedia === "function"
            && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    }

    function applyTheme(theme) {
        const normalized = normalizeTheme(theme);
        document.documentElement.setAttribute("data-agt-theme", normalized);
        if (document.body) {
            document.body.setAttribute("data-agt-theme", normalized);
        }
        return normalized;
    }

    function persistTheme(theme, persist) {
        const normalized = applyTheme(theme);
        if (persist) {
            localStorage.setItem(storageKey, normalized);
        }

        return normalized;
    }

    function closePopup(popupId) {
        if (!popupId || !window.Radzen?.closePopup) {
            return;
        }

        window.Radzen.closePopup(popupId);
    }

    const popupSelector = [
        ".rz-popup",
        ".rz-dropdown-panel",
        ".rz-lookup-panel",
        ".rz-autocomplete-panel",
        ".rz-multiselect-panel",
        ".rz-datepicker-popup"
    ].join(", ");

    const dismissHandlers = new Map();
    let dismissCounter = 0;

    function hideOpenPopups() {
        document.querySelectorAll(popupSelector).forEach((panel) => {
            panel.style.display = "none";
        });
    }

    function closeAllPopups() {
        try {
            if (typeof window.Radzen?.closeAllPopups === "function") {
                window.Radzen.closeAllPopups();
            }
        } catch {
        }

        hideOpenPopups();

        if (typeof requestAnimationFrame === "function") {
            requestAnimationFrame(hideOpenPopups);
        }

        setTimeout(hideOpenPopups, 120);
    }

    function registerDismissHandler(panelElement, triggerElement, dotNetRef, methodName) {
        if (!panelElement || !dotNetRef) {
            return "";
        }

        const id = `dismiss-${++dismissCounter}`;
        const callbackMethod = (methodName || "").toString() || "CloseNotificationsFromJs";

        const onPointerDown = (event) => {
            const target = event.target;
            if (panelElement.contains(target) || (triggerElement && triggerElement.contains(target))) {
                return;
            }

            dotNetRef.invokeMethodAsync(callbackMethod).catch(() => { });
        };

        const onKeyDown = (event) => {
            if (event.key !== "Escape") {
                return;
            }

            dotNetRef.invokeMethodAsync(callbackMethod).catch(() => { });
        };

        document.addEventListener("mousedown", onPointerDown, true);
        document.addEventListener("keydown", onKeyDown, true);

        dismissHandlers.set(id, { onPointerDown, onKeyDown });
        return id;
    }

    function unregisterDismissHandler(id) {
        const key = (id || "").toString();
        if (!dismissHandlers.has(key)) {
            return;
        }

        const handler = dismissHandlers.get(key);
        document.removeEventListener("mousedown", handler.onPointerDown, true);
        document.removeEventListener("keydown", handler.onKeyDown, true);
        dismissHandlers.delete(key);
    }

    function focusElement(element) {
        if (!element) {
            return;
        }

        if (typeof element.focus === "function") {
            element.focus();
            return;
        }

        const focusTarget = element.querySelector?.("button, [role='button'], [tabindex]");
        if (focusTarget && typeof focusTarget.focus === "function") {
            focusTarget.focus();
        }
    }

    function normalizeNavState(value, fallback = "collapsed") {
        const raw = (value || "").toString().trim().toLowerCase();
        if (raw === "expanded" || raw === "collapsed") {
            return raw;
        }

        return fallback;
    }

    function getNavStorageKey(section) {
        const normalizedSection = (section || "").toString().trim().toLowerCase();
        return `${navStoragePrefix}${normalizedSection}`;
    }

    return {
        normalizeTheme,
        prefersReducedMotion,
        getStoredTheme(defaultTheme = "plum-dark") {
            const stored = localStorage.getItem(storageKey);
            if (!stored) {
                return normalizeTheme(defaultTheme);
            }

            return normalizeTheme(stored, defaultTheme);
        },
        setTheme(theme, persist = true) {
            return persistTheme(theme, persist);
        },
        setThemeWithTransition(theme, persist = true) {
            if (prefersReducedMotion() || !supportsViewTransitions()) {
                return persistTheme(theme, persist);
            }

            const normalized = normalizeTheme(theme);
            const transition = document.startViewTransition(() => persistTheme(normalized, persist));
            transition?.finished?.catch(() => { });
            return normalized;
        },
        closePopup,
        closeAllPopups,
        registerDismissHandler,
        unregisterDismissHandler,
        focusElement,
        getStoredNavSectionState(section, defaultState = "collapsed") {
            const key = getNavStorageKey(section);
            const fallback = normalizeNavState(defaultState, "collapsed");
            const stored = localStorage.getItem(key);
            return normalizeNavState(stored, fallback);
        },
        setStoredNavSectionState(section, state) {
            const key = getNavStorageKey(section);
            const normalized = normalizeNavState(state, "collapsed");
            localStorage.setItem(key, normalized);
            return normalized;
        },
        downloadCsv(fileName, csvContent) {
            const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
            const url = URL.createObjectURL(blob);
            const anchor = document.createElement("a");
            anchor.href = url;
            anchor.download = fileName || "export.csv";
            anchor.style.display = "none";
            document.body.appendChild(anchor);
            anchor.click();
            anchor.remove();
            URL.revokeObjectURL(url);
        },
        applyInitialTheme(defaultTheme = "plum-dark") {
            const normalized = this.getStoredTheme(defaultTheme);
            applyTheme(normalized);
            return normalized;
        }
    };
})();
