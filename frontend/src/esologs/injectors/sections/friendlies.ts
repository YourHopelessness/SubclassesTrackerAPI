import { EsoSignature } from '../../types';
import { getLookupName } from '../../nameUtils';
import { applyToElement } from '../applier';

/**
 * Process "All Friendlies" table
 * @param root The root element
 * @param map The map of signatures
 * @param seen Elements seen already
 */
export function processFriendlies(root: HTMLElement, map: Map<string, EsoSignature>, seen: Set<string>) {
  root.querySelectorAll<HTMLAnchorElement>('a.actor-menu-link, a.actor-menu-link.has-submenu').forEach(link => {
    const key = getLookupName(link);
    if (!key || seen.has(key)) return;

    const sig = map.get(key);
    if (!sig) return;

    //  replace element
    if (applyToElement(link, sig, { big: false, replaceName: true })) {
      seen.add(key);
    }
  });
}