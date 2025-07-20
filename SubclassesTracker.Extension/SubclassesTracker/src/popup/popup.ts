import { getClientId, saveClientId } from '../shared/storage';

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
      window.close();
    });
  } else {
    const tpl = (document.getElementById('loggedIn') as HTMLTemplateElement).content.cloneNode(true) as DocumentFragment;
    state.appendChild(tpl);

    state.querySelector('#reAuth')?.addEventListener('click', () => {
      chrome.runtime.sendMessage({ type: 'RE_AUTH' });
      window.close();
    });
  }
}

init();