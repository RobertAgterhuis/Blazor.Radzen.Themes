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

    return {
        createMonacoEditor,
        getJson,
        getText,
        registerKeyScope,
        removeItem,
        pickDesignDocument,
        saveBytesFile,
        saveDesignDocument,
        setMonacoTheme,
        setJson
    };
})();
