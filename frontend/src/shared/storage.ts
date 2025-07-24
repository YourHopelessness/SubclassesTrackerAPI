import { Tokens } from "../types/tokens";

export function saveClientId(id: string) {
  return chrome.storage.local.set({ clientId: id });
}

export function clearClientId() {
  chrome.storage.local.remove(['clientId'], () => {
    console.log('Client ID resetted');
  });
}

export async function getClientId(): Promise<string | null> {
  const { clientId } = await chrome.storage.local.get('clientId');
  return clientId ?? null;
}

export function saveTokens(t: Tokens) {
  return chrome.storage.local.set({ tokens: t });
}

export async function getTokens(): Promise<Tokens | null> {
  const { tokens } = await chrome.storage.local.get('tokens');
  return tokens ?? null;
}

export function clearTokens() {
  return chrome.storage.local.remove(['tokens']);
}
