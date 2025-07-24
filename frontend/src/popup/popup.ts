import { AUTH_LOGIN_INTERACTIVE } from '../constants';
import { getClientId, saveClientId } from '../shared/storage';
import { MsgAuthLogin } from '../types/messages';

async function init() {
  const state = document.getElementById('state')!;
  const cid = await getClientId();

  if (!cid) {
    // Ask for ID
    const tpl = (document.getElementById('askId') as HTMLTemplateElement).content.cloneNode(true) as DocumentFragment;
    state.appendChild(tpl);

    const input = state.querySelector<HTMLInputElement>('#clientId')!;
    const saveBtn = state.querySelector<HTMLButtonElement>('#save')!;

    saveBtn.addEventListener('click', async () => {
      const val = input.value.trim();
      if (!val) return;
      await saveClientId(val);
      await chrome.runtime.sendMessage(
        { type: AUTH_LOGIN_INTERACTIVE } as MsgAuthLogin
      );
      window.close();
    });
  } else {
    const tpl = (document.getElementById('loggedIn') as HTMLTemplateElement).content.cloneNode(true) as DocumentFragment;
    state.appendChild(tpl);
  }
}

init();