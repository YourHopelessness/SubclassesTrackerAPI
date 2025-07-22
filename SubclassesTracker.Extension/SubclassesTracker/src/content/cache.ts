// ** Cache **

interface CacheEntry<T> { data: T; expires: number }
const RAM: Record<string, CacheEntry<any>> = {};
const TTL = 5 * 60_000; // 5 min

export async function getCached<T>(key: string): Promise<T | null> {
  const now = Date.now();
  const mem = RAM[key];
  if (mem && mem.expires > now) return mem.data;
  const obj = await chrome.storage.local.get(key);
  const disk = obj[key] as CacheEntry<T> | undefined;
  if (disk && disk.expires > now) { RAM[key] = disk; return disk.data; }
  return null;
}
export async function setCached<T>(key: string, data: T) {
  const entry: CacheEntry<T> = { data, expires: Date.now() + TTL };
  RAM[key] = entry;
  await chrome.storage.local.set({ [key]: entry });
}
