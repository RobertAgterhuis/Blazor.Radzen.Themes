// Monaco Editor lazy loader
let monacoLoaderPromise = null;
let monacoInstance = null;

export const loadMonaco = async () => {
  if (monacoInstance) {
    return monacoInstance;
  }

  if (!monacoLoaderPromise) {
    monacoLoaderPromise = (async () => {
      // Load Monaco's loader script
      const loaderScript = document.createElement('script');
      loaderScript.src = '../../node_modules/monaco-editor/min/vs/loader.js';
      document.head.appendChild(loaderScript);

      // Wait for loader to be available
      await new Promise(resolve => {
        loaderScript.onload = resolve;
      });

      // Configure the loader
      require.config({
        paths: { vs: '../../node_modules/monaco-editor/min/vs' }
      });

      // Load Monaco
      return new Promise(resolve => {
        require(['vs/editor/editor.main'], () => {
          monacoInstance = monaco;
          resolve(monacoInstance);
        });
      });
    })();
  }

  return monacoLoaderPromise;
};

export const createEditor = async (container, options) => {
  const monaco = await loadMonaco();
  return monaco.editor.create(container, options);
};

export const createDiffEditor = async (container, options) => {
  const monaco = await loadMonaco();
  return monaco.editor.createDiffEditor(container, options);
};
