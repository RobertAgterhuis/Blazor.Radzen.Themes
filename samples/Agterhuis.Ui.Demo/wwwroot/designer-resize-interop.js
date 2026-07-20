// Panel resize handler with localStorage persistence
export const setupResizablePanels = () => {
  const STORAGE_KEY = 'designer-panel-sizes';
  const MIN_PALETTE_WIDTH = 150;
  const MAX_PALETTE_WIDTH = 400;
  const DEFAULT_PALETTE_WIDTH = 220;

  const MIN_PROPERTY_WIDTH = 200;
  const MAX_PROPERTY_WIDTH = 500;
  const DEFAULT_PROPERTY_WIDTH = 320;

  const MIN_CODE_HEIGHT = 100;
  const MAX_CODE_HEIGHT_PCT = 0.6;
  const DEFAULT_CODE_HEIGHT = 250;

  const MIN_CANVAS_HEIGHT = 200;

  // Load saved sizes or use defaults
  const loadSizes = () => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) {
        return JSON.parse(saved);
      }
    } catch (e) {
      console.warn('Failed to load panel sizes:', e);
    }
    return {
      paletteWidth: DEFAULT_PALETTE_WIDTH,
      propertyWidth: DEFAULT_PROPERTY_WIDTH,
      codeHeight: DEFAULT_CODE_HEIGHT
    };
  };

  const saveSizes = (sizes) => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(sizes));
    } catch (e) {
      console.warn('Failed to save panel sizes:', e);
    }
  };

  const applySizes = (sizes) => {
    const designerLayout = document.querySelector('.designer-page');
    if (!designerLayout) return;

    // Apply via CSS variables that the layout will use
    designerLayout.style.setProperty('--designer-palette-width', sizes.paletteWidth + 'px');
    designerLayout.style.setProperty('--designer-property-width', sizes.propertyWidth + 'px');
    designerLayout.style.setProperty('--designer-code-height', sizes.codeHeight + 'px');
  };

  const setupDivider = (divider, direction, minSize, maxSize, defaultSize, sizeProperty, layoutElement) => {
    if (!divider) return;

    let isResizing = false;
    let startPos = 0;
    let startSize = 0;

    // Convert property name: 'palette-width' -> '--designer-palette-width'
    const cssVarName = '--designer-' + sizeProperty;
    
    // Convert to camelCase for storage: 'palette-width' -> 'paletteWidth'
    const storageKey = sizeProperty.replace(/-([a-z])/g, (g) => g[1].toUpperCase());

    const createOverlay = () => {
      const overlay = document.createElement('div');
      overlay.style.position = 'fixed';
      overlay.style.top = '0';
      overlay.style.left = '0';
      overlay.style.right = '0';
      overlay.style.bottom = '0';
      overlay.style.zIndex = '10000';
      overlay.style.cursor = direction === 'horizontal' ? 'row-resize' : 'col-resize';
      return overlay;
    };

    const startResize = (e) => {
      if (e.button !== 0) return; // Only left mouse button
      isResizing = true;
      startPos = direction === 'horizontal' ? e.clientY : e.clientX;
      startSize = parseInt(layoutElement.style.getPropertyValue(cssVarName)) || defaultSize;

      const overlay = createOverlay();
      document.body.appendChild(overlay);

      const handleMove = (moveEvent) => {
        if (!isResizing) return;

        const delta = direction === 'horizontal'
          ? moveEvent.clientY - startPos
          : moveEvent.clientX - startPos;

        let newSize = startSize + delta;
        newSize = Math.max(minSize, Math.min(maxSize, newSize));

        layoutElement.style.setProperty(cssVarName, newSize + 'px');
        divider.style.backgroundColor = 'var(--agt-color-primary-500)';
      };

      const handleEnd = (endEvent) => {
        isResizing = false;
        try {
          if (overlay && overlay.parentNode) {
            document.body.removeChild(overlay);
          }
        } catch (e) {
          console.warn('Failed to remove overlay:', e);
        }
        divider.style.backgroundColor = '';
        divider.style.width = '';
        divider.style.height = '';

        // Save new size with correct storage key
        const newSize = parseInt(layoutElement.style.getPropertyValue(cssVarName));
        const sizes = loadSizes();
        sizes[storageKey] = newSize;
        saveSizes(sizes);

        document.removeEventListener('mousemove', handleMove);
        document.removeEventListener('mouseup', handleEnd);
      };

      document.addEventListener('mousemove', handleMove);
      document.addEventListener('mouseup', handleEnd);
    };

    // Hover effect
    divider.addEventListener('mouseenter', () => {
      if (!isResizing) {
        divider.style.backgroundColor = 'var(--agt-color-primary-500)';
      }
    });

    divider.addEventListener('mouseleave', () => {
      if (!isResizing) {
        divider.style.backgroundColor = '';
      }
    });

    // Double-click to reset
    divider.addEventListener('dblclick', (e) => {
      e.stopPropagation();
      const sizes = loadSizes();
      sizes[storageKey] = defaultSize;
      saveSizes(sizes);
      applySizes(sizes);
    });

    divider.addEventListener('mousedown', startResize);
  };

  // Initialize
  const sizes = loadSizes();
  const layoutElement = document.querySelector('.designer-page');

  if (layoutElement) {
    applySizes(sizes);

    // Setup dividers
    const leftDivider = document.querySelector('[data-divider="palette-canvas"]');
    const rightDivider = document.querySelector('[data-divider="canvas-property"]');
    const bottomDivider = document.querySelector('[data-divider="canvas-code"]');

    setupDivider(leftDivider, 'vertical', MIN_PALETTE_WIDTH, MAX_PALETTE_WIDTH, DEFAULT_PALETTE_WIDTH, 'palette-width', layoutElement);
    setupDivider(rightDivider, 'vertical', MIN_PROPERTY_WIDTH, MAX_PROPERTY_WIDTH, DEFAULT_PROPERTY_WIDTH, 'property-width', layoutElement);
    setupDivider(bottomDivider, 'horizontal', MIN_CODE_HEIGHT, Math.floor(window.innerHeight * MAX_CODE_HEIGHT_PCT), DEFAULT_CODE_HEIGHT, 'code-height', layoutElement);
  }

  return { loadSizes, saveSizes, applySizes };
};
