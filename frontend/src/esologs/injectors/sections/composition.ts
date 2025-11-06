import { EsoSignature } from '../../types';
import { getLookupName } from '../../nameUtils';
import { applyToElement } from '../applier';

/**
 * Process the composition section.
 * @param root  The root element of the composition section.
 * @param map Map of signatures.
 * @param seen Set of seen keys.
 */
export function processComposition(root: HTMLElement, map: Map<string, EsoSignature>, seen: Set<string>) {
  root.querySelectorAll<HTMLSpanElement>('.composition-entry').forEach(entry => {
    const link = entry.querySelector('a');
    if (!link) return;

    const key = getLookupName(link);
    if (!key || seen.has(key)) return;

    const sig = map.get(key);
    if (!sig) return;

    if (applyToElement(link, sig, { big: false, replaceName: true })) {
      seen.add(key);
    }
  });
}