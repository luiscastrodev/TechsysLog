export interface AuthenticationResponse {
  isSuccess: boolean;
  data: {
    userId: string;
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiresAt: string;
  };
  message?: string;
}
