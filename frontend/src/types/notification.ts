export type NotificationType = 'follow' | 'mention' | 'retweet'

export interface AppNotification {
  id: string
  type: NotificationType
  actorUsername: string
  actorDisplayName: string | null
  actorAvatarUrl: string | null
  tweetId?: string
  tweetText?: string
  createdAt: string
  read: boolean
}
