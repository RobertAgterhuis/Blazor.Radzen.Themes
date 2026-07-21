window.designerInterop = (() => {
    const designerAssetBaseUrl = new URL(".", document.currentScript?.src ?? document.baseURI).href;
    let monacoLoaderPromise;
    let requirePromise;

    const ensureScript = (src) => new Promise((resolve, reject) => {
        const existing = document.querySelector(`script[data-agt-src="${src}"]`);
        if (existing) {
            existing.addEventListener("load", () => resolve(), { once: true });
            existing.addEventListener("error", (error) => reject(error), { once: true });
            if (existing.dataset.loaded === "true") {
                resolve();
            }

            return;
        }

        const script = document.createElement("script");
        script.src = src;
        script.async = true;
        script.dataset.agtSrc = src;
        script.addEventListener("load", () => {
            script.dataset.loaded = "true";
            resolve();
        }, { once: true });
        script.addEventListener("error", (error) => reject(error), { once: true });
        document.head.appendChild(script);
    });

    const ensureMonaco = async () => {
        if (window.monaco?.editor) {
            return window.monaco;
        }

        monacoLoaderPromise ??= (async () => {
            const loaderPath = "/lib/monaco-editor/min/vs/loader.js";
            await ensureScript(loaderPath);

            requirePromise ??= new Promise((resolve, reject) => {
                const amdRequire = window.require;
                if (!amdRequire) {
                    reject(new Error("Monaco loader did not expose window.require."));
                    return;
                }

                amdRequire.config({ paths: { vs: "/lib/monaco-editor/min/vs" } });
                amdRequire(["vs/editor/editor.main"], () => resolve(window.monaco), reject);
            });

            return await requirePromise;
        })();

        return await monacoLoaderPromise;
    };

    const createMonacoEditor = async (element, options) => {
        if (!element) {
            return null;
        }

        const monaco = await ensureMonaco();
        return monaco.editor.create(element, options);
    };

    const setMonacoTheme = async (themeName, definition) => {
        const monaco = await ensureMonaco();
        monaco.editor.defineTheme(themeName, definition);
        monaco.editor.setTheme(themeName);
    };

    const tryParse = (raw, fallback) => {
        if (!raw) {
            return fallback;
        }

        try {
            return JSON.parse(raw);
        }
        catch {
            return fallback;
        }
    };

    const setJson = (key, value) => {
        const payload = typeof value === "string" ? value : JSON.stringify(value);
        localStorage.setItem(key, payload);
    };

    const removeItem = (key) => {
        localStorage.removeItem(key);
    };

    const saveBytesFile = async (fileName, mimeType, bytes) => {
        const blob = new Blob([bytes], { type: mimeType || "application/octet-stream" });

        if (window.showSaveFilePicker) {
            const handle = await window.showSaveFilePicker({
                suggestedName: fileName,
                types: [{ description: mimeType || "file", accept: { [mimeType || "application/octet-stream"]: [fileName.slice(fileName.lastIndexOf(".")) || ""] } }]
            });

            const writable = await handle.createWritable();
            await writable.write(blob);
            await writable.close();
            return;
        }

        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
    };

    const saveDesignDocument = async (fileName, json) => {
        const blob = new Blob([json], { type: "application/json" });

        if (window.showSaveFilePicker) {
            const handle = await window.showSaveFilePicker({
                suggestedName: fileName,
                types: [{ description: "Agterhuis designer document", accept: { "application/json": [".agtdesign", ".json"] } }]
            });

            const writable = await handle.createWritable();
            await writable.write(blob);
            await writable.close();
            return;
        }

        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
    };

    const pickDesignDocument = async () => {
        if (!window.showOpenFilePicker) {
            return null;
        }

        const handles = await window.showOpenFilePicker({
            types: [{ description: "Agterhuis designer document", accept: { "application/json": [".agtdesign", ".json"] } }],
            excludeAcceptAllOption: false,
            multiple: false
        });

        if (!handles || handles.length === 0) {
            return null;
        }

        const file = await handles[0].getFile();
        return await file.text();
    };

    const getJson = (key) => {
        return tryParse(localStorage.getItem(key), []);
    };

    const getText = (key) => {
        return localStorage.getItem(key) || "";
    };

    const registerKeyScope = (dotNetRef, element) => {
        if (!element) {
            return;
        }

        element.addEventListener("keydown", (event) => {
            if (event.key === "Escape") {
                dotNetRef.invokeMethodAsync("HandleCanvasKey", event.key);
            }
        });
    };

    const scrollTreeItemIntoView = (nodeId) => {
        if (!nodeId) {
            return;
        }

        const target = document.querySelector(`[data-agt-tree-node-id="${nodeId}"]`);
        if (!target) {
            return;
        }

        target.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    };

    let codeEditor = null;
    let jsonEditor = null;
    let codeEditorChangeTimeout = null;

    const setupCodeEditors = async (dotnetRef, codeContainer, jsonContainer) => {
        const monaco = await ensureMonaco();
        if (codeContainer) {
            codeEditor = monaco.editor.create(codeContainer, {
                value: `<!-- Razor code will load here -->`,
                language: 'html',
                theme: 'vs',
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                automaticLayout: true,
                tabSize: 2,
                wordWrap: 'on',
                readOnly: true
            });
        }

        if (jsonContainer) {
            jsonEditor = monaco.editor.create(jsonContainer, {
                value: `{}`,
                language: 'json',
                theme: 'vs',
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                automaticLayout: true,
                tabSize: 2,
                wordWrap: 'on',
                readOnly: false
            });

            jsonEditor.onDidChangeModelContent(() => {
                clearTimeout(codeEditorChangeTimeout);
                codeEditorChangeTimeout = setTimeout(() => {
                    const json = jsonEditor.getValue();
                    dotnetRef.invokeMethodAsync('OnJsonEditorChanged', json);
                }, 500);
            });
        }
    };

    const updateCodeEditor = (code) => {
        if (codeEditor) {
            const current = codeEditor.getValue();
            if (current !== code) {
                codeEditor.setValue(code);
            }
        }
    };

    const updateJsonEditor = (json) => {
        if (jsonEditor) {
            const current = jsonEditor.getValue();
            if (current !== json) {
                jsonEditor.setValue(json);
            }
        }
    };

    const switchCodeTab = (tabName) => {
        const codePanel = document.querySelector('.designer-code-panel');
        const tabs = codePanel?.querySelectorAll('[role="tab"]');
        const panels = codePanel?.querySelectorAll('[role="tabpanel"]');

        if (tabs && panels) {
            tabs.forEach(tab => {
                tab.setAttribute('aria-selected', tab.getAttribute('data-tab') === tabName ? 'true' : 'false');
            });
            panels.forEach(panel => {
                panel.style.display = panel.getAttribute('data-panel') === tabName ? 'flex' : 'none';
            });

            if (codeEditor) codeEditor.layout();
            if (jsonEditor) jsonEditor.layout();
        }
    };

    const setupResizablePanels = async () => {
        const { setupResizablePanels: setupResize } = await import(new URL('designer-resize-interop.js', designerAssetBaseUrl).href);
        return setupResize();
    };

    const setEditorTheme = async (isDark) => {
        const monaco = await ensureMonaco();
        const themeName = isDark ? 'vs-dark' : 'vs';
        monaco.editor.setTheme(themeName);
    };

    const scrollToPropertyParameter = (parameterName) => {
        if (!parameterName) {
            return;
        }

        const selector = `[data-agt-designer-param="${parameterName}"]`;
        const target = document.querySelector(selector);
        if (!target) {
            return;
        }

        target.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        target.classList.add('designer-properties__field--highlight');
        window.setTimeout(() => target.classList.remove('designer-properties__field--highlight'), 900);
    };

    const setPaletteDragImage = (icon, label) => {
        window.addEventListener('dragstart', (event) => {
            const target = event.target;
            if (!(target instanceof HTMLElement) || !target.classList.contains('designer-palette-item')) {
                return;
            }

            const ghost = document.createElement('div');
            ghost.className = 'designer-drag-ghost';
            ghost.innerHTML = `<span class="rzi" aria-hidden="true">${icon ?? 'widgets'}</span><span>${label ?? 'Component'}</span>`;
            document.body.appendChild(ghost);
            event.dataTransfer?.setDragImage(ghost, 14, 14);
            window.setTimeout(() => ghost.remove(), 0);
        }, { once: true });
    };

    const flashNode = (nodeId) => {
        if (!nodeId) {
            return;
        }

        const node = document.querySelector(`[data-agt-design-node-id="${nodeId}"]`);
        if (!(node instanceof HTMLElement)) {
            return;
        }

        node.classList.add('designer-canvas-node--flash');
        node.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        window.setTimeout(() => node.classList.remove('designer-canvas-node--flash'), 320);
    };

    return {
        createMonacoEditor,
        getJson,
        getText,
        registerKeyScope,
        removeItem,
        pickDesignDocument,
        saveBytesFile,
        saveDesignDocument,
        setPaletteDragImage,
        setEditorTheme,
        scrollToPropertyParameter,
        flashNode,
        setMonacoTheme,
        setJson,
        setupCodeEditors,
        updateCodeEditor,
        updateJsonEditor,
        scrollTreeItemIntoView,
        switchCodeTab,
        setupResizablePanels
    };
})();
