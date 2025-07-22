// src/content/esologsInjectLines.ts

import type { PlayerSkilllinesApiResponse, SkillLineDto } from '../types/models';
import { getTokens } from '../shared/storage';
import { getCached, setCached } from './cache';
import { getPlayersLines } from './getLines';

export async function esologsInjectLines(url: string, tabId: number): Promise<void> {
  const u = new URL(url);
  const logId   = u.pathname.split('/')[2];
  const fightId = u.searchParams.get('fight') ?? '';
  const bossId  = u.searchParams.get('boss')  ?? '';
  const wipes   = u.searchParams.get('wipes') ?? '';

  const tokens = await getTokens();
  if (!tokens) return;

  const cacheKey = [logId, fightId, bossId, wipes].join('|');
  let players = await getCached<PlayerSkilllinesApiResponse[]>(cacheKey);
  if (!players) {
    players = await getPlayersLines(
      logId,
      fightId || null,
      bossId ? +bossId : null,
      wipes ? +wipes : null,
    );
    await setCached(cacheKey, players);
  }

  await chrome.scripting.executeScript({
    target: { tabId },
    world: 'ISOLATED',
    func: (playersArg: PlayerSkilllinesApiResponse[]) => {
      // ---------- bootstrap guard ----------
      // @ts-ignore
      if (window.__esoBootstrapDone) return;
      // @ts-ignore
      window.__esoBootstrapDone = true;

      // ---------- constants ----------
      const MY_ICON_CLASS    = 'eso-helper-skill';
      const WRAP_CLASS       = 'eso-wrap';
      const SIG_ATTR         = 'data-eso-sig';
      const NAME_ORIG_ATTR   = 'data-eso-original-name';
      const NAME_DONE_ATTR   = 'data-eso-name-replaced';
      const WRAPPED_ATTR     = 'data-eso-wrapped';

      // any icon that can be ESO default (they all have actor-sprite-* somewhere)
      const CANDIDATE_IMG_QS =
        'img.sprite, img.report-table-icon, img.role-spec-icon, img.composition-icon, img.tooltip';
      const MENU_LINK_SEL    = 'li.actor-menu-item > a.actor-menu-link.has-submenu';

      // ---------- CSS ----------
      const CORE_CSS = `
        img.${MY_ICON_CLASS}{
          width:18px !important;height:18px !important;
          margin:0 !important;vertical-align:middle !important;
          object-fit:contain !important;border:1px solid #555 !important;
        }
        .${WRAP_CLASS}{
          display:inline-flex !important;align-items:center !important;
          gap:3px !important;vertical-align:middle !important;
        }
        .${WRAP_CLASS}.big img.${MY_ICON_CLASS}{
          width:42px !important;height:42px !important;margin:0 3px 0 0 !important;
          vertical-align:middle !important; align-items:center !important;
        }
      `;
      ensureCss('eso-style-core', CORE_CSS);

      // ---------- utils ----------
      function ensureCss(id: string, css: string) {
        if (document.getElementById(id)) return;
        const s = document.createElement('style');
        s.id = id; s.textContent = css;
        document.head.appendChild(s);
      }

      const hasActorSprite = (el: Element) =>
        Array.prototype.some.call(el.classList, (c: string) => c.startsWith('actor-sprite-'));

      function insertBeforeSafe(parent: Node, node: Node, ref: Node | null) {
        if (!ref || ref.parentNode !== parent) parent.appendChild(node);
        else parent.insertBefore(node, ref);
      }

      function isBigIcon(img: HTMLImageElement): boolean {
        const w = img.width || parseInt(getComputedStyle(img).width || '0', 10);
        return w >= 40; // ESO big portrait ~52px
      }

      function getLookupName(el: HTMLElement): string {
        const orig = el.getAttribute(NAME_ORIG_ATTR);
        if (orig) return orig.trim();
        return (el.textContent || '').trim();
      }

      function makeSig(pl: PlayerSkilllinesApiResponse): string {
        return `${pl.playerEsoId}|${pl.playerSkillLines.map(l => l.lineIcon).join(',')}`;
      }

      function setSignature(wrap: HTMLElement, pl: PlayerSkilllinesApiResponse) {
        wrap.setAttribute(SIG_ATTR, makeSig(pl));
      }

      function isSameSignature(wrap: HTMLElement, pl: PlayerSkilllinesApiResponse) {
        return wrap.getAttribute(SIG_ATTR) === makeSig(pl);
      }

      /** Try to find the element that visually displays the name for this icon */
      function resolveNameEl(img: HTMLImageElement): HTMLElement | null {
        // 0) inside menu link
        const link = img.closest('a.actor-menu-link') as HTMLElement | null;
        if (link) {
            return link;
        }

        // 1) nested tiny table “<tr><td><img>…”
        const row = img.closest('tr');
        if (row) {
          const a =
            row.querySelector('td.tooltip.main-table-link a[href*="source="]') ||
            row.querySelector('a.actor-menu-link') ||
            row.querySelector('a[href*="source="]');
          if (a) {
            return a as HTMLElement;
          }
        }

        // 2) composition entry
        const comp = img.closest('.composition-entry') as HTMLElement | null;
        if (comp) {
          const a = comp.querySelector('a.actor-menu-link, a[href*="source="]');
          return (a as HTMLElement) || comp;
        }

        // 3) generic td
        const td = img.closest('td') as HTMLElement | null;
        if (td) {
          const a = td.querySelector('a[href*="source="], a.actor-menu-link');
          return (a as HTMLElement) || td;
        }

        // 4) fallback
        if (img.nextSibling && img.nextSibling.nodeType === Node.TEXT_NODE) {
          return img.parentElement as HTMLElement;
        }
        return null;
      }

      /** For cleanup we need a root that contains BOTH default img & name */
      function resolveRoot(wrap: HTMLElement): HTMLElement {
        return (
          wrap.closest('.composition-entry') as HTMLElement ||
          wrap.closest('tr') as HTMLElement ||
          wrap.closest('a.actor-menu-link') as HTMLElement ||
          wrap.closest('td, li.actor-menu-item') as HTMLElement ||
          wrap.parentElement ||
          wrap
        );
      }

      function stripDefaultIcons(root: HTMLElement) {
        root.querySelectorAll(`img[class*="actor-sprite-"]:not(.${MY_ICON_CLASS}), img.${MY_ICON_CLASS}`)
            .forEach(i => i.remove());
      }

      /** Replace the first text node with @esoId, keep original in attr */
      function swapVisibleName(container: HTMLElement, esoId: string) {
        if (!container.getAttribute(NAME_ORIG_ATTR)) {
          container.setAttribute(NAME_ORIG_ATTR, (container.textContent || '').trim());
        }
        if (container.getAttribute(NAME_DONE_ATTR) === '1') return;

        const tn = Array.prototype.find.call(
          container.childNodes,
          (n: Node) => n.nodeType === Node.TEXT_NODE && (n.textContent || '').trim(),
        ) as Text | undefined;

        if (tn) tn.textContent = ' ' + esoId;
        else container.appendChild(document.createTextNode(' ' + esoId));

        container.setAttribute(NAME_DONE_ATTR, '1');
      }

      function buildIcons(lines: SkillLineDto[]): DocumentFragment {
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

      /**
       * Wrap the target name into span. For menu links we must insert AFTER .sub-arrow.
       */
      function wrapName(nameEl: HTMLElement, big: boolean): HTMLElement {
        const existing = nameEl.closest('.' + WRAP_CLASS) as HTMLElement | null;
        if (existing) {
            existing.classList.toggle('big', big);

            return existing;
        }

        if (nameEl.matches('a.actor-menu-link')) {
            const link  = nameEl as HTMLAnchorElement;
            const arrow = link.querySelector('.sub-arrow');
            const wrap  = document.createElement('span');
            wrap.className = `${WRAP_CLASS} ${big ? 'big' : ''}`;

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

            link.setAttribute(WRAPPED_ATTR, '1');

            return wrap;
        }

        const wrap = document.createElement('span');
        wrap.className = `${WRAP_CLASS} ${big ? 'big' : ''}`;
        insertBeforeSafe(nameEl.parentNode as Node, wrap, nameEl);
        wrap.appendChild(nameEl);

        return wrap;
     }

      /** Single default IMG -> our icons */
      function replaceThisImg(img: HTMLImageElement, pl: PlayerSkilllinesApiResponse) {
        const big = isBigIcon(img);
        const nameEl = resolveNameEl(img);
        if (!nameEl) return;

        if (big) {
            const host = img.parentElement;
            if (host) {
                stripDefaultIcons(host);
                const wrap = document.createElement('span');
                wrap.className = `${WRAP_CLASS} big`;
                wrap.appendChild(buildIcons(pl.playerSkillLines));
                host.querySelectorAll('img.' + MY_ICON_CLASS).forEach(n => n.remove());
                insertBeforeSafe(host, wrap, host.firstChild);
                setSignature(wrap, pl);
            }

            return;
        }

        const wrap = wrapName(nameEl, false);
        if (isSameSignature(wrap, pl)) return;

        const root = resolveRoot(wrap);
        stripDefaultIcons(root);

        swapVisibleName(nameEl, pl.playerEsoId);

        wrap.querySelectorAll('img.' + MY_ICON_CLASS).forEach(n => n.remove());
        insertBeforeSafe(wrap, buildIcons(pl.playerSkillLines), wrap.firstChild);

        setSignature(wrap, pl);
      }

      /** a.actor-menu-link.has-submenu case */
      function handleMenuLinks(map: Map<string, PlayerSkilllinesApiResponse>) {
        document.querySelectorAll<HTMLAnchorElement>(MENU_LINK_SEL).forEach(link => {
          const name = getLookupName(link);
          const pl   = map.get(name);
          if (!pl) return;

          const wrap = wrapName(link, false);
          if (isSameSignature(wrap, pl)) return;

          // remove ESO + our old icons
          link.querySelectorAll('img').forEach(i => {
            if (hasActorSprite(i) || i.classList.contains(MY_ICON_CLASS)) i.remove();
          });

          swapVisibleName(link, pl.playerEsoId);

          wrap.querySelectorAll('img.' + MY_ICON_CLASS).forEach(n => n.remove());
          insertBeforeSafe(wrap, buildIcons(pl.playerSkillLines), wrap.firstChild);

          setSignature(wrap, pl);
        });
      }

      /** For places without any default img at all */
      function addWhereNoDefault(map: Map<string, PlayerSkilllinesApiResponse>) {
        document.querySelectorAll<HTMLElement>(`
          .composition-entry > a.actor-menu-link,
          .composition-entry > a[class],
          li.actor-menu-item > a.actor-menu-link,
          td a.actor-menu-link`).forEach(a => {
          const name = getLookupName(a);
          const pl   = map.get(name);
          if (!pl) return;

          const wrap = wrapName(a, false);
          if (isSameSignature(wrap, pl)) return;

          stripDefaultIcons(resolveRoot(wrap));
          swapVisibleName(a, pl.playerEsoId);

          wrap.querySelectorAll('img.' + MY_ICON_CLASS).forEach(n => n.remove());
          insertBeforeSafe(wrap, buildIcons(pl.playerSkillLines), wrap.firstChild);

          setSignature(wrap, pl);
        });
      }

      // ---------- main apply ----------
      function applySkillIcons(playersArr: PlayerSkilllinesApiResponse[]) {
        if (!playersArr?.length) return;

        const map = new Map<string, PlayerSkilllinesApiResponse>();
        playersArr.forEach(p => map.set(p.playerCharacterName.trim(), p));

        // Pass 1: every default icon (small & big)
        document.querySelectorAll<HTMLImageElement>(CANDIDATE_IMG_QS).forEach(img => {
          if (!hasActorSprite(img)) return;
          const nameEl = resolveNameEl(img);
          if (!nameEl) return;

          const name = getLookupName(nameEl);
          const pl   = map.get(name);
          if (!pl) return;

          replaceThisImg(img, pl);
        });

        // Pass 2: explicit menu links with sub-arrow
        handleMenuLinks(map);

        // Pass 3: rows without any default images
        addWhereNoDefault(map);
      }

      // ---------- expose + run ----------
      // @ts-ignore
      window.__esoPlayers = playersArg;
      // @ts-ignore
      window.__esoApply   = () => applySkillIcons(window.__esoPlayers);

      // initial run
      // @ts-ignore
      window.__esoApply();

      // observe SPA DOM changes
      let lastRun = 0;
      const observer = new MutationObserver(() => {
        const now = Date.now();
        if (now - lastRun < 150) return;
        lastRun = now;
        // @ts-ignore
        window.__esoApply();
      });
      observer.observe(document.getElementById('report-html') || document.body, {
        childList: true,
        subtree: true,
      });

      // re-apply after tab/nav clicks
      document.querySelectorAll('#table-tabs a, #navbar a').forEach(a => {
        a.addEventListener('click', () => setTimeout(() => {
          // @ts-ignore
          window.__esoApply();
        }, 0), true);
      });

      document.querySelectorAll('#table-tabs a, #navbar a, a[href*="friendlies"]').forEach(a => {
        a.addEventListener('click', () => setTimeout(() => {
            // @ts-ignore
            window.__esoApply();
        }, 0), true);
      });

      window.addEventListener('eso-location-change', () => {
        // @ts-ignore
        window.__esoApply();
      });
    },
    args: [players],
  });

  // 2) push fresh data if bootstrap was already there
  await chrome.scripting.executeScript({
    target: { tabId },
    world: 'ISOLATED',
    func: (playersArr: PlayerSkilllinesApiResponse[]) => {
      // @ts-ignore
      window.__esoPlayers = playersArr;
      // @ts-ignore
      if (typeof window.__esoApply === 'function') window.__esoApply();
    },
    args: [players],
  });
}