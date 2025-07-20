import { apiFetch } from '../shared/api';
import type { PlayerSkilllinesApiResponse } from '../types/models';
import { getTokens } from '../shared/storage';

/**
 * GET /skill/getPlayersLines?logId=...&fightId=...&bossId=...
 * Returns typed response.
 */
export async function getPlayersLines(
  logId: string,
  fightId?: number | null,
  bossId?: number | null): 
      Promise<PlayerSkilllinesApiResponse[]> {
  const params = new URLSearchParams();
  params.set('logId', logId);
  if (fightId != null) params.set('fightId', fightId.toString());
  if (bossId != null) params.set('bossId', bossId.toString());

  const res = await apiFetch(`/skill/getPlayersLines?${params.toString()}`);
  if (!res.ok) throw new Error(res.statusText);

  return (await res.json()) as PlayerSkilllinesApiResponse[];
}

(async () => {
  const { pathname, searchParams } = new URL(location.href);
  const logId = pathname.split('/')[2]; 
  const fightId = Number(searchParams.get('fight')) || null;
  const bossId = Number(searchParams.get('boss')) || null;
  var tokens = getTokens();
  if (!tokens) {
    try {
      const players = await getPlayersLines(logId, fightId, bossId);
      console.table(players);
    } catch (e) {
      console.error('[ESO-Parser]', e);
    }
  }
})();