// background/cache.ts
interface CacheEntry<T> {
  data: T;
  expires: number;            // epoch ms
}

const RAM: Record<string, CacheEntry<any>> = {};
const TTL_MS = 5 * 60_000;    // 5 minutes

export async function getCached<T>(key: string): Promise<T | null> {
  const now = Date.now();

  // 1) in‑memory first (fast)
  const mem = RAM[key];
  if (mem && mem.expires > now) return mem.data;

  // 2) fallback to storage
  const obj = await chrome.storage.local.get(key);
  const disk = obj[key] as CacheEntry<T> | undefined;
  if (disk && disk.expires > now) {
    RAM[key] = disk;  
    return disk.data;
  }
  return null;
}

export async function setCached<T>(key: string, data: T): Promise<void> {
  const entry: CacheEntry<T> = { data, expires: Date.now() + TTL_MS };
  RAM[key] = entry;
  await chrome.storage.local.set({ [key]: entry });
}
