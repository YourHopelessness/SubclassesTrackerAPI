export function resolveNameEl(img: HTMLImageElement): HTMLElement | null {
  const link = img.closest('a.actor-menu-link') as HTMLElement | null;
  if (link) return link;

  const row = img.closest('tr');
  if (row) {
    const a =
      row.querySelector('td.tooltip.main-table-link a[href*="source="]') ||
      row.querySelector('a.actor-menu-link') ||
      row.querySelector('a[href*="source="]');
    if (a) return a as HTMLElement;
  }

  const comp = img.closest('.composition-entry') as HTMLElement | null;
  if (comp) {
    const a = comp.querySelector('a.actor-menu-link, a[href*="source="]');
    return (a as HTMLElement) || comp;
  }

  const td = img.closest('td') as HTMLElement | null;
  if (td) {
    const a = td.querySelector('a[href*="source="], a.actor-menu-link');
    return (a as HTMLElement) || td;
  }

  if (img.nextSibling && img.nextSibling.nodeType === Node.TEXT_NODE) {
    return img.parentElement as HTMLElement;
  }
  return null;
}