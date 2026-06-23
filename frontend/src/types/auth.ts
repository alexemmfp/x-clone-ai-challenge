export interface AuthResult {
  accessToken: string
  userId: string
  username: string
}

export interface UserProfile {
  id: string
  username: string
  avatarUrl?: string | null
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
