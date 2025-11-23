// src/content/esologsInjectLines.ts

import type { PlayerSkilllinesApiResponse } from '../types/models';
import { getTokens } from '../shared/storage';
import { getCached, setCached } from './cache';
import { getPlayersLines } from './getLines';
import { EsoSignature } from '../esologs/types';

/**
 * Injects skill lines into ESO logs page.
 * @param url Url of the page
 * @param tabId Tab ID of the page
 * @returns empty promise
 */
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

  // Convert data to EsoSignature format for new module
  const esoData: EsoSignature[] = players.map(p => ({
    esoId: p.playerEsoId,
    icons: p.playerSkillLines.map(l => l.lineIcon),
    sourceId: String(p.playerCharacterName),
  }));

  const entries = esoData
    .filter((s): s is EsoSignature & { sourceId: string } => !!s.sourceId)
    .map(s => [s.sourceId, s] as [string, EsoSignature]);
    
  await chrome.scripting.executeScript({
    target: { tabId },
    func: (entries: [string, EsoSignature][]) => {
      const map = new Map<string, EsoSignature>(entries);
      const root = document.querySelector('#report-html') ?? document.body;
      // @ts-ignore
      window.initEsologsEnhancer?.(root, map);
    },
    args: [entries],
  });
}