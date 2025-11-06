import { EsoSignature } from '../../types';
import { getLookupName } from '../../nameUtils';
import { applyToElement } from '../applier';

/**
 * Process "Characters" table
 * @param root Root element
 * @param map Map of signatures
 * @param seen Elements seen already
 */
export function processCharacters(root: HTMLElement, map: Map<string, EsoSignature>, seen: Set<string>) {
  root.querySelectorAll<HTMLElement>('.character-details a, .summary-actors-table a').forEach(link => {
    const key = getLookupName(link);
    if (!key || seen.has(key)) return;

    const sig = map.get(key);
    if (!sig) return;

    if (applyToElement(link, sig, { big: true, replaceName: false })) {
      seen.add(key);
    }
  });
}