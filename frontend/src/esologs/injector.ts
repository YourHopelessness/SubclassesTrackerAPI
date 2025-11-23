import { EsoSignature } from './types';
import { ensureCss, CORE_CSS } from './css';
import { processCharacters } from './injectors/sections/characters';
import { processComposition } from './injectors/sections/composition';
import { processFriendlies } from './injectors/sections/friendlies';
import { processTables } from './injectors/sections/tables';

/**
 * Injects ESO data (icons, names) into all actor elements in the report.
 */
export function injectAllActors(root: HTMLElement, map: Map<string, EsoSignature>) {
  ensureCss('eso-style-core', CORE_CSS);
  const seen = new Set<string>();

  processTables(root, map, seen);
  processCharacters(root, map, seen);
  processComposition(root, map, seen);
  processFriendlies(root, map, seen);

  console.debug(`[ESOlogs enhancer] Applied icons to ${seen.size} actors`);
}