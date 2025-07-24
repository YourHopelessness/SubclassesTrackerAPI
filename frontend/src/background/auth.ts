import { API } from '../constants';
import { clearClientId, getClientId, getTokens, saveTokens } from '../shared/storage';
import type { Tokens } from '../types/tokens';
import { waitForClientIdFromStorage } from './client';

let authInFlight: Promise<Tokens | null> | null = null;

// Check if the user is already authenticated
export async function ensureAuth(interactive: boolean): Promise<Tokens | null> {
  const existing = await getTokens();
  if (existing) return existing;
  if (!interactive) return null;

  return startInteractiveAuth();
}

// Start interactive auth flow
export async function startInteractiveAuth(): Promise<Tokens | null> {
  if (authInFlight) return authInFlight;
  authInFlight = doInteractiveAuth().finally(() => {
    authInFlight = null;
  });
  return authInFlight;
}

// Do the interactive auth flow
async function doInteractiveAuth(): Promise<Tokens | null> {
  let clientId = await getClientId();
  if (!clientId) {
    chrome.action.openPopup();
    try {
      clientId = await waitForClientIdFromStorage();
    } catch (err) {
      console.error('[auth] clientId timeout', err);
      clearClientId();

      return null;
    }
  }

  const redirectUri = chrome.identity.getRedirectURL();
  const oauthUrlEndpoint =
    `${API}/oauth/url?clientId=${encodeURIComponent(clientId)}&redirectUri=${encodeURIComponent(redirectUri)}`;
  
  let oauthUrl: string;
  try {
    const r = await fetch(oauthUrlEndpoint);
    if (!r.ok) throw new Error('oauth/url HTTP ' + r.status);
    const j = await r.json();
    oauthUrl = j.url;
  } catch (err) {
    console.error('[auth] failed to get oauth url', err);
    clearClientId();

    return null;
  }

  let finalUrl: string | undefined;
  try {
    finalUrl = await chrome.identity.launchWebAuthFlow({ url: oauthUrl, interactive: true });
  } catch (err) {
    console.error('[auth] launchWebAuthFlow error', err);
    clearClientId();

    return null;
  }
  if (!finalUrl) {
    console.error('[auth] empty redirect');
    clearClientId();

    return null;
  }

  const code = extractAuthCode(finalUrl);
  if (!code) {
    console.error('[auth] no code in redirect');

    return null;
  }

  try {
    const tokenResp = await fetch(`${API}/oauth/exchange`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Code: code, RedirectUri: redirectUri, ClientId: clientId }),
    });

    if (!tokenResp.ok) throw new Error('exchange HTTP ' + tokenResp.status);
    const raw = await tokenResp.json();
    const tokens = normalizeTokens(raw);
    await saveTokens(tokens);
    return tokens;
  } catch (err) {
    console.error('[auth] exchange failed', err);
    clearClientId();

    return null;
  }
}

// *** Helpers ***
function normalizeTokens(raw: any): Tokens {
  const now = Date.now();
  const expiresAt = raw.expires_at
    ? Number(raw.expires_at)
    : raw.expires_in
    ? now + Number(raw.expires_in) * 1000
    : undefined;
  return {
    accessToken: raw.access_token ?? raw.AccessToken ?? raw.token ?? '',
    refreshToken: raw.refresh_token ?? raw.RefreshToken,
    expiresAt,
    ...raw,
  };
}

function extractAuthCode(url: string): string | null {
  try {
    const u = new URL(url);
    let code = u.searchParams.get('code');
    if (code) return code;
    if (u.hash) {
      const hashParams = new URLSearchParams(u.hash.startsWith('#') ? u.hash.slice(1) : u.hash);
      code = hashParams.get('code');

      if (code) return code;
    }
  } catch {
    clearClientId();
  }

  return null;
}