import { injectAllActors } from '../esologs/injector';
import type { EsoSignature } from '../esologs/types';

// Give the outside world a way to call the injection
declare global {
  interface Window {
    initEsologsEnhancer?: (root: HTMLElement, data: Map<string, EsoSignature>) => void;
    __ESO_DATA__?: Map<string, EsoSignature>;
  }
}

window.initEsologsEnhancer = (root, data) => {
  console.debug('[ESOlogs enhancer] Inject start...');
  window.__ESO_DATA__ = data;
  injectAllActors(root, data);
  setupObservers(root);
};

/**
 * Initialize MutationObserver to watch for DOM changes SPA pages.
 * @param root 
 */
function setupObservers(root: HTMLElement) {
  let isInjecting = false;
  let injectTimer: number | null = null;

  const observer = new MutationObserver(() => {
    if (isInjecting) return;
    if (injectTimer) clearTimeout(injectTimer);
    injectTimer = window.setTimeout(runInjection, 400); // debounce
  });

  const runInjection = () => {
    if (isInjecting || !window.__ESO_DATA__) return;
    isInjecting = true;

    try {
      // Switch off, when first injections happens
      observer.disconnect();

      const t0 = performance.now();
      injectAllActors(root, window.__ESO_DATA__!);
      const dt = (performance.now() - t0).toFixed(1);
      console.debug(`[ESOlogs enhancer] injectAllActors finished in ${dt} ms`);
    } catch (err) {
      console.error('[ESOlogs enhancer] injectAllActors error', err);
    } finally {
      isInjecting = false;
      // Switch on after short pause
      setTimeout(() => observer.observe(root, { childList: true, subtree: true }), 200);
    }
  };

  // First init
  observer.observe(root, { childList: true, subtree: true });

  // SPA navigation
  let lastHref = location.href;
  const navObs = new MutationObserver(() => {
    const currentHref = location.href;
    if (currentHref !== lastHref) {
      lastHref = currentHref;
      setTimeout(() => {
        if (window.__ESO_DATA__) {
          const target = document.querySelector<HTMLElement>('#report-html')
             ?? document.body;
          injectAllActors(target, window.__ESO_DATA__!);
        }
      }, 200);
    }
  });
  navObs.observe(document, { childList: true, subtree: true });
}