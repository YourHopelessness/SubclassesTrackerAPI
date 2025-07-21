/**
 * Content‑script for ESO Logs Helper.
 *
 * The file is injected once per tab (static in manifest and/or via
 * chrome.scripting.executeScript).  We expose a single global
 * function `window.esologsInjectLines(url)` so the background
 * worker can re‑trigger our logic whenever the SPA navigates to a
 * new report / fight.
 */

import { apiFetch } from '../shared/api';
import type { PlayerSkilllinesApiResponse } from '../types/models';
import { getTokens } from '../shared/storage';
import { HELPER_ICON_CLASS } from '../constants';

  /** Helper: build API URL and fetch skill lines for current page. */
async function getPlayersLines(
  logId: string,
  fightId?: string | null,
  bossId?: number | null,
  wipes?: number | null,
): Promise<PlayerSkilllinesApiResponse[]> {
  const params = new URLSearchParams();
  params.set('logId', logId);
  if (fightId != null) params.set('fightId', fightId);
  if (bossId != null) params.set('bossId', bossId.toString());
  if (wipes != null) params.set('wipes', wipes.toString());

  const apiUrl = `/skill/getPlayersLines?${params.toString()}`;
  console.debug('[ESO Helper] GET', apiUrl);

  const res = await apiFetch(apiUrl);
  if (!res.ok) throw new Error(res.statusText);

  return (await res.json()) as PlayerSkilllinesApiResponse[];
}

  /**
   * Exported entry point.
   * Called from the background script every time the user lands on
   * a new report URL inside the same SPA tab.
   */
export async function esologsInjectLines(url: string, tabId: number): Promise<void> {
  console.debug('[ESO Helper] esologsInjectLines →', url);

  const { pathname, searchParams } = new URL(url);
  const logId = pathname.split('/')[2];          // /reports/<logId>/...
  const fightId = searchParams.get('fight') || null;
  const wipes = Number(searchParams.get('wipes')) || null;
  const bossId = Number(searchParams.get('boss')) || null;

  if (fightId && wipes && bossId) {
    return;
  }

  const tokens = await getTokens();
  if (!tokens) {
    console.warn('[ESO Helper] No tokens – skipping API call');
    return;
  }

  try {
    const players = await getPlayersLines(logId, fightId, bossId, wipes);

    await chrome.scripting.executeScript({
      target: { tabId },
      world: 'ISOLATED',
      func: (players: PlayerSkilllinesApiResponse[]) => {
          function buildSkillUI (players: PlayerSkilllinesApiResponse[]) {
            const CLASS = 'eso-helper-skill';
            players.forEach((p) => {
              // 1. XPath – static snapshot
              const xpath = `//*[normalize-space(text()) = "${p.playerCharacterName.replace(/"/g, '\\"')}"]`;
              const snap = document.evaluate(
                xpath,
                document.body,
                null,
                XPathResult.ORDERED_NODE_SNAPSHOT_TYPE,
                null,
              );

              // 2. Loop *by index* so DOM mutations are allowed
              for (let i = 0; i < snap.snapshotLength; i += 1) {
                const el = snap.snapshotItem(i) as HTMLElement;

                /* ---- remove previous helper icons ---- */
                let prev: Node | null = el.previousSibling;
                while (
                  prev &&
                  prev.nodeType === 1 &&
                  (prev as HTMLElement).classList.contains(CLASS)
                ) {
                  const toRemove = prev;
                  prev = prev.previousSibling;
                  toRemove.parentNode?.removeChild(toRemove);
                }

                /* ---- remove ESO Logs default class icon ---- */
                if (prev && prev.nodeType === 1 && (prev as HTMLElement).tagName === 'IMG') {
                  (prev as HTMLElement).remove();
                }

                /* ---- build new fragment ---- */
                const frag = document.createDocumentFragment();
                p.playerSkillLines.forEach((sl) => {
                  const img = document.createElement('img');
                  img.src = sl.lineIcon;
                  img.title = sl.lineName;
                  img.className = CLASS;
                  img.style.width = '18px';
                  img.style.height = '18px';
                  img.style.marginRight = '2px';
                  frag.appendChild(img);
                });

                el.parentNode?.insertBefore(frag, el);
              }
            });
        };
        buildSkillUI(players);
      },
      args: [players],
    });
  } catch (e) {
    console.error('[ESO Helper] API error', e);
  }
}