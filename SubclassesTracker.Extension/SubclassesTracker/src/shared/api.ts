import { Tokens } from '../types/tokens';
import { getTokens, saveTokens, clearTokens } from './storage';

const BASE = 'https://localhost:7192/api';

async function refresh(tokens: Tokens): Promise<Tokens | null> {
  const res = await fetch(`${BASE}/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: "include",
    body: JSON.stringify({ refreshToken: tokens.refresh })
  });
  if (!res.ok) {
    await clearTokens();
    return null;
  }
  const j = await res.json();
  const next: Tokens = {
    access: j.access_token,
    refresh: j.refresh_token,
    exp: Date.now() + j.expires_in * 1000
  };
  await saveTokens(next);
  return next;
}

export async function apiFetch(path: string, init: RequestInit = {}) {
  let tok = await getTokens();
  if (!tok) throw new Error('No tokens stored');

  if (Date.now() > tok.exp - 60_000) {
    tok = (await refresh(tok)) ?? tok;
  }

  const headers = new Headers(init.headers);
  headers.set('Authorization', `Bearer ${tok.access}`);
  const res = await fetch(`${BASE}${path}`, { ...init, headers });

  // try once more on 401
  if (res.status === 401) {
    tok = await refresh(tok);
    if (!tok) throw new Error('Refresh failed');
    headers.set('Authorization', `Bearer ${tok.access}`);
    return fetch(`${BASE}${path}`, { ...init, headers });
  }
  return res;
}