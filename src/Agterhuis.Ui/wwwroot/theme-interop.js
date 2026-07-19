window.agtTheme = window.agtTheme || (() => {
    const storageKey = "agt-ui-theme";
    const densityStorageKey = "agt-ui-density";
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

    function isViewportAtMost(maxWidth) {
        const normalized = Number(maxWidth);
        if (typeof window === "undefined" || Number.isNaN(normalized)) {
            return false;
        }

        return window.innerWidth <= normalized;
    }

    function normalizeDensity(density, fallback = "comfortable") {
        const raw = (density || "").toString().trim().toLowerCase();
        if (raw === "compact") {
            return "compact";
        }

        return fallback === "compact" ? "compact" : "comfortable";
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

    function applyDensity(density) {
        const normalized = normalizeDensity(density);
        document.documentElement.setAttribute("data-agt-density", normalized);
        if (document.body) {
            document.body.setAttribute("data-agt-density", normalized);
        }

        return normalized;
    }

    function persistDensity(density, persist) {
        const normalized = applyDensity(density);
        if (persist) {
            localStorage.setItem(densityStorageKey, normalized);
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
    const shortcutHandlers = new Map();
    const focusTrapHandlers = new Map();
    let dismissCounter = 0;
    let shortcutCounter = 0;
    let focusTrapCounter = 0;

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

    function registerGlobalShortcut(dotNetRef, methodName) {
        if (!dotNetRef) {
            return "";
        }

        const id = `shortcut-${++shortcutCounter}`;
        const callbackMethod = (methodName || "").toString() || "OpenFromJs";

        const onKeyDown = (event) => {
            const key = (event.key || "").toLowerCase();
            if (key !== "k" || !(event.ctrlKey || event.metaKey) || event.altKey) {
                return;
            }

            event.preventDefault();
            dotNetRef.invokeMethodAsync(callbackMethod).catch(() => { });
        };

        document.addEventListener("keydown", onKeyDown, true);
        shortcutHandlers.set(id, { onKeyDown });
        return id;
    }

    function unregisterGlobalShortcut(id) {
        const key = (id || "").toString();
        if (!shortcutHandlers.has(key)) {
            return;
        }

        const handler = shortcutHandlers.get(key);
        document.removeEventListener("keydown", handler.onKeyDown, true);
        shortcutHandlers.delete(key);
    }

    function getFocusableElements(panelElement) {
        if (!panelElement) {
            return [];
        }

        const selector = [
            "a[href]",
            "button:not([disabled])",
            "textarea:not([disabled])",
            "input:not([disabled])",
            "select:not([disabled])",
            "[tabindex]:not([tabindex='-1'])"
        ].join(", ");

        return Array.from(panelElement.querySelectorAll(selector)).filter((element) => {
            const hiddenByLayout = element.offsetParent === null && element !== document.activeElement;
            return !element.hasAttribute("disabled") && !hiddenByLayout;
        });
    }

    function registerFocusTrap(panelElement) {
        if (!panelElement) {
            return "";
        }

        const id = `focus-trap-${++focusTrapCounter}`;

        const onKeyDown = (event) => {
            if (event.key !== "Tab") {
                return;
            }

            const focusables = getFocusableElements(panelElement);
            if (focusables.length === 0) {
                event.preventDefault();
                return;
            }

            const first = focusables[0];
            const last = focusables[focusables.length - 1];
            const active = document.activeElement;

            if (event.shiftKey) {
                if (!panelElement.contains(active) || active === first) {
                    event.preventDefault();
                    last.focus();
                }

                return;
            }

            if (!panelElement.contains(active) || active === last) {
                event.preventDefault();
                first.focus();
            }
        };

        document.addEventListener("keydown", onKeyDown, true);
        focusTrapHandlers.set(id, { onKeyDown });
        return id;
    }

    function unregisterFocusTrap(id) {
        const key = (id || "").toString();
        if (!focusTrapHandlers.has(key)) {
            return;
        }

        const handler = focusTrapHandlers.get(key);
        document.removeEventListener("keydown", handler.onKeyDown, true);
        focusTrapHandlers.delete(key);
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

    function getVisibleNavLinks(containerElement) {
        if (!containerElement) {
            return [];
        }

        return Array.from(containerElement.querySelectorAll(".rz-navigation-item-link"))
            .filter((link) => !link.closest(".demo-panel-menu__section-title, .demo-panel-menu__subtitle"))
            .filter((link) => !link.hasAttribute("disabled") && (link.offsetParent !== null || link === document.activeElement));
    }

    function focusFirstNavItem(containerElement) {
        const links = getVisibleNavLinks(containerElement);
        if (links.length === 0) {
            return false;
        }

        focusElement(links[0]);
        return true;
    }

    function focusNavItem(containerElement, direction) {
        const links = getVisibleNavLinks(containerElement);
        if (links.length === 0) {
            return false;
        }

        const active = document.activeElement;
        const currentIndex = links.findIndex((link) => link === active);
        const normalizedDirection = (direction || "").toString().trim().toLowerCase();

        let nextIndex = 0;
        if (normalizedDirection === "last") {
            nextIndex = links.length - 1;
        } else if (normalizedDirection === "next") {
            nextIndex = currentIndex >= 0 ? Math.min(currentIndex + 1, links.length - 1) : 0;
        } else if (normalizedDirection === "prev" || normalizedDirection === "previous") {
            nextIndex = currentIndex >= 0 ? Math.max(currentIndex - 1, 0) : links.length - 1;
        } else {
            nextIndex = 0;
        }

        focusElement(links[nextIndex]);
        return true;
    }

    function activateFocusedNavItem(containerElement) {
        if (!containerElement) {
            return false;
        }

        const active = document.activeElement;
        if (!active || !containerElement.contains(active) || !active.classList?.contains("rz-navigation-item-link")) {
            return false;
        }

        active.click();
        return true;
    }

    function applyNavItemTitles(containerElement) {
        if (!containerElement) {
            return;
        }

        const links = containerElement.querySelectorAll(".rz-navigation-item-link");
        links.forEach((link) => {
            const textNode = link.querySelector(".rz-navigation-item-text");
            const label = (textNode?.textContent || "").trim();
            if (!label) {
                link.removeAttribute("title");
                return;
            }

            link.setAttribute("title", label);
        });
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

    function placeContextLink(mainSelector = "#main") {
        const selector = (mainSelector || "").toString().trim() || "#main";
        const main = document.querySelector(selector);
        if (!main) {
            return false;
        }

        const contextLink = main.querySelector("[data-role='page-context-link']");
        if (!contextLink) {
            return false;
        }

        const header = main.querySelector(".agt-page-header");
        if (header) {
            const content = header.querySelector(".agt-page-header__content") || header;
            const description = content.querySelector(".agt-page-header__description");
            if (description) {
                description.insertAdjacentElement("afterend", contextLink);
            } else {
                content.appendChild(contextLink);
            }

            return true;
        }

        const catalogPage = main.querySelector(".catalog-page");
        if (catalogPage) {
            const intro = catalogPage.querySelector("p");
            if (intro) {
                intro.insertAdjacentElement("afterend", contextLink);
            } else {
                const title = catalogPage.querySelector("h1, h2");
                if (title) {
                    title.insertAdjacentElement("afterend", contextLink);
                } else {
                    catalogPage.prepend(contextLink);
                }
            }

            return true;
        }

        return false;
    }

    return {
        normalizeDensity,
        normalizeTheme,
        prefersReducedMotion,
        isViewportAtMost,
        getStoredTheme(defaultTheme = "plum-dark") {
            const stored = localStorage.getItem(storageKey);
            if (!stored) {
                return normalizeTheme(defaultTheme);
            }

            return normalizeTheme(stored, defaultTheme);
        },
        getStoredDensity(defaultDensity = "comfortable") {
            const stored = localStorage.getItem(densityStorageKey);
            if (!stored) {
                return normalizeDensity(defaultDensity, defaultDensity);
            }

            return normalizeDensity(stored, defaultDensity);
        },
        setTheme(theme, persist = true) {
            return persistTheme(theme, persist);
        },
        setDensity(density, persist = true) {
            return persistDensity(density, persist);
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
        registerGlobalShortcut,
        unregisterGlobalShortcut,
        registerFocusTrap,
        unregisterFocusTrap,
        focusElement,
        focusFirstNavItem,
        focusNavItem,
        activateFocusedNavItem,
        applyNavItemTitles,
        placeContextLink,
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
        getStoredValue(key, fallback = "") {
            const storageKeyValue = (key || "").toString();
            if (!storageKeyValue) {
                return fallback;
            }

            const value = localStorage.getItem(storageKeyValue);
            return value ?? fallback;
        },
        setStoredValue(key, value) {
            const storageKeyValue = (key || "").toString();
            if (!storageKeyValue) {
                return;
            }

            localStorage.setItem(storageKeyValue, (value || "").toString());
        },
        removeStoredValue(key) {
            const storageKeyValue = (key || "").toString();
            if (!storageKeyValue) {
                return;
            }

            localStorage.removeItem(storageKeyValue);
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
        },
        applyInitialDensity(defaultDensity = "comfortable") {
            const normalized = this.getStoredDensity(defaultDensity);
            applyDensity(normalized);
            return normalized;
        }
    };
})();
