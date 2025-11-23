import { WRAP_CLASS, NAME_ORIG_ATTR, NAME_DONE_ATTR } from './css';

/**
 * Get the name to lookup
 * @param el Element to extract the name from
 * @returns Name to lookup
 */
export function getLookupName(el: HTMLElement): string {
  const saved = el.getAttribute(NAME_ORIG_ATTR);
  if (saved) return saved.trim();

  const cloned = el.cloneNode(true) as HTMLElement;
  cloned.querySelectorAll('.sub-arrow').forEach(n => n.remove());
  return (cloned.textContent || '').replace(/\s+/g, ' ').trim();
}

/**
 * Replace the name with the ESO ID
 * @param container Container element
 * @param esoId Eso ID
 */
export function replaceNameWithEsoId(container: HTMLElement, esoId: string) {
  if (!container.getAttribute(NAME_ORIG_ATTR)) {
    const txt = (container.textContent || '').trim();
    container.setAttribute(NAME_ORIG_ATTR, txt);
  }

  if (container.getAttribute(NAME_DONE_ATTR) === '1') return;

  let textHost: HTMLElement | null = null;

  if (container.classList.contains('has-submenu')) {
    textHost = container.querySelector(':scope > span.eso-wrap');
  }
  if (!textHost) {
    textHost =
      (container.querySelector(`:scope > span.${WRAP_CLASS}`) as HTMLElement) ||
      container;
  }

  if (!textHost) return;

  // remove old text nodes
  textHost.childNodes.forEach(n => {
    if (n.nodeType === Node.TEXT_NODE) n.remove();
  });

  // insert new ESO id
  textHost.appendChild(document.createTextNode(' ' + esoId));
  container.setAttribute(NAME_DONE_ATTR, '1');

  // for All Friendlies / Players Table
  const abilityMenuTd = container.closest('tr')?.querySelector('.ability-menu-id');
  if (abilityMenuTd) {
    const originalName = container.getAttribute(NAME_ORIG_ATTR) || '';
    abilityMenuTd.textContent = originalName;
  }
}