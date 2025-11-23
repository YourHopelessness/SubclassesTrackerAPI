export const NAME_ORIG_ATTR = 'data-eso-original-name';
export const NAME_DONE_ATTR = 'data-eso-name-replaced';
export const WRAP_CLASS = 'eso-wrap';
export const MY_ICON_CLASS = 'eso-helper-skill';

export interface EsoSignature {
  esoId: string;            // @UserName
  icons: string[];          // icons url
  role?: string;
  sourceId?: string;
}

export type SkillLineDto = {
  lineIcon: string;
  lineName: string;
};