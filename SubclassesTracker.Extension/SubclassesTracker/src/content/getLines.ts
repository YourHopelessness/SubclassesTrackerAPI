import { apiFetch } from "../shared/api";
import { PlayerSkilllinesApiResponse } from "../types/models";

/** Helper: build API URL and fetch skill lines for current page. */
export async function getPlayersLines(
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
