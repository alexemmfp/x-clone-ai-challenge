export interface AuthResult {
  accessToken: string
  user: UserProfile
}

export interface UserProfile {
  id: string
  username: string
  email: string
  bio: string | null
  avatarUrl: string | null
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}
