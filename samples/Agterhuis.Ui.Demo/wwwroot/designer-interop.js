window.designerInterop = (() => {
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
        getJson,
        getText,
        registerKeyScope,
        removeItem,
        pickDesignDocument,
        saveBytesFile,
        saveDesignDocument,
        setJson
    };
})();
