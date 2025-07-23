import { API } from '../constants';
import { Tokens } from '../types/tokens';
import { getTokens, saveTokens, clearTokens } from './storage';

async function refresh(tokens: Tokens): Promise<Tokens | null> {
  const res = await fetch(`${API}/auth/refresh`, {
    method: 'POST',
    credentials: "include",
    body: JSON.stringify({ refreshToken: tokens.refreshToken })
  });
  if (!res.ok) {
    await clearTokens();
    return null;
  }
  const j = await res.json();
  const next: Tokens = {
    accessToken: j.access_token,
    refreshToken: j.refresh_token,
    expiresIn: j.expires_in
  };
  await saveTokens(next);
  return next;
}

export async function apiFetch(path: string, init: RequestInit = {}) {
  let tok = await getTokens();
  if (!tok) throw new Error('No tokens stored');

  const headers = new Headers(init.headers);
  headers.set('Authorization', `Bearer ${tok.accessToken}`);
  const res = await fetch(`${API}${path}`, { ...init, headers });

  // try once more on 401
  if (res.status === 401) {
    tok = await refresh(tok);
    if (!tok) throw new Error('Refresh failed');
    headers.set('Authorization', `Bearer ${tok.accessToken}`);
    return fetch(`${API}${path}`, { ...init, headers });
  }
  return res;
}