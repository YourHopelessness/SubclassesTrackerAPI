export interface SkillLineDto {
  lineName: string;
  lineIcon: string;
}

export interface PlayerSkilllinesApiResponse {
  playerCharacterName: string;
  playerEsoId: string;
  playerSkillLines: SkillLineDto[];
}