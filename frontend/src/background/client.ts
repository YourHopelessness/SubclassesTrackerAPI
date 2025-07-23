import { getClientId } from "../shared/storage";

/// ** Wait for clientId to be set in storage **
export function waitForClientIdFromStorage(timeoutMs = 5 * 60_000): Promise<string> {
  return new Promise((resolve, reject) => {
    let done = false;
    const finish = (ok: boolean, v?: string) => {
      if (done) return;
      done = true;
      clearTimeout(timer);
      chrome.storage.onChanged.removeListener(onChange);
      ok ? resolve(v!) : reject(new Error('Timeout waiting for clientId'));
    };

    const timer = setTimeout(() => finish(false), timeoutMs);

    async function checkNow() {
      const cid = await getClientId();
      if (cid) finish(true, cid);
    }

    function onChange(changes: Record<string, chrome.storage.StorageChange>, area: string) {
      if (area !== 'local' && area !== 'sync') return;
      const c = changes.clientId;
      if (c?.newValue) finish(true, c.newValue as string);
    }

    chrome.storage.onChanged.addListener(onChange);
    void checkNow();
  });
}