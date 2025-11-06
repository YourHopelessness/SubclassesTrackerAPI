/**
 * Background / service‑worker entry point.
 * Listens for SPA navigation on *.esologs.com/reports/*
 * and injects the compiled content‑script. 
 */

import { getTokens } from '../shared/storage';
import {
  AUTH_GET_TOKENS,
  AUTH_LOGIN_INTERACTIVE,
  ESO_PAGE_READY,
} from '../constants';
import type { IncomingMessage } from '../types/messages';
import { isMessage } from '../types/messages';
import { ensureAuth, startInteractiveAuth } from './auth';
import { esologsInjectLines } from '../content/esologs';

const REPORT_MATCHER = [
  { hostSuffix: 'esologs.com', pathPrefix: '/reports/' },
];

/* ------------------------------------------------------------------ */
/* RUNTIME MESSAGING – TOKEN / LOGIN HANDLERS                         */
/* ------------------------------------------------------------------ */
chrome.runtime.onMessage.addListener(
  (msg: IncomingMessage, _sender, sendResponse) => {
    if (isMessage(msg, ESO_PAGE_READY)) {
      console.log('[ESO Helper] ESO_PAGE_READY received');
      void (async () => {
        const tokens = await ensureAuth(Boolean(msg.interactive));
        sendResponse({ tokens });
      })();
      return true; // async response
    }

    if (isMessage(msg, AUTH_LOGIN_INTERACTIVE)) {
      void (async () => {
        const tokens = await startInteractiveAuth();
        sendResponse({ tokens });
      })();
      return true;
    }

    if (isMessage(msg, AUTH_GET_TOKENS)) {
      void (async () => {
        const tokens = await getTokens();
        sendResponse({ tokens });
      })();
      return true;
    }

    // any other message – ignore
    return;
  },
);

/* ------------------------------------------------------------------ */
/* SPA NAVIGATION LISTENERS – inject content script on every visit    */
/* ------------------------------------------------------------------ */
chrome.webNavigation.onHistoryStateUpdated.addListener(
  (d) => {
    if (d.frameId !== 0) return; 
    void esologsInjectLines(d.url, d.tabId);
  },
  { url: REPORT_MATCHER },
);

chrome.webNavigation.onCompleted.addListener(
  async (d) => {
    if (d.frameId !== 0) return;
    await esologsInjectLines(d.url, d.tabId);
  },
  { url: REPORT_MATCHER },
);

/* ------------------------------------------------------------------ */
/* One‑time installation hook                                         */
/* ------------------------------------------------------------------ */
chrome.runtime.onInstalled.addListener(() => {
  console.log('[ESO Helper] installed / updated');
  void ensureAuth(false); // silent token check, no popup on install
});