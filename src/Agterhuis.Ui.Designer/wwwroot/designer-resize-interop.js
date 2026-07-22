// Panel resize handler with localStorage persistence and delegated divider handling.
export const setupResizablePanels = () => {
  const STORAGE_KEY = 'designer-panel-sizes';
  const CONFIG = {
    'palette-canvas': { dir: 'vertical', min: 150, max: 400, def: 220, prop: 'palette-width', invert: false },
    'canvas-property': { dir: 'vertical', min: 200, max: 500, def: 320, prop: 'property-width', invert: true },
    'canvas-code': { dir: 'horizontal', min: 100, maxPct: 0.6, def: 250, prop: 'code-height', invert: false }
  };

  const designerLayout = document.querySelector('.designer-page');
  if (!designerLayout) {
    return;
  }

  if (designerLayout.dataset.resizeReady === 'true') {
    return;
  }

  const toStorageKey = (prop) => prop.replace(/-([a-z])/g, (_, c) => c.toUpperCase());
  const cssVarName = (prop) => `--designer-${prop}`;

  const loadSizes = () => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      return saved ? JSON.parse(saved) : {};
    } catch {
      return {};
    }
  };

  const saveSizes = (sizes) => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(sizes));
    } catch {
      // Best effort only; resize should still function without persistence.
    }
  };

  const saved = loadSizes();
  for (const cfg of Object.values(CONFIG)) {
    const key = toStorageKey(cfg.prop);
    const value = Number.isFinite(saved[key]) ? saved[key] : cfg.def;
    designerLayout.style.setProperty(cssVarName(cfg.prop), `${value}px`);
  }

  designerLayout.addEventListener('mousedown', (event) => {
    const target = event.target;
    if (!(target instanceof Element) || event.button !== 0) {
      return;
    }

    const divider = target.closest('.designer-divider[data-divider]');
    if (!(divider instanceof HTMLElement)) {
      return;
    }

    const dividerType = divider.getAttribute('data-divider');
    if (!dividerType) {
      return;
    }

    const cfg = CONFIG[dividerType];
    if (!cfg) {
      return;
    }

    event.preventDefault();

    const isHorizontal = cfg.dir === 'horizontal';
    const varName = cssVarName(cfg.prop);
    const startPos = isHorizontal ? event.clientY : event.clientX;
    const currentSize = parseInt(designerLayout.style.getPropertyValue(varName), 10);
    const startSize = Number.isFinite(currentSize) ? currentSize : cfg.def;
    const maxSize = cfg.maxPct ? Math.round(window.innerHeight * cfg.maxPct) : cfg.max;
      const minPanel = Math.max(160, cfg.min ?? 160);
      const maxPanel = Math.max(minPanel, Math.min(600, maxSize ?? 600));

    const overlay = document.createElement('div');
    overlay.style.cssText = `position:fixed;inset:0;z-index:10000;cursor:${isHorizontal ? 'row-resize' : 'col-resize'}`;
    document.body.appendChild(overlay);
    divider.style.backgroundColor = 'var(--agt-color-primary-500)';

    const onMove = (moveEvent) => {
      const delta = isHorizontal ? moveEvent.clientY - startPos : moveEvent.clientX - startPos;
      const adjusted = cfg.invert ? startSize - delta : startSize + delta;
        const clamped = Math.max(minPanel, Math.min(maxPanel, adjusted));
      designerLayout.style.setProperty(varName, `${clamped}px`);
    };

    const onUp = () => {
      overlay.remove();
      divider.style.backgroundColor = '';

      const size = parseInt(designerLayout.style.getPropertyValue(varName), 10);
      const sizes = loadSizes();
      sizes[toStorageKey(cfg.prop)] = Number.isFinite(size) ? size : cfg.def;
      saveSizes(sizes);

      document.removeEventListener('mousemove', onMove);
      document.removeEventListener('mouseup', onUp);
    };

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup', onUp);
  });

  designerLayout.dataset.resizeReady = 'true';
};
