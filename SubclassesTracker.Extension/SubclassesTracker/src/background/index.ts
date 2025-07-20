import { getTokens } from '../shared/storage';
import { IncomingMessage, isMessage } from '../types/messages';
import { AUTH_GET_TOKENS, AUTH_LOGIN_INTERACTIVE, ESO_PAGE_READY } from '../constants';
import { ensureAuth, startInteractiveAuth } from './auth';

chrome.runtime.onMessage.addListener((msg: IncomingMessage, _sender, sendResponse) => {
  if (isMessage(msg, ESO_PAGE_READY)) {
    console.log('ESO Logs Helper SW');
    void (async () => {
      const tokens = await ensureAuth(Boolean(msg.interactive));
      sendResponse({ tokens });
    })();
    return true;
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
  return;
});

chrome.runtime.onInstalled.addListener((_details) => {
  console.log('ESO Logs Helper installed/updated');
  void ensureAuth(true);
});


