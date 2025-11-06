import { EsoSignature } from '../types';
import { ensureWrap, insertBeforeSafe, buildIcons, stripDefaultIcons, isSameSignature, setSignature, resolveRoot } from '../iconUtils';
import { replaceNameWithEsoId } from '../nameUtils';
import { placeBigIconsAndRestoreName } from './bigPlacer';

/**
 * Applies the replacement to the given element.
 * @param el element to apply to
 * @param sig current signature
 * @param param2 is the element big/or need to replace name
 * @returns true if applied, false if not
 */
export function applyToElement(
  el: HTMLElement, 
  sig: EsoSignature, 
  { big, replaceName }: { big: boolean; replaceName: boolean }) {
  const wrap = ensureWrap(el, big);
  if (isSameSignature(wrap, sig)) return false;

  stripDefaultIcons(resolveRoot(wrap));
  wrap.querySelectorAll('img.eso-helper-skill').forEach(n => n.remove());

  insertBeforeSafe(wrap, buildIcons(sig), wrap.firstChild);

  // big icons mode: move icons, restore name, insert @ESO on new line
  if (big) {
    placeBigIconsAndRestoreName(el, wrap, sig);
  } 
  if (replaceName && !big) {
    // regular tables/lists: replace name directly in container
    if (replaceName) replaceNameWithEsoId(el, sig.esoId);
    // add data attribute for preventing multiple injections
    setSignature(wrap, sig);
  }

  return true;
}