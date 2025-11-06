import { EsoSignature } from '../../types';
import { getLookupName } from '../../nameUtils';
import { applyToElement } from '../base';
import { TABLE_ACTOR_LINKS_SEL } from '../../domSelectors';

/**
 * Process tables as Damage Done / Healing Done / Deaths, etc.
 * @param root The root element
 * @param map The map of signatures
 * @param seen Elements seen already
 */
export function processTables(root: HTMLElement, map: Map<string, EsoSignature>, seen: Set<string>) {
  root.querySelectorAll<HTMLAnchorElement>(TABLE_ACTOR_LINKS_SEL).forEach(link => {
    if (link.closest('.character-details, .name-and-spec')) return;
    
    const key = getLookupName(link);
    if (!key || seen.has(key)) return;

    const sig = map.get(key);
    if (!sig) return;

    if (applyToElement(link, sig, { big: false, replaceName: true })) {
      seen.add(key);
    }
  });
}