export type MsgEsoPageReady = { type: 'ESO_PAGE_READY'; interactive?: boolean };
export type MsgAuthLogin     = { type: 'AUTH_LOGIN_INTERACTIVE' };
export type MsgAuthGetTokens = { type: 'AUTH_GET_TOKENS' };
export type MsgAuthRefresh   = { type: 'AUTH_REFRESH' };

export type IncomingMessage =
  | MsgEsoPageReady
  | MsgAuthLogin
  | MsgAuthGetTokens
  | MsgAuthRefresh;

export function isMessage<T extends IncomingMessage['type']>(
  msg: unknown,
  type: T,
): msg is Extract<IncomingMessage, { type: T }> {
  return typeof msg === 'object' && msg !== null && (msg as any).type === type;
}