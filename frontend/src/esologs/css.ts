export const MY_ICON_CLASS = 'eso-helper-skill';
export const WRAP_CLASS = 'eso-wrap';
export const NAME_ORIG_ATTR = 'data-eso-original-name';
export const NAME_DONE_ATTR = 'data-eso-name-replaced';
export const WRAPPED_ATTR = 'data-eso-wrapped';
export const CHARACTER_DETAILS = 'character-details';
export const SIG_ATTR = 'data-eso-sig';

export const CORE_CSS = `
  img.${MY_ICON_CLASS}{
    width:18px !important;height:18px !important;
    margin:0 !important;vertical-align:middle !important;
    object-fit:contain !important;border:1px solid #555 !important;
  }
  .${WRAP_CLASS}{
    display:inline-flex !important;align-items:center !important;
    gap:3px !important;vertical-align:middle !important;
  }
  .${WRAP_CLASS}.big img.${MY_ICON_CLASS}{
    width:44px !important;height:44px !important;margin:0 3px 0 0 !important;
    vertical-align:middle !important; align-items:center !important;
  }
`;

export function ensureCss(id: string, css: string) {
  if (document.getElementById(id)) return;
  const s = document.createElement('style');
  s.id = id;
  s.textContent = css;
  document.head.appendChild(s);
}