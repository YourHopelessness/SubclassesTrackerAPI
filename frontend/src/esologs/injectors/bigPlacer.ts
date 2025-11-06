import { setSignature } from "../iconUtils";
import { EsoSignature } from "../types";

/**
 * Places big icons and restores name.
 * @param el Element to place big icons in
 * @param wrap Wrapper element
 */
export function placeBigIconsAndRestoreName(
  el: HTMLElement,
  wrap: HTMLElement,
  sig: EsoSignature
) {
  const td = el.closest('td') as HTMLTableCellElement | null;
  if (!td) return;

  // Remove old big icons
  td.querySelectorAll(':scope > .eso-wrap.big').forEach(old => old.remove());

  // Find name and spec
  let nameSpec = td.querySelector('.name-and-spec') as HTMLElement | null;
  if (!nameSpec) {
    nameSpec = document.createElement('span');
    nameSpec.className = 'name-and-spec';
    td.appendChild(nameSpec);
  }

  // Returns the first link in the name-and-spec
  const link =
    (wrap.querySelector('a[href*="source="]') as HTMLAnchorElement | null) ||
    (el.closest('a[href*="source="]') as HTMLAnchorElement | null);
  if (link && link.parentElement !== nameSpec) {
    nameSpec.prepend(link);
  }

  // Create new icon wrapper block
  const iconBlock = document.createElement('span');
  iconBlock.className = 'eso-wrap big';
  iconBlock.setAttribute('data-eso-wrapped', '1');
  iconBlock.style.display = 'block';
  iconBlock.style.marginBottom = '4px';

  wrap.querySelectorAll('img.eso-helper-skill').forEach(img => {
    iconBlock.appendChild(img.cloneNode(true));
  });

  // Make signature
  setSignature(iconBlock, sig);

  // Remove the old wrapper
  wrap.remove();

  // Insert the new icon block
  td.insertBefore(iconBlock, nameSpec);
}