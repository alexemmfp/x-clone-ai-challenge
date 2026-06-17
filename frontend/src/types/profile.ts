export interface Profile {
  id: string
  username: string
  email: string
  bio: string | null
  avatarUrl: string | null
  followerCount: number
  followingCount: number
  isFollowedByViewer: boolean
}
