window.designerInterop = (() => {
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
        
        // Track active drag
        let activeDrag = null;

        // Setup palette items for dragging
        const paletteItems = document.querySelectorAll('.designer-palette-item');
        paletteItems.forEach(item => {
            item.addEventListener('dragstart', (e) => {
                const componentType = item.title; // title attribute holds component type
                activeDrag = { type: 'palette', value: componentType };
                e.dataTransfer.effectAllowed = 'copy';
                e.dataTransfer.setData('text/plain', componentType);
                
                // Visual feedback
                item.style.opacity = '0.6';
                console.log(`🎯 Drag started: ${componentType}`);
            }, false);

            item.addEventListener('dragend', (e) => {
                item.style.opacity = '1';
                activeDrag = null;
                console.log(`❌ Drag ended`);
            }, false);
        });

        // Setup dropzones for dropping
        const dropzones = document.querySelectorAll('.designer-dropzone');
        dropzones.forEach((dropzone, index) => {
            dropzone.addEventListener('dragover', (e) => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'copy';
                
                // Visual feedback
                dropzone.classList.add('designer-dropzone--drag-over');
                console.log(`📍 Dragover detected on zone ${index}`);
            }, false);

            dropzone.addEventListener('dragleave', (e) => {
                // Only remove if actually leaving the element
                if (e.target === dropzone) {
                    dropzone.classList.remove('designer-dropzone--drag-over');
                }
            }, false);

            dropzone.addEventListener('drop', (e) => {
                e.preventDefault();
                e.stopPropagation();
                dropzone.classList.remove('designer-dropzone--drag-over');
                
                const componentType = e.dataTransfer.getData('text/plain');
                console.log(`✅ Drop received on zone ${index}: ${componentType}`);
                
                if (activeDrag && blazorRef) {
                    // Call Blazor method to handle the drop
                    dotnetHelper.invokeMethodAsync('OnJavaScriptDrop', componentType, index)
                        .then(() => console.log(`✨ Blazor handler completed`))
                        .catch(err => console.error(`❌ Blazor handler error:`, err));
                }
            }, false);
        });

        console.log(`✨ Drag & drop setup complete: ${paletteItems.length} palette items, ${dropzones.length} dropzones`);
    };

    let codeEditor = null;
    let jsonEditor = null;
    let codeEditorChangeTimeout = null;

    const setupCodeEditors = async (dotnetRef, codeContainer, jsonContainer) => {
        const monaco = await ensureMonaco();
        
        // Create Razor code editor
        if (codeContainer) {
            codeEditor = monaco.editor.create(codeContainer, {
                value: `<!-- Razor code will load here -->`,
                language: 'html', // Use HTML for Razor syntax highlighting
                theme: 'vs',
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                automaticLayout: true,
                tabSize: 2,
                wordWrap: 'on',
                readOnly: false
            });

            // Debounced code change handler
            codeEditor.onDidChangeModelContent(() => {
                clearTimeout(codeEditorChangeTimeout);
                codeEditorChangeTimeout = setTimeout(() => {
                    const code = codeEditor.getValue();
                    dotnetRef.invokeMethodAsync('OnCodeEditorChanged', code)
                        .catch(err => console.error('❌ Code editor sync error:', err));
                }, 500);
            });
        }

        // Create JSON model editor
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

            // JSON change handler
            jsonEditor.onDidChangeModelContent(() => {
                clearTimeout(codeEditorChangeTimeout);
                codeEditorChangeTimeout = setTimeout(() => {
                    const json = jsonEditor.getValue();
                    dotnetRef.invokeMethodAsync('OnJsonEditorChanged', json)
                        .catch(err => console.error('❌ JSON editor sync error:', err));
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

            // Trigger layout recalculation for Monaco editors
            if (codeEditor) codeEditor.layout();
            if (jsonEditor) jsonEditor.layout();
        }
    };

    const setupResizablePanels = async () => {
        // Dynamic import of resize interop
        const { setupResizablePanels: setupResize } = await import('./designer-resize-interop.js');
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
