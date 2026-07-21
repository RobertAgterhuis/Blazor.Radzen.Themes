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

    let blazorRef = null;

    const setupDragAndDrop = (dotnetHelper) => {
        blazorRef = dotnetHelper;
        let activeDrag = null;
        const paletteItems = document.querySelectorAll('.designer-palette-item');
        paletteItems.forEach(item => {
            item.addEventListener('dragstart', (e) => {
                const componentType = item.title;
                activeDrag = { type: 'palette', value: componentType };
                e.dataTransfer.effectAllowed = 'copy';
                e.dataTransfer.setData('text/plain', componentType);
                item.style.opacity = '0.6';
            }, false);

            item.addEventListener('dragend', () => {
                item.style.opacity = '1';
                activeDrag = null;
            }, false);
        });

        const dropzones = document.querySelectorAll('.designer-dropzone');
        dropzones.forEach((dropzone, index) => {
            dropzone.addEventListener('dragover', (e) => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'copy';
                dropzone.classList.add('designer-dropzone--drag-over');
            }, false);

            dropzone.addEventListener('dragleave', (e) => {
                if (e.target === dropzone) {
                    dropzone.classList.remove('designer-dropzone--drag-over');
                }
            }, false);

            dropzone.addEventListener('drop', (e) => {
                e.preventDefault();
                e.stopPropagation();
                dropzone.classList.remove('designer-dropzone--drag-over');
                const componentType = e.dataTransfer.getData('text/plain');
                if (activeDrag && blazorRef) {
                    dotnetHelper.invokeMethodAsync('OnJavaScriptDrop', componentType, index);
                }
            }, false);
        });
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
                readOnly: false
            });

            codeEditor.onDidChangeModelContent(() => {
                clearTimeout(codeEditorChangeTimeout);
                codeEditorChangeTimeout = setTimeout(() => {
                    const code = codeEditor.getValue();
                    dotnetRef.invokeMethodAsync('OnCodeEditorChanged', code);
                }, 500);
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

    return {
        createMonacoEditor,
        getJson,
        getText,
        registerKeyScope,
        removeItem,
        pickDesignDocument,
        saveBytesFile,
        saveDesignDocument,
        setEditorTheme,
        setMonacoTheme,
        setJson,
        setupDragAndDrop,
        setupCodeEditors,
        updateCodeEditor,
        updateJsonEditor,
        switchCodeTab,
        setupResizablePanels
    };
})();
