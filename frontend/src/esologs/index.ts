import { injectAllActors } from './injector';
import { EsoSignature } from './types';

/**
 * Initializes the ESOlogs UI module.
 */
export function initEsologsEnhancer(actorRoot: HTMLElement, esoData: EsoSignature[]) {
  console.log('[ESOlogs enhancer] started injects');
  const map = new Map(esoData.map(e => [e.sourceId!, e]));
  injectAllActors(actorRoot, map);
}