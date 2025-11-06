import { EsoSignature, SkillLineDto } from './types';
import { MY_ICON_CLASS, WRAP_CLASS, WRAPPED_ATTR, SIG_ATTR } from './css';

export function hasActorSprite(el: Element) {
  return Array.prototype.some.call(
    el.classList,
    (c: string) => c.startsWith('actor-sprite-')
  );
}

export function insertBeforeSafe(parent: Node, node: Node, ref: Node | null) {
  if (!ref || ref.parentNode !== parent) parent.appendChild(node);
  else parent.insertBefore(node, ref);
}

export function isBigIcon(img: HTMLImageElement): boolean {
  const w = img.width || parseInt(getComputedStyle(img).width || '0', 10);
  return w >= 40; // 40px is the minimum width for the big icon
}

export function buildIconsFromLines(lines: SkillLineDto[]): DocumentFragment {
  const frag = document.createDocumentFragment();
  for (const sl of lines) {
    const ic = document.createElement('img');
    ic.src = sl.lineIcon;
    ic.title = sl.lineName;
    ic.className = MY_ICON_CLASS;
    frag.appendChild(ic);
  }
  return frag;
}

export function buildIcons(sig: EsoSignature): DocumentFragment {
  const frag = document.createDocumentFragment();
  for (const url of sig.icons) {
    const ic = document.createElement('img');
    ic.src = url;
    ic.className = MY_ICON_CLASS;
    frag.appendChild(ic);
  }
  return frag;
}

export function ensureWrap(nameEl: HTMLElement, big = false): HTMLElement {
  if (nameEl.classList.contains(WRAP_CLASS) && nameEl.hasAttribute(WRAPPED_ATTR)) {
    nameEl.classList.toggle('big', big);
    return nameEl;
  }
  if (nameEl.parentElement?.classList.contains(WRAP_CLASS) &&
      nameEl.parentElement?.hasAttribute(WRAPPED_ATTR)) {
    nameEl.parentElement.classList.toggle('big', big);
    return nameEl.parentElement;
  }
  const existingChildWrapper = Array.from(nameEl.children).find(
    ch => ch.classList.contains(WRAP_CLASS) && (ch as HTMLElement).hasAttribute(WRAPPED_ATTR)
  ) as HTMLElement | undefined;
  if (existingChildWrapper) {
    existingChildWrapper.classList.toggle('big', big);
    return existingChildWrapper;
  }

  const wrap = document.createElement('span');
  wrap.className = `${WRAP_CLASS} ${big ? 'big' : ''}`;
  wrap.setAttribute(WRAPPED_ATTR, '1');

  if (nameEl.matches('a.actor-menu-link')) {
    const link = nameEl as HTMLAnchorElement;
    const arrow = link.querySelector('.sub-arrow');
    if (arrow) {
      arrow.after(wrap);
      while (wrap.nextSibling && wrap.nextSibling !== arrow) {
        wrap.appendChild(wrap.nextSibling);
      }
    } else {
      link.prepend(wrap);
      while (wrap.nextSibling) {
        wrap.appendChild(wrap.nextSibling);
      }
    }
  } else {
    insertBeforeSafe(nameEl.parentNode!, wrap, nameEl);
    wrap.appendChild(nameEl);
  }
  return wrap;
}

export function setSignature(wrap: HTMLElement, sig: EsoSignature) {
  wrap.setAttribute(SIG_ATTR, `${sig.esoId}|${sig.icons.join(',')}`);
}
export function isSameSignature(wrap: HTMLElement, sig: EsoSignature) {
  return wrap.getAttribute(SIG_ATTR) === `${sig.esoId}|${sig.icons.join(',')}`;
}

export function stripDefaultIcons(root: HTMLElement) {
  root
    .querySelectorAll(`img[class*="actor-sprite-"]:not(.${MY_ICON_CLASS}), img.${MY_ICON_CLASS}`)
    .forEach(i => i.remove());
}

export function resolveRoot(wrap: HTMLElement): HTMLElement {
  return (
    (wrap.closest('.composition-entry') as HTMLElement) ||
    (wrap.closest('tr') as HTMLElement) ||
    (wrap.closest('a.actor-menu-link') as HTMLElement) ||
    (wrap.closest('td, li.actor-menu-item') as HTMLElement) ||
    wrap.parentElement ||
    wrap
  );
}