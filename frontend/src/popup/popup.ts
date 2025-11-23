import { AUTH_LOGIN_INTERACTIVE } from '../constants';
import {
  getClientId,
  saveClientId,
  clearClientId,
  clearTokens,
} from '../shared/storage';
import { MsgAuthLogin } from '../types/messages';

async function renderLoading(message: string) {
  const state = document.getElementById('state')!;
  state.innerHTML = `<div id="loading">${message}</div>`;
}

async function renderAskId() {
  const state = document.getElementById('state')!;
  state.innerHTML = '';

  const tpl = (document.getElementById('askId') as HTMLTemplateElement)
    .content.cloneNode(true) as DocumentFragment;
  state.appendChild(tpl);

  const input = state.querySelector<HTMLInputElement>('#clientId')!;
  const saveBtn = state.querySelector<HTMLButtonElement>('#save')!;

  saveBtn.addEventListener('click', async () => {
    const val = input.value.trim();
    if (!val) return;

    await saveClientId(val);
    await renderLoading('Waiting for login...');
    await chrome.runtime.sendMessage({ type: AUTH_LOGIN_INTERACTIVE } as MsgAuthLogin);
  });
}

async function renderLoggedIn() {
  const state = document.getElementById('state')!;
  state.innerHTML = '';

  const tpl = (document.getElementById('loggedIn') as HTMLTemplateElement)
    .content.cloneNode(true) as DocumentFragment;
  state.appendChild(tpl);

  const cidLabel = state.querySelector<HTMLElement>('#clientIdLabel')!;
  const cid = await getClientId();
  cidLabel.textContent = `Client ID: ${cid ?? '(unknown)'}`;

  const resetBtn = state.querySelector<HTMLButtonElement>('#reset')!;
  const reloginBtn = state.querySelector<HTMLButtonElement>('#relogin')!;

  resetBtn.addEventListener('click', async () => {
    await renderLoading('Clearing data...');
    clearClientId();
    clearTokens();
    await renderAskId();
  });

  reloginBtn.addEventListener('click', async () => {
    await renderLoading('Re-login in progress...');
    await chrome.runtime.sendMessage({ type: AUTH_LOGIN_INTERACTIVE } as MsgAuthLogin);
  });
}

async function init() {
  await renderLoading('Checking client ID...');
  const cid = await getClientId();
  if (cid) await renderLoggedIn();
  else await renderAskId();

  // слушаем события от background
  chrome.runtime.onMessage.addListener((msg) => {
    if (msg?.type === 'AUTH_TOKEN_SAVED') renderLoggedIn();
    if (msg?.type === 'AUTH_FAILED') renderAskId();
  });
}

init();